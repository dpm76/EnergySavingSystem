using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO.Ports;

namespace EnergyConsumption
{
    /// <summary>
    /// Lector de datos para el MK de Circutor
    /// </summary>
    public class MkDataReader:DataReader
    {
        private readonly ModbusPort _port = new ModbusPort();
        private float _lastActivePower;

        public MkDataReader(int currentPowerReadInterval, int qPowerReadInterval, DbConnection dbConnection)
            : base(currentPowerReadInterval, qPowerReadInterval, dbConnection)
        {
            this.SourceId = "MK Reader"; //Nombre por defecto
            this.Magnitudes = new[] {"W", "V", "A"};
        }

        protected override Dictionary<string, float> ReadCurrentPowerData()
        {
            Dictionary<string, float> data = new Dictionary<string, float>(this.Magnitudes.Length);

            if(this._port.IsOpen && (this.DeviceId > 0))
            {
                short[] buffer = new short[0xc];
                lock (_port)
                {
                    this._port.SendFc3(this.DeviceId, 0, 0xc, ref buffer);
                }

                float watts = ((((long) (buffer[4])) << 16) + buffer[5]);
                float volts = ((((long) (buffer[0])) << 16) + buffer[1]) / 10f;
                float amps = ((((long) (buffer[2])) << 16) + buffer[3]);
                data.Add(this.Magnitudes[0], watts);
                data.Add(this.Magnitudes[1], volts);
                data.Add(this.Magnitudes[2], amps);
            }

            return data;
        }

        protected override ConsumptionData ReadQPowerData()
        {
            float currentActivePower = this.ReadActivePower();
            float incActivePower = currentActivePower - this._lastActivePower;
            this._lastActivePower = currentActivePower;

            return new ConsumptionData{Data = incActivePower, TimeStamp = DateTime.UtcNow};
        }

        private float ReadActivePower()
        {
            short[] buffer = new short[0xc];
            lock (_port)
            {
                this._port.SendFc3(this.DeviceId, 0, 0xc, ref buffer);
            }
            float activePower = (((long)buffer[8]) << 16) + buffer[9];
            this.Log.Trace("Energía activa: " + activePower);

            return activePower;
        }

        public override void Start()
        {
            if (!string.IsNullOrEmpty(this.ComPort) && this.ComPort.StartsWith("COM"))
            {
                this._port.Open(this.ComPort, 9600, 8, Parity.None, StopBits.One);
                this._lastActivePower = this.ReadActivePower();
                base.Start();
            }else
            {
                throw new Exception("No se ha establecido un puerto de comunicación o no es correcto.");
            }
        }

        public void Start(string comPort)
        {
            this.ComPort = comPort;
            this.Start();
        }

        public override void Stop()
        {
            base.Stop();
            lock (_port)
            {
                this._port.Close();
            }
        }
    }
}
