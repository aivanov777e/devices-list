using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public record Device
    {
        public int Id { get; private set; }
        public short? Parameter1 { get; init; }
        public short? Parameter2 { get; init; }
 
        public short? Parameter2ThresholdLo { get; init; }
        public short? Parameter2ThresholdHi { get; init; }
        public bool Parameter2Correct
        {
            get => Parameter2 >= Parameter2ThresholdLo && Parameter2 <= Parameter2ThresholdHi;
        }

        public Device(int id)
        {
            this.Id = id;
        }
    }
}
