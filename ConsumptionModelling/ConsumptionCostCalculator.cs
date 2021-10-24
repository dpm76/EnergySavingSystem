using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerCostListSerializer = ConsumptionModelling.GenericSerializer<System.Collections.Generic.List<ConsumptionModelling.PowerCost>>;

namespace ConsumptionModelling
{
    /// <summary>
    /// Calcula el coste del consumo
    /// </summary>
    public class ConsumptionCostCalculator
    {
        public IConsumptionDataProvider ConsumptionDataProvider { get; set; }
        public ModelDataProvider PreviousModelDataProvider { get; set; }
        public ModelDataProvider SavingModelDataProvider { get; set; }

        
        public bool Load(string path)
        {
            if (File.Exists(path))
            {
                this.PowerCosts = PowerCostListSerializer.Deserialize(path);
            }

            return ((this.PowerCosts != null) && (this.PowerCosts.Count > 0));
        }

        public void Save(string path)
        {
            PowerCostListSerializer.Serialize(path, this.PowerCosts);
        }

        /// <summary>
        /// Costes de la energía por periodos
        /// </summary>
        public List<PowerCost> PowerCosts{ get; set; }

        /// <summary>
        /// Introduce un nuevo coste de manera ordenada
        /// </summary>
        /// <param name="newPowerCost"></param>
        public void AddPowerCost(PowerCost newPowerCost)
        {
            if(this.PowerCosts == null)
            {
                this.PowerCosts = new List<PowerCost>();
            }

            if(this.PowerCosts.Count == 0)
            {
                this.PowerCosts.Add(PowerCost.Dummy);
            }

            if (this.PowerCosts.Count > 0)
            {
                foreach (PowerCost cost in this.PowerCosts.ToArray())
                {
                    if ((cost.Since > newPowerCost.Since) && (cost.Until < newPowerCost.Until))
                    {
                        this.PowerCosts.Remove(cost);
                    }
                    else if ((cost.Since == newPowerCost.Since) && (cost.Until == newPowerCost.Until))
                    {
                        this.PowerCosts[this.PowerCosts.IndexOf(cost)] = newPowerCost;
                    }
                    else if ((newPowerCost.Since > cost.Since) && (newPowerCost.Until < cost.Until))
                    {
                        PowerCost splitPowerCost1 = cost;
                        PowerCost splitPowerCost2 = cost;
                        splitPowerCost2.Until = newPowerCost.Since;
                        splitPowerCost1.Since = newPowerCost.Until;
                        int i = this.PowerCosts.IndexOf(cost);
                        this.PowerCosts[i] = splitPowerCost2;
                        this.PowerCosts.Insert(i + 1, newPowerCost);
                        this.PowerCosts.Insert(i + 2, splitPowerCost1);
                    }
                    else if ((newPowerCost.Since > cost.Since) && (newPowerCost.Until == cost.Until))
                    {
                        int costIndex = this.PowerCosts.IndexOf(cost);
                        PowerCost temp = cost;
                        temp.Until = newPowerCost.Since;
                        this.PowerCosts[costIndex] = temp;
                        this.PowerCosts.Insert(costIndex + 1, newPowerCost);
                    }
                    else if ((newPowerCost.Until > cost.Since) && (newPowerCost.Until <= cost.Until))
                    {
                        int costIndex = this.PowerCosts.IndexOf(cost);
                        PowerCost temp = cost;
                        temp.Since = newPowerCost.Until;
                        this.PowerCosts[costIndex] = temp;
                        this.PowerCosts.Insert(costIndex, newPowerCost);
                    }
                    else if ((newPowerCost.Since >= cost.Since) && (newPowerCost.Since < cost.Until))
                    {
                        int costIndex = this.PowerCosts.IndexOf(cost);
                        PowerCost temp = cost;
                        temp.Until = newPowerCost.Since;
                        this.PowerCosts[costIndex] = temp;
                    }
                }
            }else
            {
                this.PowerCosts.Add(newPowerCost);
            }
        }

