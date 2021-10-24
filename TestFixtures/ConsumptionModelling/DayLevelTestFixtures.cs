using System;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class DayLevelTestFixtures
    {
        [Test]
        public void GetHourlyValuesTest()
        {
            DayLevel dayLevel = new DayLevel();
            dayLevel.Ranges = new []
                                  {
                                      new ConsumptionRange{Since = 0, Until = 8, Value = 1f},
                                      new ConsumptionRange{Since = 8, Until = 14, Value = 2f},
                                      new ConsumptionRange{Since = 14, Until = 15, Value = 3f},
                                      new ConsumptionRange{Since = 15, Until = 18, Value = 4f},
                                      new ConsumptionRange{Since = 18, Until = 20, Value = 5f},
                                      new ConsumptionRange{Since = 20, Until = 24, Value = 6f}
                                  };

            float[] data = dayLevel.GetHourlyValues(2, 6);
            Assert.IsTrue(Array.TrueForAll(data, (x => (x == 1f))));
            data = dayLevel.GetHourlyValues(6, 10);
            float[] d1 = new float[2];
            float[] d2 = new float[2];
            Array.Copy(data, 0, d1, 0, 2);
            Array.Copy(data, 2, d2, 0, 2);
            Assert.IsTrue(Array.TrueForAll(d1, (x => (x == 1f))));
            Assert.IsTrue(Array.TrueForAll(d2, (x => (x == 2f))));
            data = dayLevel.GetHourlyValues(14, 22);
            d1 = new float[1];
            d2 = new float[3];
            float[] d3 = new float[2];
            float[] d4 = new float[2];
            Array.Copy(data, 0, d1, 0, 1);
            Array.Copy(data, 1, d2, 0, 3);
            Array.Copy(data, 4, d3, 0, 2);
            Array.Copy(data, 6, d4, 0, 2);
            Assert.IsTrue(Array.TrueForAll(d1, (x => (x == 3f))));
            Assert.IsTrue(Array.TrueForAll(d2, (x => (x == 4f))));
            Assert.IsTrue(Array.TrueForAll(d3, (x => (x == 5f))));
            Assert.IsTrue(Array.TrueForAll(d4, (x => (x == 6f))));
        }

        [Test]
        public void GetAllValuesTest()
        {
            DayLevel dayLevel = new DayLevel();
            dayLevel.Ranges = new []
                                  {
                                      new ConsumptionRange{Since = 0, Until = 8, Value = 1f},
                                      new ConsumptionRange{Since = 8, Until = 14, Value = 2f},
                                      new ConsumptionRange{Since = 14, Until = 15, Value = 3f},
                                      new ConsumptionRange{Since = 15, Until = 18, Value = 4f},
                                      new ConsumptionRange{Since = 18, Until = 20, Value = 5f},
                                      new ConsumptionRange{Since = 20, Until = 24, Value = 6f}
                                  };

            float[] data = dayLevel.GetAllHourlyValues();
            Assert.AreEqual(24, data.Length);
            
            float[] d1 = new float[8];
            float[] d2 = new float[6];
            float[] d3 = new float[1];
            float[] d4 = new float[3];
            float[] d5 = new float[2];
            float[] d6 = new float[4];

            Array.Copy(data, 0, d1, 0, 8);
            Array.Copy(data, 8, d2, 0, 6);
            Array.Copy(data, 14, d3, 0, 1);
            Array.Copy(data, 15, d4, 0, 3);
            Array.Copy(data, 18, d5, 0, 2);
            Array.Copy(data, 20, d6, 0, 4);

            Assert.IsTrue(Array.TrueForAll(d1, (x => (x == 1f))));
            Assert.IsTrue(Array.TrueForAll(d2, (x => (x == 2f))));
            Assert.IsTrue(Array.TrueForAll(d3, (x => (x == 3f))));
            Assert.IsTrue(Array.TrueForAll(d4, (x => (x == 4f))));
            Assert.IsTrue(Array.TrueForAll(d5, (x => (x == 5f))));
            Assert.IsTrue(Array.TrueForAll(d6, (x => (x == 6f))));
        }

        [Test]
        public void InsertRangeInto()
        {
            DayLevel dayLevel = new DayLevel
                                    {
                                        Ranges = new[] {new ConsumptionRange {Since = 0, Until = 24, Value = 1f}}
                                    };
            dayLevel.AddConsumptionRange(new ConsumptionRange{Since = 9, Until = 18, Value = 3f});

            Assert.AreEqual(3, dayLevel.Ranges.Length);
            Assert.AreEqual(new ConsumptionRange {Since = 0, Until = 9, Value = 1f}, dayLevel.Ranges[0]);
            Assert.AreEqual(new ConsumptionRange {Since = 9, Until = 18, Value = 3f}, dayLevel.Ranges[1]);
            Assert.AreEqual(new ConsumptionRange {Since = 18, Until = 24, Value = 1f}, dayLevel.Ranges[2]);
        }

        [Test]
        public void InsertRangeOver()
        {
            DayLevel dayLevel = new DayLevel
                                    {
                                        Ranges = new[]
                                                     {
                                                         new ConsumptionRange {Since = 0, Until = 8, Value = 1f},
                                                         new ConsumptionRange {Since = 8, Until = 16, Value = 2f},
                                                         new ConsumptionRange {Since = 16, Until = 24, Value = 3f}
                                                     }
                                    };
            dayLevel.AddConsumptionRange(new ConsumptionRange { Since = 6, Until = 18, Value = 4f });

            Assert.AreEqual(3, dayLevel.Ranges.Length);
            Assert.AreEqual(new ConsumptionRange { Since = 0, Until = 6, Value = 1f }, dayLevel.Ranges[0]);
            Assert.AreEqual(new ConsumptionRange { Since = 6, Until = 18, Value = 4f }, dayLevel.Ranges[1]);
            Assert.AreEqual(new ConsumptionRange { Since = 18, Until = 24, Value = 3f }, dayLevel.Ranges[2]);
        }

        [Test]
        public void InsertBetween()
        {
            DayLevel dayLevel = new DayLevel
            {
                Ranges = new[]
                                                     {
                                                         new ConsumptionRange {Since = 0, Until = 12, Value = 1f},
                                                         new ConsumptionRange {Since = 12, Until = 24, Value = 2f}
                                                     }
            };
            dayLevel.AddConsumptionRange(new ConsumptionRange { Since = 6, Until = 18, Value = 3f });

            Assert.AreEqual(3, dayLevel.Ranges.Length);
            Assert.AreEqual(new ConsumptionRange { Since = 0, Until = 6, Value = 1f }, dayLevel.Ranges[0]);
            Assert.AreEqual(new ConsumptionRange { Since = 6, Until = 18, Value = 3f }, dayLevel.Ranges[1]);
            Assert.AreEqual(new ConsumptionRange { Since = 18, Until = 24, Value = 2f }, dayLevel.Ranges[2]);
        }
    }
}
