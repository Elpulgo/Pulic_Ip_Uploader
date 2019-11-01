using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PublicIpUploader
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (!SetupLogger())
                return;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var executioner = serviceProvider.GetRequiredService<IExecutioner>();

            executioner.ExecuteAsync().Wait();

            Log.Information("Shuting down program.");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog());

            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IConfigurationSupplier, ConfigurationSupplier>();

            services.AddTransient<ILocalStore, LocalStore>();
            services.AddTransient<IExecutioner, Executioner>();
        }

        private static bool SetupLogger()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "logs", "publicipuploader.log"))
                    .CreateLogger();

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to setup logger '{exception.Message}'");
            }

            return false;
        }
    }
}
