using CCyberPick.Models.Devices;
using CCyberPick.Models.VisionEngines;
using HalconDotNet;
using INITUBE.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VO_PICKER.Models;

namespace CCyberPick.Models
{
    public class Cycle
    {

        #region Fields

        //private static readonly object lockObject = new object();
        private readonly RobotHandler robotHandler = RobotHandler.Instance;
        private readonly SystemStates systemStates = SystemStates.Instance;
        private readonly FlexiBowlHandler flexibowlHandler = FlexiBowlHandler.Instance;
        //private readonly AreaScanCamera cameraHandler = AreaScanCamera.Instance;
        private readonly ModbusHandler modbusHandler = ModbusHandler.Instance;

        // Define an event using the EventHandler delegate.
        public event EventHandler UpdateCycleOutputEvent;
        public event EventHandler UpdateImage;
        public event EventHandler UpdateCurrentRecipe;
        public event EventHandler UpdateEnviroment;
        public event EventHandler UpdateCycleState;
        public event EventHandler UpdateCandidates;
        public event EventHandler UpdatePickRatio;


        public IDevice mainDevice;
        private HEngine visionEngine;
        private Recipes currentRecipe;
        private Enviroment currentEnviroment;
        private Calibration currentCalibration;

        // Algorithm dicts
        HDict checkQuantityDict = new HDict();
        HDict findDict = new HDict();


        private bool CycleRunning = false;
        private HObject myImage;

        // 
        private int Rotation = 0;
        private bool[] ToFeed;
        Timer pickTimer;
        #endregion

        #region Constructor
        public Cycle()
        {
            
            mainDevice = new SVSCamera();

            currentRecipe = new Recipes()
            {
                Name = "7863",
                Path = SystemStates.Instance.RecipiesPath
            };
            _ = currentRecipe.Load();

            currentEnviroment = new Enviroment()
            {
                Name = "Enviroment",
                Path = SystemStates.Instance.EnviromentPath
            };
            _ = currentEnviroment.Load();

            currentCalibration = new Calibration()
            {
                Name = "CalibrationDict_New",
                Path = SystemStates.Instance.CalibrationPath
            };
            _ = currentCalibration.Load();

            // Set parameters to the algorithm dicts
            checkQuantityDict.SetDictObject(currentEnviroment.HDict.GetDictObject("ROI"), "ROI");

            findDict.SetDictObject(currentEnviroment.HDict.GetDictObject("ROI"), "ROI");
            findDict.SetDictTuple("ModelID", currentRecipe.HDict);
            findDict.SetDictTuple("RefRow", currentCalibration.HDict.GetDictTuple("RefRow"));
            findDict.SetDictTuple("RefCol", currentCalibration.HDict.GetDictTuple("RefCol"));
            findDict.SetDictTuple("Scale", currentCalibration.HDict.GetDictTuple("Scale"));
            findDict.SetDictObject(currentCalibration.HDict.GetDictObject("Map"), "Map");

            pickTimer = new Timer(ProcessPickRatio, robotHandler.NumCadidates, 5, 30000);

        }
        #endregion

        #region Properties



        #endregion

        #region Methods
        private void ProcessPickRatio(object obj)
        {
            //Console.WriteLine("Timer executed");
            //if (obj != null)
            UpdatePickRatio?.Invoke(robotHandler.NumCadidates, EventArgs.Empty);
            robotHandler.NumCadidates = 0;
        }
        public bool StartEngine()
        {
            visionEngine = new HEngine();

            bool result = visionEngine.Init(SystemStates.Instance.ProceduresPath);

            return result;
        }

        public bool StartRobotCom()
        {
            //var result = true;


            var result = robotHandler.StartTCPServer(systemStates.RobotServerIp, systemStates.RobotServerPort);

            return result;
        }

        public bool StartModbusCom()
        {
            var result = modbusHandler.InitCom();

            if (result)
            {
                modbusHandler.UpdateWarningLight(255, "red");
            }

            return result;
        }

        public bool StartFlexiBowlCom()
        {
            var result = flexibowlHandler.InitClientCOM();
            return result;
        }

