using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Screw_detect
{
    class Program
    {
        static readonly HProcedures procedures = HProcedures.Instance;
        static readonly RobotHandler robotHandler = RobotHandler.Instance;

        static void Main(string[] args)
        {
            Console.WriteLine("Init procedures");
            /*
            if (procedures.Init())
            {
                Console.WriteLine("DONE");
                
            }
            else
            {
                Console.WriteLine("FAIL");
            }

            procedures.Find_Screw();

            Console.WriteLine("X: " + procedures.resultX.ToString() + "\n" + "\nY: " + procedures.resultY.ToString() + "\n" + "\nHight: " +
                procedures.resultH.ToString() + "\n" + "\nWidth: " + procedures.resultW.ToString());
            */
            // Init robot server com 
            robotHandler.InitServerCOM("192.168.5.1", 8080); // 127.0.0.1 = Localhost
            Console.WriteLine("TCP Server COM - Started");

            

            Console.ReadKey();

        }
    }
}
