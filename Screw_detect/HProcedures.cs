using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using HalconDotNet;
using System.IO;

namespace Screw_detect
{
    class HProcedures
    {

        private static readonly Object s_lock = new Object();
        private static HProcedures instance;

        public HTuple resultX = new HTuple(); //TODO: propiedad de resultado 
        public HTuple resultY = new HTuple();
        public HTuple resultW = new HTuple();
        public HTuple resultH = new HTuple();

        private HDevProcedureCall processingProcedureCall;
        private HDevProcedure processingProcedure;

        HDevEngine hDevEngine;

        #region Singleton Pattern Constructor
        // Singleton Pattern
        public static HProcedures Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (instance == null)
                    {
                        instance = new HProcedures();
                    }
                    return instance;
                }
            }
        }
        #endregion


        public bool Init()
        {
            try
            {
                hDevEngine = new HDevEngine();
                hDevEngine.SetProcedurePath("C:\\Users\\VOServer2\\Desktop\\Practicas\\Screw_detect\\bin\\x64\\Debug\\lib");
                HDevProcedure processingProcedure = new HDevProcedure("vo_find_screw_main");  //TODO: Nombre procedimiento
                processingProcedureCall = new HDevProcedureCall(processingProcedure);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }



        public void Find_Screw()
        {


            bool isDebuggingHalcon = false;
            try
            {

                HImage hImage = new HImage();


                hImage.ReadImage("C:\\Users\\VOServer2\\Desktop\\las practicas\\fotos cargols\\foto6.png");  //TODO: Ruta de la imagen

                //TODO: Param Entrada
                //Set the procedure inputs
                processingProcedureCall.SetInputIconicParamObject("Image", hImage); //testImage
                //processingProcedureCall.SetInputCtrlParamTuple("DICTPATH", ); //v1.0.5 added the dictionary


                //TODO: Ejecucion Procedimiento
                //Executes procedure
                if (isDebuggingHalcon)
                {
                    hDevEngine.StartDebugServer();
                    processingProcedureCall.Execute();
                    hDevEngine.StopDebugServer();
                }
                else
                    processingProcedureCall.Execute();

                //TODO: param salida
                //Get procedure outputs
                //ocrResultImage = processingProcedureCall.GetOutputIconicParamImage("OCRResultImage");
                resultX = processingProcedureCall.GetOutputCtrlParamTuple("ResultX");//TODO: en vez de darme un ok me de un "[]" para solucionarlo,, es lo del windowhandle del halcon boralo
                resultY = processingProcedureCall.GetOutputCtrlParamTuple("ResultY");
                resultH = processingProcedureCall.GetOutputCtrlParamTuple("ResultH");
                resultW = processingProcedureCall.GetOutputCtrlParamTuple("ResultW");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }

    
}
