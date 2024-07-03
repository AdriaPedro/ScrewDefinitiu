using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CCyberPick.Models;
using ModbusTCP;

namespace VO_PICKER.Models
{
    public class ModbusHandler
    {
        #region Fields

        private static readonly object s_lock = new object();
        private ModbusTCP.Master MBmaster;
        public string modbus_Master_IP = "10.17.3.50";
        public string modbus_Master_Port = "8502";
        public bool is_connected = false;
        public DispatcherTimer  Pooling_Timer;
        public bool IsFeeding;
        private byte singleBitValue;
        #endregion

        #region Properties
        private static ModbusHandler instance;
        public static ModbusHandler Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (instance == null)
                    {
                        instance = new ModbusHandler();
                    }
                    return instance;
                }
            }
        }
        #endregion

        #region Public definitions
        public Dictionary<string, ushort> PLCMappingIO = new Dictionary<string, ushort>()
        {
            {"Backlight", 67 }, //Switch ON/OFF FlexiBowl Backlight Q8.3
            //{"Frontlight", 68 }, //Switch ON/OFF FlexiBowl FrontLight for calibration Q8.4
            {"Frontlight", 2 }, //Switch ON/OFF FlexiBowl FrontLight for calibration Q8.4
            {"Feeder", 24 }, //Trigger feeding routine Q3.0
            {"W_Light_CH0", 25 }, //Red Light Bit mapping Q3.1 on Q8.0
            {"W_Light_CH1", 26 }, //Green Light Bit mapping on Q8.1
            {"W_Light_CH2", 27 }, //Blue Light Bit mapping on Q8.2
            {"RLY230VAC", 3}, //Turn ON/OFF FlexiBowl (HARD RESET)
            {"RDY", 1}, //Turn ON/OFF System RDY flag Q0.1
            {"CYLINDER", 3}, //Turn ON/OFF System RDY flag Q0.1
        };

        public Dictionary<string, ushort> PLCMappingREG = new Dictionary<string, ushort>()
        {
            {"FeedingTimeMS", 0 }, //Feeding activation time
            {"BlinkingTimeMS", 1 }, //Blinking time
        };
        #endregion

        #region Private Methods
        //
        // Event for response data
        // ------------------------------------------------------------------------
        private void MBmaster_OnResponseData(ushort ID, byte unit, byte function, byte[] values)
        {
            // ------------------------------------------------------------------
            // Seperate calling threads
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Master.ResponseData(MBmaster_OnResponseData), new object[] { ID, unit, function, values });
                return;
            }

            // Identify requested data
            switch (ID)
            {
                case 1:
                    singleBitValue = values[0];
                    break;
                case 2:
                    //Read discrete inputs
                    var bitresponseArray = new BitArray(values);
                    //Prevent thread error read/write
                    lock (s_lock)
                    {
                        //Update system value
                        //VOSystem.Instance.Is2DNIOCntrLimit = Convert.ToBoolean(bitresponseArray[1]);
                        //Update system value
                        //VOSystem.Instance.Is2DNOCntrLimit = Convert.ToBoolean(bitresponseArray[2]);
                        ////Update system value
                        ////VOSystem.Instance.IsRobotRunning = Convert.ToBoolean(bitresponseArray[3]);
                        ////Update system value
                        //VOSystem.Instance.IsAirPressureOK = Convert.ToBoolean(bitresponseArray[4]);
                        ////Update system value
                        //VOSystem.Instance.IsConveyorRdy = Convert.ToBoolean(bitresponseArray[5]);

                    }

                    //****Cast current system state ****
                    //Cast 2D cam NIO limit reached
                    //if (VOSystem.Instance.Is2DNIOCntrLimit)
                    //    AlarmsManager.Instance.CreateAlarm("2D CAM NIO Limit Reached!", "warning");
                    ////Cast 2D cam NO limit reached
                    //if (VOSystem.Instance.Is2DNOCntrLimit)
                    //    AlarmsManager.Instance.CreateAlarm("2D CAM IO Limit Reached!", "warning");
                    ////Cast robot live signal
                    //if (!VOSystem.Instance.IsRobotRunning)
                    //    AlarmsManager.Instance.CreateAlarm("Error 404-R Robot not ready", "error");
                    ////Cast air preassure live signal
                    //if (!VOSystem.Instance.IsAirPressureOK)
                    //    AlarmsManager.Instance.CreateAlarm("Error 101-P Air preasure to low", "error");
                    //Cast conveyor rdy live signal
                    //if (!VOSystem.Instance.IsConveyorRdy)
                    //    AlarmsManager.Instance.CreateAlarm("Error 405-S Conveyor Belt not ready", "error");
                    break;

                case 3:
                    //Read holding register 2 bytes representation 
                    var holdsresponseArray = values;

                    //Update system value 
                    SystemStates.Instance.IsAirPressureOK = Convert.ToBoolean(holdsresponseArray[1]);
                    //Update system value 
                    SystemStates.Instance.IsRobotRunning = Convert.ToBoolean(holdsresponseArray[3]);

                    //Cast robot live signal 
                    //if (!SystemStates.Instance.IsRobotRunning)
                        //Console.WriteLine("Robot Not Running");
                    //Cast air preassure live signal 
                    //if (!SystemStates.Instance.IsAirPressureOK)
                        //Console.WriteLine("Air Presure Error");
                    break;
                case 4:
                    //Read input register
                    break;
                case 5:
                    ////Cast the answer according to flag
                    //if (VOSystem.Instance.Is_Feeding)
                    //{
                    //    //Pull down the flag to allow new Find Next
                    //    VOSystem.Instance.Is_Feeding = false;

                    //    //Increase the acknowledge feed counter
                    //    VOSystem.Instance.Feed_RequestCounter++;

                    //    //To logger
                    //    Utilities.Instance.WriteMessageToLog("ROBOT FEED ACKNOWLEDGE");
                    //}
                    break;
                case 6:
                    //Write multiple coils
                    break;
                case 7:
                    //Write single register
                    break;
                case 8:
                    //Write multiple register
                    break;
            }
        }
        // ------------------------------------------------------------------------
        // Event for Modbus TCP slave exception
        // ------------------------------------------------------------------------
        private void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case Master.excIllegalFunction: exc += "Illegal function!"; break;
                case Master.excIllegalDataAdr: exc += "Illegal data adress!"; break;
                case Master.excIllegalDataVal: exc += "Illegal data value!"; break;
                case Master.excSlaveDeviceFailure: exc += "Slave device failure!"; break;
                case Master.excAck: exc += "Acknoledge!"; break;
                case Master.excGatePathUnavailable: exc += "Gateway path unavailbale!"; break;
                case Master.excExceptionTimeout: exc += "Slave timed out!"; break;
                case Master.excExceptionConnectionLost: exc += "Connection is lost!"; break;
                case Master.excExceptionNotConnected: exc += "Not connected!"; break;
            }
        }

        //
        private byte[] GetData(int wordValue)
        {
            byte[] data = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)wordValue));

            return data;
        }

        // ------------------------------------------------------------------------
        // Command write single coil
        // ------------------------------------------------------------------------
        private void WriteSingleCoilCMD(ushort StartAddress, byte data)
        {
            ushort ID = 5;
            byte unit = 0;

            MBmaster.WriteSingleCoils(ID, unit, StartAddress, Convert.ToBoolean(data));
            Thread.Sleep(100);
        }

        private bool ReadSingleCoilCMD(string bitDir)
        {
            //string bitAssignment = "0.0";
            ushort bytePos = Convert.ToUInt16(bitDir.Substring(0, 1));
            ushort bitPos = Convert.ToUInt16(bitDir.Substring(2, 1));

            ushort ID = 1;
            byte unit = 0;
            ushort StartAddress = (ushort)(bitPos + (bytePos * 8));

            //MBmaster.WriteSingleCoils(ID, 1, StartAddress, false);

            byte[] values = new byte[3];
            MBmaster.ReadCoils(ID, unit, 0, 100);

            return true;
        }


        // ------------------------------------------------------------------------
        // Command write single register
        // ------------------------------------------------------------------------
        private void WriteSingleRegCMD(ushort StartAddress, byte[] data)
        {
            ushort ID = 7;
            byte unit = 0;

            MBmaster.WriteSingleRegister(ID, unit, StartAddress, data);
        }

        // ------------------------------------------------------------------------
        // Command read holding register
        // ------------------------------------------------------------------------
        private void ReadHoldRegisterCMD(ushort StartAddress, UInt16 length)
        {
            ushort ID = 3;
            byte unit = 0;

            MBmaster.ReadHoldingRegister(ID, unit, StartAddress, length);
        }

        // ------------------------------------------------------------------------
        // Command read digital inputs
        // ------------------------------------------------------------------------
        private void ReadDigitalIOCMD(ushort StartAddress, UInt16 length)
        {
            ushort ID = 2;
            byte unit = 0;

            MBmaster.ReadDiscreteInputs(ID, unit, StartAddress, length);
        }

        private void PoolingRequest(object sender, EventArgs e)
        {
            //*********Read the registers ********
            //I0.1 2d Camera NIO Counter limit reached
            //I0.2 2d Camera NO Counter limit reached
            //I0.3 2d Robot is ready
            //I0.4 2d Air preassure is ready
            //ReadDigitalIOCMD(0, 8); //get the first byte for the 8 Input bits 
            //ReadHoldRegisterCMD(2, 2);
        }
        #endregion

        #region Public Methods
        public bool InitCom()
        {
            try
            {
                // Create new modbus master and add event functions
                MBmaster = new Master(modbus_Master_IP, ushort.Parse(modbus_Master_Port), true);
                MBmaster.OnResponseData += new ModbusTCP.Master.ResponseData(MBmaster_OnResponseData);
                MBmaster.OnException += new ModbusTCP.Master.ExceptionData(MBmaster_OnException);

                //Create and start pooling timer
                Pooling_Timer = new DispatcherTimer();
                Pooling_Timer.Interval = TimeSpan.FromMilliseconds(1000);
                Pooling_Timer.Tick += new EventHandler(PoolingRequest);
                Pooling_Timer.Start();

                //rise up falg
                is_connected = true;

                return true;
            }
            catch (SystemException)
            {
                return false;
            }
        }

        public void RequestFeed(/*ushort feedAddress, byte value, bool is_forced = false*/)
        {
            //Send command
            //WriteSingleCoilCMD(PLCMappingIO["Feeder"], 1);
            bool aa = ReadSingleCoilCMD("0.0");


            //Rise up flag for feeding in process (only in auto request not forced ones)
            //if(!is_forced)
            //    VOSystem.Instance.Is_Feeding = true;
        }

        public void Feed(int timeMs)
        {
            Task.Run(() => {
            //Send command
                IsFeeding = true;
                WriteSingleCoilCMD(0, 1);
                Thread.Sleep(timeMs/2);
                IsFeeding = false;
                Thread.Sleep(timeMs/2);
                WriteSingleCoilCMD(0, 0);
                Thread.Sleep(10);

                //singleBitValue = 255;
                //while (singleBitValue == 255)
                //{
                //    WriteSingleCoilCMD(0, 0);
                //    Thread.Sleep(10);
                //    ReadSingleCoilCMD("0.0");            
                //}
                //IsFeeding = false;

            });
        }

        public void SwitchFrontLight(byte val)
        {
            WriteSingleCoilCMD(PLCMappingIO["Frontlight"], val);
        }

        public void SwitchBackLight(byte value)
        {
            WriteSingleCoilCMD(PLCMappingIO["Backlight"], value);
        }

        public void SwitchFlexiBowlBacklight(ushort feedAddress, byte value)
        {
            //Send command
            WriteSingleCoilCMD(feedAddress, value);
        }

        public void SwitchSystemReadySignal(ushort feedAddress, byte value)
        {
            //Send command
            WriteSingleCoilCMD(feedAddress, value);
        }

        public void WriteDataToRegister(ushort address, int data) 
        {
            try
            {
                WriteSingleRegCMD(address, GetData(data));
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Used to flip pieces with cylinder
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public void Flip()
        {
            WriteSingleCoilCMD(PLCMappingIO["CYLINDER"], 1);
            Thread.Sleep(1000);
            WriteSingleCoilCMD(PLCMappingIO["CYLINDER"], 0);
        }

        public void UpdateWarningLight(byte state, string color)
        {
            if(color == "red")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], 0);
            }
            else if(color == "green")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], 0);
            }
            else if(color == "yellow")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], 0);
            }
            else if (color == "blue")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], state);
            }
            else if (color == "purple")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], state);
            }
            else if (color == "white")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], state);
            }
            else if (color == "cyan")
            {
                //Send command
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH0"], 0);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH1"], state);
                WriteSingleCoilCMD(PLCMappingIO["W_Light_CH2"], state);
            }
        }
        #endregion
    }
}
