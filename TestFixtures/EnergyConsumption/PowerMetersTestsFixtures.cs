using System;
using System.IO.Ports;
using EnergyConsumption;
using NUnit.Framework;

namespace TestFixtures.EnergyConsumption
{
    [TestFixture]
    public class PowerMetersTestsFixtures
    {
        [Explicit]
        [Test]
        public void MkRead()
        {
            ModbusPort port = new ModbusPort();
            port.Open("COM22", 9600, 8, Parity.None, StopBits.One);

            short[] data = new short[0xc];
            port.SendFc3(2, 0, 0xc, ref data);
            port.Close();

            float volts = Words2Float(data[0], data[1]) / 10f;
            float current = Words2Float(data[2], data[3]);
            float power = Words2Float(data[4], data[5]);
            float frec = Words2Float(data[6], data[7]) / 10f;
            float activeEnergy = Words2Float(data[8], data[9]);
            float partialEnergy = Words2Float(data[10], data[11]);

            Console.WriteLine(@"Voltaje: {0} V", volts);
            Console.WriteLine(@"Corriente: {0} mA", current);
            Console.WriteLine(@"Potencia: {0} W", power);
            Console.WriteLine(@"Frecuencia: {0} Hz", frec);
            Console.WriteLine(@"Energía activa: {0} Wh", activeEnergy);
            Console.WriteLine(@"Energía parcial: {0} Wh", partialEnergy);
        }

        private static float Words2Float(short word1, short word2)
        {
            return ((((long) word1) << 16) + word2);
        }
    }
}
