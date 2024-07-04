using HalconDotNet;
using System.IO;

namespace CCyberPick.Models
{
    public class Calibration
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public HDict HDict { get; set; }

        public bool Load()
        {
            HOperatorSet.SetSystem("clip_region", "false");

            string file = Path + Name + ".hdict";
            if (File.Exists(file))
            {
                HDict = new HDict();
                HDict.ReadDict(file, new HTuple(), new HTuple());
            }
            else return false;

            return true;
        }
    }
}