        /// <summary>
        /// Calcula el coste de la energía ya consumida
        /// </summary>
        /// <param name="since">desde</param>
        /// <param name="until">hasta</param>
        /// <param name="sourceId">Identificador de la fuente de consumo</param>
        /// <returns></returns>
        public float CommitedConsumptionCost(string sourceId, DateTime since, DateTime until)
        {
            return this.SumPowerCost(sourceId, since, until, SumCommitedPower);
        }

        private float SumCommitedPower(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            return this.ConsumptionDataProvider.GetData(sourceId, since, until, startPeriodHour, endPeriodHour)
                .Sum(new Func<float?, float>(ConvertNullableFloat2Float));
        }

        private static float ConvertNullableFloat2Float(float? value)
        {
            return (value.HasValue) ? value.Value : 0f;
        }

        private delegate float SumPowerDelegate(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour);

        /// <summary>
        /// Calcula el coste de la energía según una función proveedora de consumo
        /// </summary>
        /// <param name="since">desde</param>
        /// <param name="until">hasta</param>
        /// <param name="sourceId">Identificador de la fuente de consumo</param>
        /// <param name="sumPowerDelegate">Función para calcular la suma de energía</param>
        /// <returns></returns>
        private float SumPowerCost(string sourceId, DateTime since, DateTime until, SumPowerDelegate sumPowerDelegate)
        {
            float cost = 0f;
            PowerCost[] powerCosts = this.GetCostDefinitions(since, until);
            foreach (PowerCost powerCost in powerCosts)
            {
                DateTime[] dstSwitches = GetDstSwitches(powerCost.Since, powerCost.Until);
                DateTime startDstInterval = powerCost.Since;
                int i = 0;
                do
                {
                    DateTime endDstInterval = ((dstSwitches != null) && (dstSwitches.Length > i))
                                                  ? dstSwitches[i]
                                                  : powerCost.Until;

                    CostPeriod[] costPeriods =
                        TimeZoneInfo.Local.IsDaylightSavingTime(endDstInterval)
                            ? powerCost.SummerCostPeriods
                            : powerCost.WinterCostPeriods;

                    cost += costPeriods.Sum(costPeriod => sumPowerDelegate(sourceId, startDstInterval, endDstInterval, costPeriod.Since, costPeriod.Until)*costPeriod.Price);
                    i++;
                } while ((dstSwitches != null) && (i <= dstSwitches.Length));
            }

            return cost;
        }

        /// <summary>
        /// Obtiene el coste unitario por periodos
        /// </summary>
        /// <param name="since">desde</param>
        /// <param name="until">hasta</param>
        /// <returns></returns>
        public PowerCost[] GetCostDefinitions(DateTime since, DateTime until)
        {
            List<PowerCost> costList = new List<PowerCost>();

            if ((this.PowerCosts != null) && (this.PowerCosts.Count > 0) && (since < this.PowerCosts[this.PowerCosts.Count - 1].Until))
            {
                if (since < this.PowerCosts[0].Since)
                {
                    PowerCost powerCost = PowerCost.Dummy;
                    powerCost.Since = since;
                    powerCost.Until = (until < this.PowerCosts[0].Since)?until:this.PowerCosts[0].Since;
                    costList.Add(powerCost);

                    since = this.PowerCosts[0].Since;
                }

                if (until > this.PowerCosts[0].Since)
                {
                    int i = 0;

                    //Buscar primer periodo
                    while ((i < this.PowerCosts.Count) && (since >= this.PowerCosts[i].Until))
                    {
                        ++i;
                    }

                    //Abarcar periodos hasta que se termine el periodo solicitado
                    do
                    {
                        DateTime periodUntil = (until <= this.PowerCosts[i].Until) ? until : this.PowerCosts[i].Until;
                        costList.Add(new PowerCost
                                         {
                                             Since = since,
                                             Until = periodUntil,
                                             WinterCostPeriods = this.PowerCosts[i].WinterCostPeriods,
                                             SummerCostPeriods = this.PowerCosts[i].SummerCostPeriods
                                         });

                        since = periodUntil;
                        ++i;
                    } while ((i < this.PowerCosts.Count) && (since < until));

                    if(since < until)
                    {
                        PowerCost powerCost = PowerCost.Dummy;
                        powerCost.Since = since;
                        powerCost.Until = until;
                        costList.Add(powerCost);
                    }
                }
            }else
            {
                PowerCost powerCost = PowerCost.Dummy;
                powerCost.Since = since;
                powerCost.Until = until;
                costList.Add(powerCost);
            }

            return costList.ToArray();
        }

