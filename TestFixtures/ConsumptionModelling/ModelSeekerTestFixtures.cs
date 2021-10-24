using System;
using System.Text;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class ModelSeekerTestFixtures
    {
        private readonly YearLevel _model = new YearLevel
                                      {
                                          DefaultDayModel =
                                              {
                                                  Ranges = new[]
                                                               {
                                                                   new ConsumptionRange
                                                                       {
                                                                           Since = 0,
                                                                           Until = 8,
                                                                           Value = 12
                                                                       },
                                                                   new ConsumptionRange
                                                                       {
                                                                           Since = 8,
                                                                           Until = 17,
                                                                           Value = 32
                                                                       },
                                                                   new ConsumptionRange
                                                                       {
                                                                           Since = 17,
                                                                           Until = 20,
                                                                           Value = 24
                                                                       },
                                                                   new ConsumptionRange
                                                                       {
                                                                           Since = 20,
                                                                           Until = 24,
                                                                           Value = 12
                                                                       }
                                                               }
                                              }
                                      };
                

        [TestCase(2011, 2, 19, Result = 3)]
        [TestCase(2011, 2, 1, Result = 1)]
        [TestCase(2011, 1, 31, Result = 6)]
        [TestCase(2011, 2, 28, Result = 5)]
        public int MonthWeekNumberTest(int year, int month, int day)
        {
            DateTime dateTime = new DateTime(year, month, day);
            return ModelSeeker.GetMonthWeekNumber(dateTime);
        }

        [Test]
        public void GetSavingModel()
        {
            float[] values = ModelSeeker.GetHourlyValuesInPeriodFromModel(
                this._model,
                new DateTime(2011, 7, 28, 8, 19, 0, DateTimeKind.Utc),
                new DateTime(2011, 7, 28, 12, 19, 0, DateTimeKind.Utc),
                0, 24);

            float[] expected = new float[] {32, 32, 32, 32, 32};

            Assert.IsTrue(ArraysAreEqual(values, expected), string.Format("Se esperaba {0} y se ha obtenido {1}", Array2String(expected), Array2String(values)));
        }

        private bool ArraysAreEqual(float[] v1, float[] v2)
        {
            int i = 0;
            if (v1.Length == v2.Length)
            {
                while ((i < v1.Length) && (v1[i] == v2[i])) ++i;
            }

            return ((v1.Length == v2.Length) && (i == v1.Length));
        }
        
        private string Array2String(float[] v)
        {
            StringBuilder sb = new StringBuilder();
            foreach (float f in v)
            {
                sb.Append(f + "; ");
            }

            return sb.ToString();
        }
    }
}
