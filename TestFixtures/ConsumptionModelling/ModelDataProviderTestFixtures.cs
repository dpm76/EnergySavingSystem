
using System;
using ConsumptionModelling;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class ModelDataProviderTestFixtures
    {
        private ModelDataProvider _modelDataProvider;

        [SetUp]
        public void Init()
        {
            this._modelDataProvider = new ModelDataProvider();
            this._modelDataProvider.Models.Add("test",
                                               new YearLevel
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
                                                   }
                );
        }

        [Test]
        public void GetMultipleDataTest()
        {
            float[] data = this._modelDataProvider.GetData("test",
                new DateTime(2011, 4, 4, 7, 0, 0, DateTimeKind.Utc),
                new DateTime(2011, 4, 4, 9, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(12, data[0]);
            Assert.AreEqual(32, data[1]);
        }

        [Test]
        public void GetSingleDataTest()
        {
            float[] data1 = this._modelDataProvider.GetData("test",
                new DateTime(2011, 4, 4, 7, 0, 0, DateTimeKind.Utc),
                new DateTime(2011, 4, 4, 8, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(12, data1[0]);
            
            float[] data2 = this._modelDataProvider.GetData("test",
                new DateTime(2011, 4, 4, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2011, 4, 4, 9, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(32, data2[0]);
        }

        [Test]
        public void GetQDataTest()
        {
            float[] data = this._modelDataProvider.GetData("test",
                new DateTime(2011, 4, 4, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2011, 4, 4, 8, 15, 0, DateTimeKind.Utc));
            Assert.AreEqual(32, data[0]);
        }
    }
}