        public bool ConnectLastCamera()
        {
            UpdateCurrentRecipe?.Invoke(currentRecipe.Name, EventArgs.Empty);
            UpdateEnviroment?.Invoke(currentEnviroment.Name, EventArgs.Empty);

            if (SystemStates.Instance.LastConnectedDevice != "")
            {
                bool result = mainDevice.Open(SystemStates.Instance.LastConnectedDevice);

                return result;
            }
            else return false;
        }

        public void StartCycle()
        {
            UpdateCurrentRecipe?.Invoke(currentRecipe.Name, EventArgs.Empty);
            UpdateEnviroment?.Invoke(currentEnviroment.Name, EventArgs.Empty);
            if (!CycleRunning)
            {
                Task task = new Task(() => { CycleTask(); });
                CycleRunning = true;
                UpdateCycleState?.Invoke(CycleRunning, EventArgs.Empty);
                

                task.Start();
            }
        }

        public void StopCycle()
        {
            if (CycleRunning)
            {
                CycleRunning = false;
                UpdateCycleState?.Invoke(CycleRunning, EventArgs.Empty);
            }
        }

        public void Find()
        {
            //HObject obj;
            //HOperatorSet.ReadObject(out obj, @"F:\BM Development\BM Development\CCyberPick\CCyberPick\bin\x64\Debug\Images\7861\31 Oct 23 - 14_43_05.hobj");
            //HOperatorSet.SelectObj(obj, out HObject backImage, 2);
            //HOperatorSet.SelectObj(obj, out HObject frontImage, 1);
            var backImage = SnapBackImage(16000);
            var frontImage = SnapFrontImage(3000);
            SaveData(backImage, frontImage);

            // Find candidates
            var candidates = FindCandidates(backImage, frontImage, out List<string[]> toShow);
            //MessageBox.Show($"Result Pose:\n - X: {candidates[0][0]}\n - Y: {candidates[0][1]}\n - ZRot: {candidates[0][2]}");
            robotHandler.Candidates.Clear();

            if (candidates.Any())
            {
                foreach (var candidate in candidates)
                    robotHandler.Candidates.Enqueue(candidate);
            }
            
            UpdateImage?.Invoke(myImage, EventArgs.Empty);
            UpdateCandidates?.Invoke(toShow, EventArgs.Empty);

        }

        private void CycleTask()
        {
            modbusHandler.UpdateWarningLight(255, "green");
            //modbusHandler.SwitchBackLight(1);

            int rotationStep = 45;
            int feedStep = 90;
            int quadrants = 360 / feedStep;
            ToFeed = new bool[quadrants];

            Console.WriteLine("Cycle task started.");
            // Turn on Backlight
            // modbusHandler.SwitchBackLight(1);
            robotHandler.Candidates.Clear();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int quadrantIndex = 0;
            while (SystemStates.Instance.State == SystemState.Running)
            {
                if (robotHandler.Candidates.Count == 0)
                {
                    // First, take a snap for Backligth
                    Stopwatch sw = Stopwatch.StartNew();

                    while (SystemStates.Instance.IsRobotIn)
                    {
                        Thread.Sleep(1);
                    }

                    var sw2 = Stopwatch.StartNew();
                    // Snap Back image
                    var backImage = SnapBackImage(15000);

                    // Check the quantity threshold
                    if (Rotation % 90 == 0 && Rotation <= 360) //Ensures the rotation is in one of the specified angles (0, 90, 180, 360)
                    {
                        quadrantIndex = (Rotation / 90) % quadrants;
                        var quantity = CheckQuantity(backImage);
                        if (quantity < 15)
                            ToFeed[quadrantIndex] = true;
                        else
                            ToFeed[quadrantIndex] = false;
                    }

                    // Snap Front image
                    var frontImage = SnapFrontImage(3000);
                    sw2.Stop();
                    Console.WriteLine($"*** Captured at: {DateTime.Now:hh-mm-ss-fff} time: {sw2.ElapsedMilliseconds}");

                    // Find candidates
                    var candidates = FindCandidates(backImage, frontImage, out List<string[]> toShow);
                    //UpdateCycleOutputEvent?.Invoke(toShow, EventArgs.Empty);
                    UpdateCandidates?.Invoke(toShow, EventArgs.Empty);

                    // Save Data
                    //SaveData(backImage, frontImage);

                    if (candidates.Any())
                    {
                        //totalCandidates += candidates.Count;
                        //findTry = 0;
                        foreach (var candidate in candidates)
                            robotHandler.Candidates.Enqueue(candidate);

                        Console.WriteLine($"*** Candidates loaded at: {DateTime.Now:hh-mm-ss-fff}");
                    }
                    else
                    {
                        // Rotate flexibowl
                        // Wait for feeder to finish
                        //while (modbusHandler.IsFeeding)
                        //{
                        //    Thread.Sleep(1);
                        //}
                        //quadrantIndex = (Rotation / feedStep) % quadrants;
                        if (Rotation % 90 == 0 && Rotation <= 360) //Ensures the rotation is in one of the specified angles (0, 90, 180, 360)
                        {
                            int contraryIndex = (quadrantIndex + quadrants / 2) % quadrants;
                            int trueCount = ToFeed.Count(b => b);
                            int feedTime = 2000;
                            if (trueCount >= 2)
                                feedTime = 2000;

                            if (ToFeed[contraryIndex])
                            {
                                modbusHandler.Feed(feedTime);
                                ToFeed[contraryIndex] = false;
                            }
                        }
                        var commandSent = flexibowlHandler.SendCommand(flexibowlHandler.FlexiBowlCommands["Move - Flip"]);
                        if (commandSent)
                        {
                            // Wait for movement finish
                            bool finish = false;
                            while (!finish)
                            {
                                finish = FlexiBowlHandler.Instance.CheckStatus();
                                Thread.Sleep(10);
                            }
                            Rotation = (Rotation + rotationStep) % 360;
                        }

                    }

                    sw.Stop();
                    Console.WriteLine($"Process time: {sw.ElapsedMilliseconds}");
                    //UpdateCycleOutputEvent?.Invoke(myImage, EventArgs.Empty);
                    UpdateImage?.Invoke(myImage, EventArgs.Empty);
                }
            }

            // Turn off Backlight
            modbusHandler.SwitchBackLight(0);
            modbusHandler.UpdateWarningLight(255, "red");
            Console.WriteLine("Cycle task finished.");
        }

