using System;
using ConsumptionDaemon;

namespace EnergyConsumptionConsole
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                ConsumptionManager.Default.Start();
                Console.WriteLine("Pulsa intro para detener");
                Console.ReadLine();
                ConsumptionManager.Default.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}
