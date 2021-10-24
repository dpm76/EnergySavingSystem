using System.Collections.Generic;
using System.IO;
using ConsumptionModelling.Model;
using NUnit.Framework;
using ModelSerializer = ConsumptionModelling.GenericSerializer<ConsumptionModelling.Model.YearLevel>;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class ModelSerializerTesFixtures
    {
        [Test]
        public void SerializationDeserialization()
        {
            YearLevel model = new YearLevel
                                  {
                                      ConcreteDayModels =
                                          new List<ConcreteDay>(new[] {new ConcreteDay {Day = 21, Month = 6, Ranges = new []{new ConsumptionRange{Since = 0, Until = 24, Value = 7f}}}}),
                                      DefaultDayModel =
                                          {Ranges = new[] {new ConsumptionRange {Since = 0, Until = 24, Value = 3f}}},
                                      MonthModels = new List<MonthLevel>(new[]
                                                                             {
                                                                                 new MonthLevel
                                                                                     {
                                                                                         Month = 2,
                                                                                         HolidayModel = new HolidayLevel
                                                                                                 {
                                                                                                     Ranges = new[]
                                                                                                             {
                                                                                                                 new ConsumptionRange
                                                                                                                     {
                                                                                                                         Since = 0,
                                                                                                                         Until = 24,
                                                                                                                         Value = 2f
                                                                                                                     }
                                                                                                             }
                                                                                                 }
                                                                                     }
                                                                             }),
                                      WorkingModel = new WorkingLevel
                                                         {
                                                             Ranges = new[]
                                                                          {
                                                                              new ConsumptionRange
                                                                                  {Since = 0, Until = 8, Value = 1f},
                                                                              new ConsumptionRange
                                                                                  {Since = 8, Until = 13, Value = 6f},
                                                                              new ConsumptionRange
                                                                                  {Since = 13, Until = 15, Value = 4f},
                                                                              new ConsumptionRange
                                                                                  {Since = 15, Until = 18, Value = 6f},
                                                                              new ConsumptionRange
                                                                                  {Since = 18, Until = 20, Value = 3f},
                                                                              new ConsumptionRange
                                                                                  {Since = 20, Until = 24, Value = 1f}
                                                                          }
                                                         }
                                  };

            MemoryStream stream = new MemoryStream();
            ModelSerializer.Serialize(stream, model);
            
            MemoryStream stream2 = new MemoryStream(stream.GetBuffer());
            YearLevel model2 = ModelSerializer.Deserialize(stream2);
            Assert.AreEqual(model.ToString(), model2.ToString());
        }

        [Explicit]
        [Test]
        public void CreateExampleModel()
        {
            YearLevel model = new YearLevel
            {
                ConcreteDayModels =
                    new List<ConcreteDay>(new[] { new ConcreteDay { Day = 21, Month = 6, Ranges = new[] { new ConsumptionRange { Since = 0, Until = 24, Value = 7f } } } }),
                DefaultDayModel = { Ranges = new[] { new ConsumptionRange { Since = 0, Until = 24, Value = 3f } } },
                MonthModels = new List<MonthLevel>(new[]
                                                                             {
                                                                                 new MonthLevel
                                                                                     {
                                                                                         Month = 2,
                                                                                         HolidayModel = new HolidayLevel
                                                                                                 {
                                                                                                     Ranges = new[]
                                                                                                             {
                                                                                                                 new ConsumptionRange
                                                                                                                     {
                                                                                                                         Since = 0,
                                                                                                                         Until = 24,
                                                                                                                         Value = 2f
                                                                                                                     }
                                                                                                             }
                                                                                                 }
                                                                                     }
                                                                             }),
                WorkingModel = new WorkingLevel
                {
                    Ranges = new[]
                                                                          {
                                                                              new ConsumptionRange
                                                                                  {Since = 0, Until = 8, Value = 1f},
                                                                              new ConsumptionRange
                                                                                  {Since = 8, Until = 13, Value = 6f},
                                                                              new ConsumptionRange
                                                                                  {Since = 13, Until = 15, Value = 4f},
                                                                              new ConsumptionRange
                                                                                  {Since = 15, Until = 18, Value = 6f},
                                                                              new ConsumptionRange
                                                                                  {Since = 18, Until = 20, Value = 3f},
                                                                              new ConsumptionRange
                                                                                  {Since = 20, Until = 24, Value = 1f}
                                                                          }
                }
            };

            FileStream stream = new FileStream("c:\\temp\\model.xml", FileMode.Create, FileAccess.Write);
            ModelSerializer.Serialize(stream, model);
        }
    }
}
