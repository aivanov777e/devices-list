using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ConsoleService: IConsoleService, IDisposable
    {
        private NetworkConfiguration netConfiguration;
        private CancellationTokenSource cancelTokenSource;
        private bool disposedValue;
        private readonly ILogger<ConsoleService> logger;
        private readonly IDeviceService deviceService;

        public ConsoleService(IOptions<NetworkConfiguration> options, ILogger<ConsoleService> logger, IDeviceService deviceService)
        {
            netConfiguration = options.Value;
            this.logger = logger;
            this.deviceService = deviceService;
        }

        public void Start()
        {
            cancelTokenSource = new CancellationTokenSource();
            var token = cancelTokenSource.Token;
            Task.Run(() => Process(token), token);
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

        async Task Process(CancellationToken token)
        {
            bool exit = false;
            while (!exit && !token.IsCancellationRequested)
            {
                Console.WriteLine("Enter 'd' to show devices, 't <ID> <HiVal> <LoVal>' for change parameter2 thresholds, Ctrl+C for quit");
                var commandString = Console.ReadLine();
                if (String.IsNullOrEmpty(commandString))
                {
                    continue;
                }
                var command = commandString.ToLower().Split(' ');
                switch (command[0])
                {
                    case "d":
                        using (var tokenSource = new CancellationTokenSource())
                        {
                            var tokenShowDevices = tokenSource.Token;
                            Task.Run(() => ShowDevices(tokenShowDevices), tokenShowDevices);
                            Console.ReadKey(true);
                            tokenSource.Cancel();
                        }
                        break;
                    case "t":
                        if (command.Length != 4)
                        {
                            Console.WriteLine("Command for change parameter2 thresholds must have 3 parameters: t <ID> <HiVal> <LoVal>");
                            break;
                        }
                        if (Int32.TryParse(command[1], out int id) && Int16.TryParse(command[2], out short hiVal) && Int16.TryParse(command[3], out short loVal))
                        {
                            logger.LogInformation("Changing parameter2 thresholds {0}: {1}, {2}", id, hiVal, loVal);
                            await deviceService.SetThresholds(id, hiVal, loVal);
                            Console.WriteLine("Command for change parameter2 thresholds sended");
                        }
                        else
                        {
                            Console.WriteLine("Command for change parameter2 thresholds must have 3 number parameters");
                        }
                        break;
                }
            }
        }
        private void ShowDevices(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine("Press any key to exit...");
                string format = "{0,6}{1,15}{2,15}{3,37}{4,8}";
                Console.WriteLine(format, "ID", "Parameter 1", "Parameter 2", "Thresholds for Parameter 2: upper,", "lower");
                foreach (var device in deviceService.GetDevices())
                {
                    Console.Write("{0,6}{1,15}", device.Id, device.Parameter1);
                    if (!device.Parameter2Correct)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.Write("{0,15}", device.Parameter2);
                    Console.ResetColor();
                    Console.WriteLine("{0,37}{1,8}", device.Parameter2ThresholdHi, device.Parameter2ThresholdLo);
                }
                Thread.Sleep(1500);
            }
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
        ~ConsoleService()
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
