using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class DeviceService: IDeviceService, IDisposable
    {
        ConcurrentDictionary<int, Device> devices = new ConcurrentDictionary<int, Device>();
        private NetworkConfiguration netConfiguration;
        private CancellationTokenSource cancelTokenSource;
        private bool disposedValue;
        private readonly ILogger<DeviceService> logger;
        private readonly IMapper mapper;

        public DeviceService(IOptions<NetworkConfiguration> options, ILogger<DeviceService> logger, IMapper mapper) {
            netConfiguration = options.Value;
            this.logger = logger;
            this.mapper = mapper;
        }
        public void Start()
        {
            cancelTokenSource = new CancellationTokenSource();
            var token = cancelTokenSource.Token;
            Task.Run(() => Listen(token), token);
        }

        public void Stop()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
                cancelTokenSource = null;
            }
        }

        public IEnumerable<DeviceDto> GetDevices()
        {
            var result = mapper.Map<IEnumerable<DeviceDto>>(devices.Values);
            return result;
        }

        private void Listen(CancellationToken token)
        {
            logger.LogInformation("Listen ...");
            try
            {
                UdpClient client = new UdpClient(netConfiguration.LocalPort);
                IPEndPoint ip = null;
                while (!token.IsCancellationRequested)
                {
                    byte[] data = client.Receive(ref ip);
                    logger.LogInformation("Listen take data len {0}", data.Length);
                    Task.Run(() => ReadDatagram(data));
                }
                client.Close();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            logger.LogInformation("Listen stoped...");
        }

        private void ReadDatagram(byte[] data)
        {
            logger.LogInformation("ReadDatagram...");
            if (data.Length == 8)
            {
                Int32 id = BitConverter.ToInt32(data, 0);
                Int16 parameter1 = BitConverter.ToInt16(data, 4);
                Int16 parameter2 = BitConverter.ToInt16(data, 6);

                //var device = devices.GetOrAdd(id, CreateDevice);
                //var newdevice = device with { Parameter1 = parameter1, Parameter2 = parameter2 };
                devices.AddOrUpdate(id, 
                    (id)=>CreateDevice(id, parameter1:parameter1, parameter2:parameter2), 
                    (id, d) => d with { Parameter1 = parameter1, Parameter2 = parameter2 });
                //device.Parameter1 = parameter1;
                //device.Parameter2 = parameter2;

                logger.LogInformation("ReadDatagram {0}: {1}, {2}", id, parameter1, parameter2);
            }
            else if (data.Length == 12)
            {
                Int32 id = BitConverter.ToInt32(data, 0);
                var command = Encoding.UTF8.GetString(data, 4, 2);
                var status = (StatusResponse)BitConverter.ToUInt16(data, 6);
                Int16 parameter2ThresholdHi = BitConverter.ToInt16(data, 8);
                Int16 parameter2ThresholdLo = BitConverter.ToInt16(data, 10);

                if (status == StatusResponse.Successes)
                {
                    //var device = devices.GetOrAdd(id, CreateDevice);
                    //device.Parameter2ThresholdHi = parameter2ThresholdHi;
                    //device.Parameter2ThresholdLo = parameter2ThresholdLo;
                    devices.AddOrUpdate(id,
                        (id) => CreateDevice(id, parameter2ThresholdHi: parameter2ThresholdHi, parameter2ThresholdLo: parameter2ThresholdLo),
                        (id, d) => d with { Parameter2ThresholdHi = parameter2ThresholdHi, Parameter2ThresholdLo = parameter2ThresholdLo });

                }

                logger.LogInformation("ReadDatagram {0}: {1}, {2}, {3}, {4}",
                    id, command, status, parameter2ThresholdHi, parameter2ThresholdLo);
            }
        }

        private Device CreateDevice(int id)
        {
            var device = new Device(id);

            Task.Run(() =>
            {
                byte[] data = new byte[6];
                BitConverter.GetBytes(id).CopyTo(data, 0);
                string message = "LR";
                Encoding.UTF8.GetBytes(message).CopyTo(data, 4);

                logger.LogInformation("Sending LR for id {0} ...", id);
                UdpClient client = new UdpClient();
                client.Send(data, data.Length, "255.255.255.255", netConfiguration.RemotePort);
                client.Close();
            });

            return device;
        }
        private Device CreateDevice(int id,
            short? parameter1 = null, short? parameter2 = null,
            short? parameter2ThresholdHi = null, short? parameter2ThresholdLo = null)
        {
            var device = new Device(id) { 
                Parameter1 = parameter1, 
                Parameter2 = parameter2, 
                Parameter2ThresholdHi = parameter2ThresholdHi, 
                Parameter2ThresholdLo = parameter2ThresholdLo };

            Task.Run(() =>
            {
                byte[] data = new byte[6];
                BitConverter.GetBytes(id).CopyTo(data, 0);
                string message = "LR";
                Encoding.UTF8.GetBytes(message).CopyTo(data, 4);

                logger.LogInformation("Sending LR for id {0} ...", id);
                UdpClient client = new UdpClient();
                client.Send(data, data.Length, "255.255.255.255", netConfiguration.RemotePort);
                client.Close();
            });

            return device;
        }

        public async Task SetThresholds(int id, short hiVal, short loVal)
        {
            byte[] data = new byte[10];
            BitConverter.GetBytes(id).CopyTo(data, 0);
            string message = "LW";
            Encoding.UTF8.GetBytes(message).CopyTo(data, 4);
            BitConverter.GetBytes(hiVal).CopyTo(data, 6);
            BitConverter.GetBytes(loVal).CopyTo(data, 8);

            logger.LogInformation("Sending LW ...");
            UdpClient client = new UdpClient();
            await client.SendAsync(data, data.Length, "255.255.255.255", netConfiguration.RemotePort);
            client.Close();

            logger.LogInformation("Command for change parameter2 thresholds sended: id {0}: hiVal {1}, loVal {2}", id, hiVal, loVal);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }
                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                Stop();
                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        ~DeviceService()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
