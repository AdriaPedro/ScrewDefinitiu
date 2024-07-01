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
        static HProcedures procedures = HProcedures.Instance;

         static void Main(string[] args)
        {
            Console.WriteLine("Init procedures");

            if (procedures.Init())
            {
                Console.WriteLine("DONE");
                
            }
            else
            {
                Console.WriteLine("FAIL");
            }


            procedures.Find_Screw();

            Console.WriteLine("X: " + procedures.resultX.ToString() + "\n" + "\nY: " + procedures.resultY.ToString() + "\n" + "\nHight: " + procedures.resultH.ToString() + "\n" + "\nWidth: " + procedures.resultW.ToString());

            Thread.Sleep(10000);

        }
    }
}
