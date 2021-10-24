using System.Data.SqlServerCe;
using EnergyConsumption;
using NUnit.Framework;

namespace TestFixtures.EnergyConsumption
{
    [TestFixture]
    public class DataReaderTestFixtures
    {
        [Test]
        public void CreateDataReaderByReflection()
        {
            DataReader dataReader = DataReader.Create("EnergyConsumption.FakeDataReader", "Fake", "COM1", 1, 1000, 5000, null);
            Assert.IsInstanceOf(typeof(FakeDataReader), dataReader);
        }

    }
}
