using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeighingScaleEmulator
{
    internal class Worker
    {
        public IConfiguration _configuration;
        public Worker(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task Client(string path, string name, string building)
        {

            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            var _connection = new HubConnectionBuilder()
              .WithUrl(appSettings.SignalrUrl)
             .Build();

            _connection.On<string, string, string>("Welcom", (scalingMachineID, amount, unit) =>
            {
                string text = unit != "g" ? $"{name} The big one: " : $"{name} The small one: ";
                string newMessage = $"#### ### {text} {scalingMachineID}: {amount}{unit} {building}";
                Console.ForegroundColor = unit != "g" ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine(newMessage);
            });
            _connection.StartAsync();
            Console.WriteLine(_connection.State);


            while (true)
            {
                Parallel.Invoke(

                async () =>
                {
                    Thread.Sleep(300);

                    double g = Math.Round(RandomNumber(appSettings.RandomStart, appSettings.RandomEnd), 2);
                    await _connection.InvokeAsync("Welcom", appSettings.MachineID, g + "", appSettings.Unit);
                }
                );

                Thread.Sleep(300);

            }
        }

        public static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        
    }
}
