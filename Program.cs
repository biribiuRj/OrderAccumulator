using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderAccumulator.Config;
using Serilog;

namespace OrderAccumulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("logs");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration) 
                .CreateLogger();

            try
            {
                Log.Information("Iniciando host do serviço...");
                CreateHostBuilder(args, configuration).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erro fatal ao iniciar o serviço.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddConfiguration(configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<FixSettings>(configuration.GetSection("FixSettings"));
                    services.AddHostedService<FixService>();
                });
    }
}
