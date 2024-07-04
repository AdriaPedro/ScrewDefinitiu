using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace VO_PICKER.Models
{
    public class FlexiBowlHandler
    {
        #region Fields

        private static readonly object s_lock = new object();
        private static FlexiBowlHandler instance;
        public string flexiBowl_IP = "10.17.1.20";
        public static TcpClient tcpclnt { get; set; }

        public Dictionary<string, string> FlexiBowlCommands = new Dictionary<string, string>()
        {
            {"Move", "\0\aQX2\r" }, //Moves the Flexibowl® with the current parameters
            {"Move - Flip", "\0\aQX3\r" }, //Moves the Flexibowl® and activates the Flip during the movement.
            {"Move - Blow - Flip", "\0\aQX4\r" }, //Moves the Flexibowl® and activates the Flip and the second valve during the movement.
            {"Move - Blow", "\0\aQX5\r" }, //Moves the Flexibowl® and activates the second valve during the movement.
            {"Shake", "\0\aQX6\r" }, //Shakes the Flexibowl® with the current parameters.
            {"Light on", "\0\aQX7\r" }, //Turns the backlight on.
            {"Light off", "\0\aQX8\r" }, //Turns the backlight off.
            {"Blow", "\0\aQX9\r" }, //Turns the drill on with the current parameters.
            {"Flip", "\0\aQX10\r" }, //Turns the Air blow on with the current parameters.
            {"Quick Emptying", "\0\aQX11\r" }, //Perform the Quick Emptying sequence of the Flexibowl®
            {"Reset Alarm", "\0\aQX12\r" }, //Reset the alarm and enable the motor
            {"Raises signal 2", "\0\aSO2L\r" }, //Turns on the Flip valve (the Flip remains high)
            {"Lowers signal 2", "\0\aSO2H\r" }, //Turns off the Flip valve (the Flip lowers)
            {"Lifts signal 3", "\0\aSO3L\r" }, //Turns the Air Blow on
            {"Lowers signal 3", "\0\aSO3H\r" }, //Turns the Air Blow off
            {"Lifts signal 4", "\0\aSO4L\r" }, //Turns the backlight on
            {"Lowers signal 4", "\0\aSO4H\r" }, //Turns the backlight off
            {"Status", "\0\aIO\r" }, //To know if the Flexibowl® has finished the command
        };
        #endregion

        #region Singleton Pattern Constructor
        // Singleton Pattern
        public static FlexiBowlHandler Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (instance == null)
                    {
                        instance = new FlexiBowlHandler();
                    }
                    return instance;
                }
            }
        }
        #endregion

        #region Public definitions
        
        #endregion

        #region Methods
        //Method to connect to the localhost
        public bool InitClientCOM()
        {
            try
            {
                tcpclnt = new TcpClient();

                //tcpclnt.Connect("10.17.1.20", Int32.Parse("7776"));
                tcpclnt.Connect(flexiBowl_IP, int.Parse("7776"));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Send messsage throught client communication
        public bool SendCommand(string str)
        {
            try
            {
                #region Info
                //Casting HexToCHar FlexiBowl 
                //int value = Convert.ToInt32("0D", 16);
                //// Get the character corresponding to the integral value.
                //string stringValue = Char.ConvertFromUtf32(value);
                //char charValue = (char)value;
                #endregion
                //Create a new socket for every command
                tcpclnt = new TcpClient();
                tcpclnt.Connect(flexiBowl_IP, int.Parse("7776"));

                //get stream and send command
                Stream stm = tcpclnt.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                //Console.WriteLine("Transmitting.....");
                stm.Write(ba, 0, ba.Length);
                //Cast the ECHO message
                byte[] bb = new byte[512];
                //Read echo buffer
                int k = stm.Read(bb, 0, 512);
                byte[] num = new byte[1];
                num[0] = bb[2];
                string result = Encoding.UTF8.GetString(num);
                if (result == "%")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error..... " + ex.StackTrace);
                return false;
            }
        }

        //Send FlexiBowl current status
        public bool CheckStatus(string str= "\0\aIO\r")
        {
            try
            {
                //Create a new socket for every command
                //tcpclnt = new TcpClient();
                //tcpclnt.Connect(flexiBowl_IP, int.Parse("7776"));
                //get stream and send command
                Stream stm = tcpclnt.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                stm.Write(ba, 0, ba.Length);
                //Cast the ECHO message
                byte[] bb = new byte[512];
                //Read echo buffer
                int k = stm.Read(bb, 0, 512);
                byte[] num = new byte[1];
                num[0] = bb[12];
                string result = Encoding.UTF8.GetString(num);
                if (result == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error..... " + ex.StackTrace);
                return false;
            }
        }
        #endregion
    }
}
