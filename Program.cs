
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WeighingScaleEmulator
{
    class Program
    {
        static DateTime lastSend;
        public static IConfiguration Configuration { get; }

        static void Main(string[] args)
        {
            // Setup Host
            var host = CreateDefaultBuilder().Build();

            // Invoke Worker
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            var workerInstance = provider.GetRequiredService<Worker>();
            workerInstance.Client("http://10.4.5.132:1009/ec-hub", "Client 1", "E");

            host.Run();
        }

        static IHostBuilder CreateDefaultBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(app =>
                {
                    app.AddJsonFile("appsettings.json");
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<Worker>();
                });
        }
        //static async Task Main(string[] args)
        //{

        //    await Client("http://10.4.5.132:1009/ec-hub", "Client 1", "E");

        //}


        static async Task Client(string path, string name, string building)
        {

            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
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
            await _connection.StartAsync();
            Console.WriteLine(_connection.State);


            while (true)
            {
                Parallel.Invoke(

                async () =>
                {
                    Thread.Sleep(300);

                    double g = Math.Round(RandomNumber(appSettings.RandomStart, appSettings.RandomEnd), 2);
                    await _connection.InvokeAsync("Welcom", "4", g + "", appSettings.MachineID);
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
