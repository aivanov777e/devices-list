using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public short? Parameter1 { get; set; }
        public short? Parameter2 { get; set; }

        public short? Parameter2ThresholdLo { get; set; }
        public short? Parameter2ThresholdHi { get; set; }
        public bool Parameter2Correct { get; set; }
    }
}
