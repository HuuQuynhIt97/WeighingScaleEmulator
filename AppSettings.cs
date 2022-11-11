using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeighingScaleEmulator
{
    public class AppSettings
    {
        public string SignalrUrl { get; set; }
        public double RandomStart { get; set; }
        public double RandomEnd { get; set; }
        public string MachineID { get; set; }
        public string Unit { get; set; }
    }
}
