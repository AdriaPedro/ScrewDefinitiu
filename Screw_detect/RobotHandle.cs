using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Screw_detect
{
    public class RobotHandler
    {
        #region Fields

        private static RobotHandler instance;
        private static readonly object lockObject = new object();

        #endregion


        #region Properties

        ///<summary>
        ///Singleton object instance
        ///</summary>
        public static RobotHandler Instance
        {
            get
            {
                //Double-checked locking for thread safety
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                            instance = new RobotHandler();
                    }
                }
                return instance;
            }
        }

        public Queue<double[]> Candidates { get; set; }
        public int NumCadidates;

        #endregion


        #region Constructor

        private RobotHandler()
        {
        }

        #endregion


        #region Methods

        #endregion

        private bool StartTCPServer(string Ip, int Port)
        {
            IPAddress ipAddress = IPAddress.Parse(Ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

            try
            {
                //Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);

                //Specify how many requests a Socket can listen before it gives Server busy response.
                listener.Listen(1);

                //Socket handler = listener.Accept();

                receiveDataFromServer(listener);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _ = ex;
                return false;
            }

            return true;
        }

        private void receiveDataFromServer(Socket listener)
        {
            Task.Run(() =>
            {
                // Incoming data from the client.    
                string data = string.Empty;
                byte[] bytes = null;

                string[] sSplitted1;
                Socket handler = listener.Accept();

                while (true)
                {
                    try
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);

                        if (bytesRec == 0)
                        {
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            //restart handler
                            handler = listener.Accept();
                            bytesRec = handler.Receive(bytes);
                        }

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.Length > 0)
                        {
                            byte[] answer;
                            sSplitted1 = data.Split('|');
                            var splitted2 = sSplitted1[1].Split(';');
                            string command = sSplitted1[0];

                            switch (command)
                            {
                                case "FIND":
                                    string result = "Find Message has been processed. with value: "+ sSplitted1[1];
                                    int value = int.Parse(splitted2[0]);
                                    answer = Encoding.ASCII.GetBytes(result);

                                    handler.Send(answer);
                                    break;
                                case "SNAP":

                                    break;
                                default:

                                    break;
                            }

                            data = "";
                        }
                    }
                    catch (Exception)
                    {
                        //handle exception
                        handler = null;
                        handler = listener.Accept();

                        data = "";
                    }

                    Thread.Sleep(1);
                }
            });

        }

        public bool InitServerCOM(string IP, int Port)
        {
            return StartTCPServer(IP, Port);
        }

    }
}