        public List<string> DiscoverDevices()
        {
            var result = mainDevice.Discover();
            return new List<string>(result);
        }

        public bool OpenDevice(string selectedDevice)
        {
            var result = mainDevice.Open(selectedDevice);
            return result;
        }

        public HObject Grab()
        {
            var result = mainDevice.GrabData();
            HObject obj = new HObject();

            if (result.ImageSize > 0)
            {
                HOperatorSet.GenImage1(out obj, "byte", result.Width, result.Height, result.ImagePtr);
                //obj.WriteObject("myObj.hobj");
            }

            return obj;

        }


        public HObject SnapBackImage(int ExposureTime) 
        {
            // Snap Back image
            //modbusHandler.SwitchFrontLight(0);
            modbusHandler.SwitchBackLight(1);
            Thread.Sleep(10);
            mainDevice.SetParam("ExposureTime", ExposureTime);
            var backImage = Grab();
            modbusHandler.SwitchBackLight(0);

            return backImage;
        }

        public HObject SnapFrontImage(int ExposureTime)
        {
            // Snap Front image
            //modbusHandler.SwitchBackLight(0);
            //Thread.Sleep(20);
            modbusHandler.SwitchFrontLight(1);
            Thread.Sleep(10);
            mainDevice.SetParam("ExposureTime", ExposureTime);
            var frontImage = Grab();
            //Thread.Sleep(10);
            modbusHandler.SwitchFrontLight(0);
            //Thread.Sleep(10);
            //modbusHandler.SwitchFrontLight(0);
            return frontImage;
        }
        #endregion

        #region Private Methods

        private double CheckQuantity(HObject Image)
        {
            var sw = Stopwatch.StartNew();
            // Set incoming parameters
            checkQuantityDict.SetDictObject(Image, "ImageBL");

            // Call procedure
            visionEngine.HPC_CheckPartQuantity.SetInputCtrlParamTuple("ParamsDict", checkQuantityDict);
            //visionEngine.HDevEngine.StartDebugServer();
            visionEngine.HPC_CheckPartQuantity.Execute();
            //visionEngine.HDevEngine.StopDebugServer();

            // Get procedure results
            HTuple resultsTuple0 = visionEngine.HPC_CheckPartQuantity.GetOutputCtrlParamTuple("ResultsDict");
            HDict resultDict0 = new HDict(resultsTuple0.H);

            var CandidatesPercent = (double)resultDict0.GetDictTuple("Value");

            sw.Stop();
            Console.WriteLine("*** Check quantity time: "+ sw.ElapsedMilliseconds);
            return Math.Round(CandidatesPercent, 2);

        }

