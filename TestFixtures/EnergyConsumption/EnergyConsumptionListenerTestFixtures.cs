using System;
using System.Net;
using System.Net.Sockets;
using Communication.Messages;
using ConsumptionDaemon;
using NUnit.Framework;

namespace TestFixtures.EnergyConsumption
{
    [TestFixture]
    public class EnergyConsumptionListenerTestFixtures
    {
        private const string FAKE_SOURCE = "Fake";

        [SetUp]
        public void Init()
        {
            ConsumptionManager.Default.Start();
        }

        [TearDown]
        public void Finish()
        {
            ConsumptionManager.Default.Stop();
        }

        [Test]
        public void GetSourcesTest()
        {
            TcpClient client = new TcpClient("127.0.0.1", ConsumptionManager.Default.ListenPort);
            NetworkStream stream = client.GetStream();
            MessageSerializer.Instance.ToStream(stream, new GetAvailableConsumptionSourcesCommand());
            AvailableConsumptionSourcesMessage message = 
                (AvailableConsumptionSourcesMessage)
                MessageSerializer.Instance.FromStream(stream);

            Assert.IsTrue(message.MagnitudesBySources.ContainsKey(FAKE_SOURCE));
        }

        [Test]
        public void GetConsumptionDataTest()
        {
            TcpClient client = new TcpClient("127.0.0.1", ConsumptionManager.Default.ListenPort);
            NetworkStream stream = client.GetStream();
            MessageSerializer.Instance.ToStream(stream, new RegisterConsumptionDataReaderListenerCommand{ListenPort = 1000, SourceId = FAKE_SOURCE});

            UdpClient listener = new UdpClient(new IPEndPoint(IPAddress.Any, 1000));
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 1000);
            byte[] inBuffer = listener.Receive(ref remote);
            listener.Close();
            ConsumptionDataMessage message = (ConsumptionDataMessage) MessageSerializer.Instance.FromBuffer(inBuffer);
            Console.WriteLine(message);
            Assert.AreEqual(FAKE_SOURCE, message.Source);
        }
    }
}
