using System;
using System.Collections.Generic;
using ConsumptionModelling;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class ConsumptionCostCalculatorTestFixtures
    {
        private readonly ConsumptionCostCalculator _calculator = new ConsumptionCostCalculator();

        [SetUp]
        public void Init()
        {
            this._calculator.ConsumptionDataProvider = new FakeConsumptionDataProvider();
            PowerCost[] powerCosts =
                new[]
                    {
                        new PowerCost
                            {
                                Since = new DateTime(2011, 1, 1),
                                Until = new DateTime(2011, 2, 1),
                                WinterCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 1f} },
                                SummerCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 1f} }
                            },
                        new PowerCost
                            {
                                Since = new DateTime(2011, 2, 1),
                                Until = new DateTime(2011, 4, 1),
                                WinterCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 2f} },
                                SummerCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 2f} }
                            },
                        new PowerCost
                            {
                                Since = new DateTime(2011, 4, 1),
                                Until = new DateTime(2011, 10, 1),
                                WinterCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 3f} },
                                SummerCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 3f} }
                            },
                        new PowerCost
                            {
                                Since = new DateTime(2011, 10, 1),
                                Until = new DateTime(2012, 1, 1),
                                WinterCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 4f} },
                                SummerCostPeriods = new []{new CostPeriod{Since = 0, Until = 24, Price = 4f} }
                            }
                    };

            this._calculator.PowerCosts = new List<PowerCost>(powerCosts);

            YearLevel previousModel =
                new YearLevel
                    {
                        DefaultDayModel =
                            new DayLevel
                                {
                                    Ranges =
                                        new[]
                                            {
                                                new ConsumptionRange
                                                    {
                                                        Since = 0,
                                                        Until = 8,
                                                        Value = 4f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 8,
                                                        Until = 12,
                                                        Value = 10f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 12,
                                                        Until = 14,
                                                        Value = 6f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 14,
                                                        Until = 18,
                                                        Value = 10f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 18,
                                                        Until = 20,
                                                        Value = 8f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 20,
                                                        Until = 24,
                                                        Value = 4f
                                                    }
                                            }
                                }
                    };

            YearLevel savingModel =
                new YearLevel
                    {
                        DefaultDayModel =
                            new DayLevel
                                {
                                    Ranges =
                                        new[]
                                            {
                                                new ConsumptionRange
                                                    {
                                                        Since = 0,
                                                        Until = 8,
                                                        Value = 2f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 8,
                                                        Until = 12,
                                                        Value = 5f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 12,
                                                        Until = 14,
                                                        Value = 3f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 14,
                                                        Until = 18,
                                                        Value = 5f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 18,
                                                        Until = 20,
                                                        Value = 4f
                                                    },
                                                new ConsumptionRange
                                                    {
                                                        Since = 20,
                                                        Until = 24,
                                                        Value = 2f
                                                    }
                                            }
                                }
                    };

            this._calculator.PreviousModelDataProvider = new ModelDataProvider();
            this._calculator.PreviousModelDataProvider.Models.Add("test", previousModel);
            this._calculator.SavingModelDataProvider = new ModelDataProvider();
            this._calculator.SavingModelDataProvider.Models.Add("test", savingModel);
        }

        [Test]
        public void GetCostPeriodsOverFirstTest()
        {
            PowerCost []powerCosts =
                this._calculator.GetCostDefinitions(new DateTime(2010, 12, 1), new DateTime(2011, 6, 21));

            Assert.AreEqual(4, powerCosts.Length);
            Assert.AreEqual(
                new PowerCost
                    {
                        Since = new DateTime(2010, 12, 1),
                        Until = new DateTime(2011, 1, 1),
                        SummerCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 0f},},
                        WinterCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 0f},}
                    }, powerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                    {
                        Since = new DateTime(2011, 1, 1),
                        Until = new DateTime(2011, 2, 1),
                        SummerCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 1f},},
                        WinterCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 1f},}
                    },
                powerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                    {
                        Since = new DateTime(2011, 2, 1),
                        Until = new DateTime(2011, 4, 1),
                        SummerCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 2f},},
                        WinterCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 2f},}
                    }, powerCosts[2]);
            Assert.AreEqual(
                new PowerCost
                    {
                        Since = new DateTime(2011, 4, 1),
                        Until = new DateTime(2011, 6, 21),
                        SummerCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 3f},},
                        WinterCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 3f},}
                    }, powerCosts[3]);
        }

        [Test]
        public void GetCostPeriodsOverLastTest()
        {
            PowerCost[] powerCosts =
                this._calculator.GetCostDefinitions(new DateTime(2011, 12, 1), new DateTime(2012, 6, 21));

            Assert.AreEqual(2, powerCosts.Length);

            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 12, 1),
                    Until = new DateTime(2012, 1, 1),
                    SummerCostPeriods = new[]{ new CostPeriod { Since = 0, Until = 24, Price = 4f }, },
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 4f }, }
                }, powerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2012, 1, 1),
                    Until = new DateTime(2012, 6, 21),
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, },
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, }
                },
                powerCosts[1]);
        }

        [Test]
        public void GetCostPeriodsBeforeDefinedRange()
        {
            PowerCost[] powerCosts =
                this._calculator.GetCostDefinitions(new DateTime(2009, 1, 1), new DateTime(2009, 6, 21));

            Assert.AreEqual(1, powerCosts.Length);

            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2009, 1, 1),
                    Until = new DateTime(2009, 6, 21),
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, },
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, }
                }, powerCosts[0]);

        }

        [Test]
        public void GetCostPeriodsAfterDefinedRange()
        {
            PowerCost[] powerCosts =
                this._calculator.GetCostDefinitions(new DateTime(2013, 1, 1), new DateTime(2013, 6, 21));

            Assert.AreEqual(1, powerCosts.Length);

            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2013, 1, 1),
                    Until = new DateTime(2013, 6, 21),
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, },
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f }, }
                }, powerCosts[0]);

        }

        [Test]
        public void EstimateCostFromModelTest()
        {
            DateTime since0 = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime until0 = new DateTime(2011, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            float savingCost0 = this._calculator.EstimateCostFromModel("test", since0, until0, true);
            float previousCost0 = this._calculator.EstimateCostFromModel("test", since0, until0, false);

            Assert.AreEqual(6786, savingCost0);
            Assert.AreEqual(13572, previousCost0);
        }

        [Test]
        public void EstimateCostFromModelFinishingBorderPeriod()
        {
            DateTime since1 = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime until1 = new DateTime(2011, 2, 1, 0, 0, 0, DateTimeKind.Utc);

            float savingCost1 = this._calculator.EstimateCostFromModel("test", since1, until1, true);
            float previousCost1 = this._calculator.EstimateCostFromModel("test", since1, until1, false);
            Assert.AreEqual(2418, savingCost1);
            Assert.AreEqual(4836, previousCost1);
        }

        [Test]
        public void EstimateCostFromModelStartingBorderPeriod()
        {
            DateTime since2 = new DateTime(2011, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime until2 = new DateTime(2011, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            float savingCost2 = this._calculator.EstimateCostFromModel("test", since2, until2, true);
            float previousCost2 = this._calculator.EstimateCostFromModel("test", since2, until2, false);
            Assert.AreEqual(4368, savingCost2);
            Assert.AreEqual(8736, previousCost2);
        }

        [Test]
        public void EstimateCostTest()
        {
            DateTime since = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime until = new DateTime(2011, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            FakeConsumptionDataProvider dataProvider =
                (FakeConsumptionDataProvider) this._calculator.ConsumptionDataProvider;
            dataProvider.LastDataTimeStamp = new DateTime(2011, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            float cost = this._calculator.EstimateCost("test", since, until);

            Assert.AreEqual(dataProvider.Ac + 4368, cost);
        }

        [Test]
        public void EstimateSavingTest()
        {
            DateTime since = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime until = new DateTime(2011, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            FakeConsumptionDataProvider dataProvider =
                (FakeConsumptionDataProvider) this._calculator.ConsumptionDataProvider;
            dataProvider.LastDataTimeStamp = new DateTime(2011, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            float saving = this._calculator.EstimateSaving("test", since, until);

            Assert.AreEqual(13572 - dataProvider.Ac - 4368, saving);
        }

        [Test]
        public void AddCostInMiddle()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 2, 15),
                Until = new DateTime(2011, 3, 15),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
            });

            Assert.AreEqual(6, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                    {
                        Since = new DateTime(2011, 2, 1),
                        Until = new DateTime(2011, 2, 15),
                        WinterCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 2f}},
                        SummerCostPeriods = new[] {new CostPeriod {Since = 0, Until = 24, Price = 2f}}
                    },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 2, 15),
                    Until = new DateTime(2011, 3, 15),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[2]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 3, 15),
                    Until = new DateTime(2011, 4, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } }
                },
                this._calculator.PowerCosts[3]);

        }

        [Test]
        public void AddCostOverExisting()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 1, 15),
                Until = new DateTime(2011, 4, 15),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
            });

            Assert.AreEqual(4, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 1),
                    Until = new DateTime(2011, 1, 15),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 15),
                    Until = new DateTime(2011, 4, 15),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 4, 15),
                    Until = new DateTime(2011, 10, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 3f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 3f } }
                },
                this._calculator.PowerCosts[2]);
        }

        [Test]
        public void AddCostBetween()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 1, 15),
                Until = new DateTime(2011, 2, 15),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }

            });

            Assert.AreEqual(5, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 1),
                    Until = new DateTime(2011, 1, 15),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 15),
                    Until = new DateTime(2011, 2, 15),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 2, 15),
                    Until = new DateTime(2011, 4, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } }
                },
                this._calculator.PowerCosts[2]);
        }

        [Test]
        public void ReplaceCost()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 2, 1),
                Until = new DateTime(2011, 4, 1),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 10f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 10f } }

            });

            Assert.AreEqual(4, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 1),
                    Until = new DateTime(2011, 2, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 2, 1),
                    Until = new DateTime(2011, 4, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 10f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 10f } }
                },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 4, 1),
                    Until = new DateTime(2011, 10, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 3f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 3f } }
                },
                this._calculator.PowerCosts[2]);
        }

        [Test]
        public void AddCostWithSameSince()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 2, 1),
                Until = new DateTime(2011, 3, 1),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }

            });

            Assert.AreEqual(5, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 1),
                    Until = new DateTime(2011, 2, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 2, 1),
                    Until = new DateTime(2011, 3, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 3, 1),
                    Until = new DateTime(2011, 4, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } }
                },
                this._calculator.PowerCosts[2]);
        }

        [Test]
        public void AddCostWithSameUntil()
        {
            this._calculator.AddPowerCost(new PowerCost
            {
                Since = new DateTime(2011, 3, 1),
                Until = new DateTime(2011, 4, 1),
                WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }

            });

            Assert.AreEqual(5, this._calculator.PowerCosts.Count);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 1, 1),
                    Until = new DateTime(2011, 2, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[0]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 2, 1),
                    Until = new DateTime(2011, 3, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 2f } }
                },
                this._calculator.PowerCosts[1]);
            Assert.AreEqual(
                new PowerCost
                {
                    Since = new DateTime(2011, 3, 1),
                    Until = new DateTime(2011, 4, 1),
                    WinterCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } },
                    SummerCostPeriods = new[] { new CostPeriod { Since = 0, Until = 24, Price = 1f } }
                },
                this._calculator.PowerCosts[2]);
        }

        [Explicit]
        [Test]
        public void SerializePowerCosts()
        {
            this._calculator.Save(@"C:\Temp\powercosts.xml");
        }
    }
}
