using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Screw_detect
{
    public class RobotHandler
    {
        #region Fields

        private static RobotHandler instance;
        private static readonly object lockObject = new object();
        private readonly ModbusHandler modbusHandler = ModbusHandler.Instance;

        #endregion


        #region Properties

        /// <summary>
        ///Singleton object instance
        /// </summary>
        public static RobotHandler Instance
        {
            get
            {
                // Double-checked locking for thread safety
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


        //#region Constructor

        //private RobotHandler()
        //{
        //    Candidates = new Queue<double[]>();
        //}

        //#endregion


        //#region Methods
        ///*
        //public bool InitServerCOM(string IP, int Port)
        //{
        //    try
        //    {
        //        IPAddress ServerIP = IPAddress.Parse(IP);
        //        IPEndPoint localEndPoint = new IPEndPoint(ServerIP, Port);
        //        //IPEndPoint localEndPoint = new IPEndPoint(ServerIP, int.Parse(ConfigurationManager.AppSettings[VOParam.AppSettings.SocketServer_HostIPPort.ToString()]));

        //        m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        //        {
        //            ReceiveTimeout = 0,
        //            SendTimeout = 0
        //        };
        //        m_ServerSocket.Bind(localEndPoint);
        //        m_ServerSocket.Listen(1);               //Only one client accepted
        //        m_ServerSocket.BeginAccept(new AsyncCallback(Read_Callback), m_ServerSocket);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _ = ex;
        //        return false;
        //    }
        //}

        //public void Read_Callback(IAsyncResult ar)
        //{
        //    // Receive TPC commands
        //    Thread.CurrentThread.Priority = ThreadPriority.Highest;

        //    int iBytesReceived;
        //    byte[] bytesReceived;
        //    Socket socketTemp;
        //    string sReceived;

        //    bytesReceived = new byte[2048];
        //    socketTemp = (Socket)ar.AsyncState;

        //    var m_AcceptedSocket = socketTemp.EndAccept(ar);
        //    iBytesReceived = m_AcceptedSocket.Receive(bytesReceived);

        //    //procesar bytes recibidos
        //    sReceived = Encoding.ASCII.GetString(bytesReceived, 0, iBytesReceived);

        //    if (sReceived.Contains("START|"))
        //    {
        //        var res = RSDevice.Snap();
        //        ColorImage = res[0];
        //        RangeImage = res[1];
        //        m_AcceptedSocket.Send(Encoding.ASCII.GetBytes("RESULT|-1;-1;-1;-1"));

        //    }

        //    m_AcceptedSocket.Close();
        //    m_ServerSocket.BeginAccept(new AsyncCallback(Read_Callback), m_ServerSocket);
        //}
        //*/
        //#endregion

        //public bool StartTCPServer(string Ip, int Port)
        //{
        //    IPAddress ipAddress = IPAddress.Parse(Ip);
        //    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

        //    try
        //    {
        //        //Create a Socket that will use Tcp protocol      
        //        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        //        //A Socket must be associated with an endpoint using the Bind method  
        //        listener.Bind(localEndPoint);

        //        //Specify how many requests a Socket can listen before it gives Server busy response.
        //        listener.Listen(1);

        //        //Socket handler = listener.Accept();

        //        receiveDataFromServer(listener);

        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine(e.ToString());
        //        _ = ex;
        //        return false;
        //    }

        //    return true;
        //}

        //private void receiveDataFromServer(Socket listener)
        //{
        //    Task.Run(() =>
        //    {
        //        // Incoming data from the client.    
        //        string data = null;
        //        byte[] bytes = null;

        //        string[] sSplitted1;
        //        Socket handler = listener.Accept();

        //        while (true)
        //        {
        //            try
        //            {
        //                bytes = new byte[1024];
        //                int bytesRec = handler.Receive(bytes);

        //                if (bytesRec == 0)
        //                {
        //                    handler.Shutdown(SocketShutdown.Both);
        //                    handler.Close();
        //                    //restart handler
        //                    handler = listener.Accept();
        //                    bytesRec = handler.Receive(bytes);
        //                }

        //                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

        //                if (data.Length > 0)
        //                {
        //                    byte[] answer;
        //                    sSplitted1 = data.Split('|');
        //                    string command = sSplitted1[0];

        //                    switch (command)
        //                    {
        //                        case "FIND":
        //                            // 1. Check if there is any available candidate, if not wait for it.
        //                            Stopwatch sw = Stopwatch.StartNew();
        //                            //Console.WriteLine($"*** Find received at: {DateTime.Now:hh-mm-ss-fff}");

        //                            if (Candidates.Any())
        //                            {
        //                                //Console.WriteLine($"*** Candidates available at: {DateTime.Now:hh-mm-ss-fff}");

        //                                var deq = Candidates.Peek();
        //                                byte[] xBytes = BitConverter.GetBytes(deq[0]);
        //                                byte[] yBytes = BitConverter.GetBytes(deq[1]);
        //                                byte[] zRotBytes = BitConverter.GetBytes(deq[2]);
        //                                byte[] faceBytes = BitConverter.GetBytes(deq[3]);
        //                                //byte[] xBytes = BitConverter.GetBytes((double)0);
        //                                //byte[] yBytes = BitConverter.GetBytes((double)0);
        //                                //byte[] zRotBytes = BitConverter.GetBytes((double)0);
        //                                //byte[] faceBytes = BitConverter.GetBytes((double)0);

        //                                // Ensure little-endian or big-endian depending on your system
        //                                if (BitConverter.IsLittleEndian)
        //                                {
        //                                    // Reverse the byte arrays if your system is little-endian
        //                                    Array.Reverse(xBytes);
        //                                    Array.Reverse(yBytes);
        //                                    Array.Reverse(zRotBytes);
        //                                    Array.Reverse(faceBytes);
        //                                }

        //                                // Concatenate the byte arrays into a single byte array
        //                                byte[] combinedByteArray = new byte[xBytes.Length + yBytes.Length + zRotBytes.Length + faceBytes.Length];

        //                                Buffer.BlockCopy(xBytes, 0, combinedByteArray, 0, xBytes.Length);
        //                                Buffer.BlockCopy(yBytes, 0, combinedByteArray, xBytes.Length, yBytes.Length);
        //                                Buffer.BlockCopy(zRotBytes, 0, combinedByteArray, xBytes.Length + yBytes.Length, zRotBytes.Length);
        //                                Buffer.BlockCopy(faceBytes, 0, combinedByteArray, xBytes.Length + yBytes.Length + zRotBytes.Length, faceBytes.Length);
        //                                NumCadidates++;
        //                                handler.Send(combinedByteArray);

        //                                sw.Stop();
        //                                //Console.WriteLine("Time to send candidates: "+sw.ElapsedMilliseconds);
        //                                //Console.WriteLine($"*** Candidates sent at: {DateTime.Now:hh-mm-ss-fff}");

        //                            }
        //                            else
        //                            {
        //                                handler.Send(new byte[0]);
        //                            }
        //                            break;

        //                        case "ROBOTAT":
        //                            Console.WriteLine("--- ROBOTAT| Processed");

        //                            if (sSplitted1[1].Contains("True"))
        //                                SystemStates.Instance.IsRobotIn = true;
        //                            else if (sSplitted1[1].Contains("False"))
        //                                SystemStates.Instance.IsRobotIn = false;
        //                            //handler.Send(answer);
        //                            break;

        //                        case "REMOVECANDIDATE":
        //                            _ = Candidates.Dequeue();
        //                            Console.WriteLine("Candidate Removed...");

        //                            break;

        //                        case "FLIP":
        //                            modbusHandler.Flip();
        //                            break;

        //                        case "STOP":
        //                            answer = Encoding.ASCII.GetBytes("This is my answer");
        //                            handler.Send(answer);
        //                            break;

        //                        default:

        //                            break;
        //                    }

        //                    data = "";
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                //handle exception
        //                handler = null;
        //                handler = listener.Accept();

        //                data = "";
        //            }

        //            Thread.Sleep(1);
        //        }
        //    });

        //}

        //public bool InitServerCOM(string IP, int Port)
        //{
        //    return StartTCPServer(IP, Port);
        //}

    }
}
