using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using ConsumptionModelling;
using NUnit.Framework;

namespace TestFixtures.ConsumptionModelling
{
    [TestFixture]
    public class CommitedConsumptionDataProviderTestFixtures
    {
        private CommitedConsumptionDataProvider _dataProvider;
        private SqlCeConnection _dbConnection;

        [SetUp]
        public void Init()
        {
            this._dbConnection = new SqlCeConnection(@"Data Source=TestFixturesDataBase.sdf;Persist Security Info=False;");
            this._dbConnection.Open();
            this._dataProvider = new CommitedConsumptionDataProvider(this._dbConnection);
            this.DeleteTable();

            this.InsertData(new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), 1f);
            this.InsertData(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                            new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc), 4f);
            this.InsertData(new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc),
                            new DateTime(2011, 1, 2, 0, 0, 0, DateTimeKind.Utc), 1f);

        }

        [TearDown]
        public void Finish()
        {
            this.DeleteTable();
            this._dbConnection.Close();
        }

        private void InsertData(DateTime since, DateTime until, float value)
        {
            using (DbCommand command = this._dbConnection.CreateCommand())
            {
                for (DateTime d = since; d < until; d = d.AddMinutes(15d))
                {

                    command.CommandText =
                        string.Format("INSERT INTO Consumption (Source, ReadTime, Energy) VALUES ('{0}','{1}',{2})",
                                      "test", d, value);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteTable()
        {
            using (DbCommand command = this._dbConnection.CreateCommand())
            {
                command.CommandText = "Delete Consumption";
                command.ExecuteNonQuery();
            }
        }

        [Test]
        public void GetLastDataTimeStampTest()
        {
            DateTime timeStamp = this._dataProvider.GetLastDataTimeStamp("test");
            Assert.AreEqual(new DateTime(2011, 1, 1, 23, 45, 0, DateTimeKind.Utc), timeStamp);
        }

    }
}
