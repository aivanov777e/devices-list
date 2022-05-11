using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IDeviceService
    {
        void Start();
        void Stop();
        IEnumerable<DeviceDto> GetDevices();
        Task SetThresholds(int id, short hiVal, short loVal);
    }
}