        public void SaveData(HObject backImage, HObject frontImage)
        {
            DateTime dateTime = DateTime.Now;

            if (!Directory.Exists(SystemStates.Instance.ImagesPath))
                Directory.CreateDirectory(SystemStates.Instance.ImagesPath);

            HObject obj;
            HOperatorSet.ConcatObj(frontImage, backImage, out obj);

            obj.WriteObject(SystemStates.Instance.ImagesPath + $"\\{dateTime:dd MMM yy - HH_mm_ss}");

        }
        private List<double[]> FindCandidates(HObject backImage, HObject frontImage, out List<string[]> candidatesToShow)
        {
            var sw = Stopwatch.StartNew();
            candidatesToShow = new List<string[]>();

            // Set incoming parameters
            findDict.SetDictObject(backImage, "ImageBL");
            findDict.SetDictObject(frontImage, "ImageFL");
            
            // ['ImageSearch', 'ModelID', 'RobotRef', 'PickPoint', 'CamParams','CamPose', 'Map']
            visionEngine.HPC_FindShapeModel.SetInputCtrlParamTuple("ParamsDict", findDict);

            //visionEngine.HDevEngine.StartDebugServer();
            visionEngine.HPC_FindShapeModel.Execute();
            //visionEngine.HDevEngine.StopDebugServer();

            // Get result parameters.
            HTuple resultsTuple = visionEngine.HPC_FindShapeModel.GetOutputCtrlParamTuple("ResultsDict");
            HDict resultDict = new HDict(resultsTuple.H);

            myImage = resultDict.GetDictObject("ImageResult");
            var Scores = resultDict.GetDictTuple("Score");
            var _scores = Scores.ToDArr();

            List<double[]> foundCandidates = new List<double[]>();

            if (_scores.Any())
            {
                var XTrans = resultDict.GetDictTuple("Xpose");
                var YTrans = resultDict.GetDictTuple("Ypose");
                var ZRot = resultDict.GetDictTuple("RZpose");
                var Face = resultDict.GetDictTuple("Face");

                // Convert values to array
                var _xTrans = XTrans.ToDArr();
                var _yTrans = YTrans.ToDArr();
                var _rot = ZRot.ToDArr();
                var _face = Face.ToSArr();

                for (int i = 0; i < Scores.Length; i++)
                {

                    string[] temp = { (i + 1).ToString(), _face[i], _xTrans[i].ToString("F2"), _yTrans[i].ToString("F2"), _rot[i].ToString("F2"), _scores[i].ToString("F2") };

                    candidatesToShow.Add(temp);
                    var angle = _rot[i];
                    //Apply correction to angle
                    if (_face[i] == "B")
                    {
                        if (angle < 0)
                        {
                            var corrValue = 0.015;
                            var reduction = corrValue * angle;
                            angle += reduction;
                        }
                        else
                        {
                            var corrValue = 0.01;
                            var reduction = corrValue * angle;
                            angle -= reduction;
                        }
                    }
                    else if (_face[i] == "A")
                    {
                        if (angle < 0)
                        {
                            var corrValue = 0.008;
                            var reduction = corrValue * angle;
                            angle += reduction;
                        }
                        else
                        {
                            var corrValue = 0.008;
                            var reduction = corrValue * angle;
                            angle -= reduction;
                        }
                    }


                    double[] candidate = { _xTrans[i], _yTrans[i], angle, 0 }; // Face A

                    if (_face[i] == "B")
                        candidate[3] = 1;
                    if (_face[i] == "-")
                        Console.WriteLine("*** NO FACE ****");

                    //if (_face[i] == "B")
                        foundCandidates.Add(candidate);
                }
            }
            sw.Stop();
            Console.WriteLine("*** Find Cadidates: "+sw.ElapsedMilliseconds);
            return foundCandidates;
        }

        #endregion
    }
}