        /// <summary>
        /// Estima el coste del cosumo.
        /// Si coincide con un periodo que ya se ha consumido, 
        /// se utiliza el consumo realizado,
        /// para el periodo sin información de consumo se utiliza el modelo
        /// de ahorrro.
        /// </summary>
        /// <param name="since">desde</param>
        /// <param name="until">hasta</param>
        /// <param name="sourceId">Identificador de la fuente de consumo</param>
        /// <returns></returns>
        public float EstimateCost(string sourceId, DateTime since, DateTime until)
        {
            float totalCost;

            DateTime lastReadingTime = this.ConsumptionDataProvider.GetLastDataTimeStamp(sourceId);
            if(lastReadingTime >= until)
            {
                totalCost = this.CommitedConsumptionCost(sourceId, since, until);
            }
            else if (lastReadingTime <= since)
            {
                totalCost = this.EstimateCostFromModel(sourceId, since, until, true);
            }
            else
            {
                //Estimar coste de consumo realizado
                totalCost = this.CommitedConsumptionCost(sourceId, since, lastReadingTime);

                //Estimar coste de consumo aún no realizado
                totalCost += this.EstimateCostFromModel(sourceId, lastReadingTime, until, true);
            }

            return totalCost;
        }

        /// <summary>
        /// Estima la cantidad que se va a ahorrar el periodo.
        /// Si hay datos reales, utiliza estos, si no, supone que el consumo será el del modelo de ahorro.
        /// Esto se resta del modelo anterior a la implantación para calcular el ahorro.
        /// </summary>
        /// <param name="since"></param>
        /// <param name="until"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public float EstimateSaving(string sourceId, DateTime since, DateTime until)
        {
            float previousCost = this.EstimateCostFromModel(sourceId, since, until, false);
            float currentCost = this.EstimateCost(sourceId, since, until);

            return previousCost - currentCost;
        }
        
        /// <summary>
        /// Obtiene el coste del consumo estimado según un modelo
        /// </summary>
        /// <param name="since">desde</param>
        /// <param name="until">hasta</param>
        /// <param name="sourceId">Identificador de la fuente de consumo</param>
        /// <param name="savingModel">Indica si se utilizará el modelo de ahorro</param>
        /// <returns></returns>
        public float EstimateCostFromModel(string sourceId, DateTime since, DateTime until, bool savingModel)
        {
            return this.SumPowerCost(sourceId, since, until, (savingModel ? (SumPowerDelegate)SumSavingModelPower : SumPreviousModelPower));
        }

        private float SumSavingModelPower(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            return this.SavingModelDataProvider.GetData(sourceId, since, until, startPeriodHour, endPeriodHour).Sum();
        }

        private float SumPreviousModelPower(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            return this.PreviousModelDataProvider.GetData(sourceId, since, until, startPeriodHour, endPeriodHour).Sum();
        }

        private static DateTime[] GetDstSwitches(DateTime since, DateTime until)
        {
            since = new DateTime(since.Year, since.Month, since.Day, 12, 0, 0, DateTimeKind.Local);
            List<DateTime> switches = new List<DateTime>();
            bool dst = TimeZoneInfo.Local.IsDaylightSavingTime(since);
            for (DateTime date = since; date < until; date = date.AddDays(1d) )
            {
                if(dst != TimeZoneInfo.Local.IsDaylightSavingTime(date))
                {
                    switches.Add(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime());
                    dst = !dst;
                }
            }

            return switches.ToArray();
        }
    }
}
