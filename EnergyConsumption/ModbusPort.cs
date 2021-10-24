using System;
using System.IO.Ports;

namespace EnergyConsumption
{
    /// <summary>
    /// Lee datos a través de una conexión Modbus
    /// </summary>
    public class ModbusPort
    {
        private readonly SerialPort _sp = new SerialPort();
        public string ModbusStatus { get; set; }

        #region Open / Close Procedures

        public bool IsOpen
        {
            get { return this._sp.IsOpen; }
        }

        public bool Open(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {
            bool success;

            //Ensure port isn't already opened:
            if (!_sp.IsOpen)
            {
                //Assign desired settings to the serial port:
                _sp.PortName = portName;
                _sp.BaudRate = baudRate;
                _sp.DataBits = databits;
                _sp.Parity = parity;
                _sp.StopBits = stopBits;
                //These timeouts are default and cannot be editted through the class at this point:
                _sp.ReadTimeout = 1000;
                _sp.WriteTimeout = 1000;

                try
                {
                    _sp.Open();
                    ModbusStatus = portName + " opened successfully";
                    success = true;
                }
                catch (Exception err)
                {
                    ModbusStatus = "Error opening " + portName + ": " + err.Message;
                    success = false;
                }
                
            }
            else
            {
                ModbusStatus = portName + " already opened";
                success = false;
            }

            return success;
        }
        public bool Close()
        {
            bool success;

            //Ensure port is opened before attempting to close:
            if (_sp.IsOpen)
            {
                try
                {
                    _sp.Close();
                    ModbusStatus = _sp.PortName + " closed successfully";
                    success = true;
                }
                catch (Exception err)
                {
                    ModbusStatus = "Error closing " + _sp.PortName + ": " + err.Message;
                    success = false;
                }
            }
            else
            {
                ModbusStatus = _sp.PortName + " is not open";
                success = false;
            }

            return success;
        }
        #endregion

        #region CRC Computation
        private static void GetCrc(byte[] message, ref byte[] crc)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort crcFull = 0xFFFF;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                crcFull = (ushort)(crcFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    char crcLsb = (char)(crcFull & 0x0001);
                    crcFull = (ushort)((crcFull >> 1) & 0x7FFF);

                    if (crcLsb == 1)
                    {
                        crcFull = (ushort) (crcFull ^ 0xA001);
                    }
                }
            }
            crc[1] = (byte)((crcFull >> 8) & 0xFF);
            crc[0] = (byte)(crcFull & 0xFF);
        }
        #endregion

        #region Build Message
        private static void BuildMessage(byte address, byte type, ushort start, ushort registers, ref byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] crc = new byte[2];

            message[0] = address;
            message[1] = type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;

            GetCrc(message, ref crc);
            message[message.Length - 2] = crc[0];
            message[message.Length - 1] = crc[1];
        }
        #endregion

        #region Check Response
        private static bool CheckResponse(byte[] response)
        {
            //Perform a basic CRC check:
            byte[] crc = new byte[2];
            GetCrc(response, ref crc);
            return 
                ((crc[0] == response[response.Length - 2]) &&
                (crc[1] == response[response.Length - 1]));
        }
        #endregion

        #region Get Response
        private void GetResponse(ref byte[] response)
        {
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time).  Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.
            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(_sp.ReadByte());
            }
        }
        #endregion

        #region Function 16 - Write Multiple Registers
        public bool SendFc16(byte address, ushort start, ushort registers, short[] values)
        {
            bool success;

            //Ensure port is open:
            if (_sp.IsOpen)
            {
                //Clear in/out buffers:
                _sp.DiscardOutBuffer();
                _sp.DiscardInBuffer();
                //Message is 1 addr + 1 fcn + 2 start + 2 reg + 1 count + 2 * reg vals + 2 CRC
                byte[] message = new byte[9 + 2 * registers];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];

                //Add bytecount to message:
                message[6] = (byte)(registers * 2);
                //Put write values into message prior to sending:
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(values[i] >> 8);
                    message[8 + 2 * i] = (byte)(values[i]);
                }
                //Build outgoing message:
                BuildMessage(address, 16, start, registers, ref message);
                
                //Send Modbus message to Serial Port:
                try
                {
                    _sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                    //Evaluate message:
                    if (CheckResponse(response))
                    {
                        ModbusStatus = "Write successful";
                        success = true;
                    }
                    else
                    {
                        ModbusStatus = "CRC error";
                        success = false;
                    }
                }
                catch (Exception err)
                {
                    ModbusStatus = "Error in write event: " + err.Message;
                    success = false;
                }
                
            }
            else
            {
                ModbusStatus = "Serial port not open";
                success = false;
            }

            return success;
        }
        #endregion

        #region Function 3 - Read Registers
        public bool SendFc3(byte address, ushort start, ushort registers, ref short[] values)
        {
            bool success;
            //Ensure port is open:
            if (_sp.IsOpen)
            {
                //Clear in/out buffers:
                _sp.DiscardOutBuffer();
                _sp.DiscardInBuffer();
                //Function 3 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, 3, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                    _sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                    //Evaluate message:
                    if (CheckResponse(response))
                    {
                        //Return requested register values:
                        for (int i = 0; i < (response.Length - 5) / 2; i++)
                        {
                            values[i] = response[2 * i + 3];
                            values[i] <<= 8;
                            values[i] += response[2 * i + 4];
                        }
                        ModbusStatus = "Read successful";
                        success = true;
                    }
                    else
                    {
                        ModbusStatus = "CRC error";
                        success = false;
                    }
                }
                catch (Exception err)
                {
                    ModbusStatus = "Error in read event: " + err.Message;
                    success = false;
                }
            }
            else
            {
                ModbusStatus = "Serial port not open";
                success = false;
            }

            return success;
        }
        #endregion

    }
}
