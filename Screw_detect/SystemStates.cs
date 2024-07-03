using System.Collections;
using System.IO;
using System.Reflection;

namespace CCyberPick.Models
{
    public enum SystemState
    {
        Stopped,
        Running
    }
    public class SystemStates
    {
        #region Fields

        private static readonly object lockObject = new object();
        

        #endregion

        #region Constructor

        private SystemStates()
        {
            RobotServerIp = "10.17.2.1";
            RobotServerPort = 32001;
            LastConnectedDevice = "exo264MGE:ac-4f-fc-01-3a-53";
            ProceduresPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Lib\\HProcedures\\";
            RecipiesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Lib\\Recipes\\";
            EnviromentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Lib\\Enviroments\\";
            CalibrationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Lib\\Calibration\\";
            ImagesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Images\\";
            IsAirPressureOK = false;
            IsRobotRunning = true;
            State = SystemState.Running;
        }

        #endregion

        #region Properties

        public string RobotServerIp { get; set; }
        public int RobotServerPort { get; set; }
        public string LastConnectedDevice { get; set; }
        public string ProceduresPath { get; set; }
        public string RecipiesPath { get; set; }
        public string EnviromentPath { get; set; }
        public string CalibrationPath { get; set; }
        public string ImagesPath { get; set; }
        public bool IsAirPressureOK { get; set; }
        public bool IsRobotRunning { get; set; }
        public bool IsRobotIn { get; set; }
        public bool IsFirstCycle { get; set; }
        public SystemState State { get; set; }

        /// <summary>
        /// Singletone pattern instance
        /// </summary>
        public static SystemStates Instance
        {
            get
            {
                if (instance == null)
                    lock (lockObject)
                    {
                        if (instance == null)
                            instance = new SystemStates();
                    }

                return instance;
            }
        }

        private static SystemStates instance;

        #endregion


    }
}
