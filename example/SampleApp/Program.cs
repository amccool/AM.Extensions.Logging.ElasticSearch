using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ElasticSearch.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Create service collection
            var serviceCollection = new ServiceCollection();
            var serviceProvider = ConfigureServices(serviceCollection);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            //do the actual work here
            var bar = serviceProvider.GetService<IApp>();

            Task.Run(() => bar.Run()).Wait();

            logger.LogDebug("All done!");

            Console.ReadLine();
        }


        private static IServiceProvider ConfigureServices(IServiceCollection serviceCollection)
        {
            //setup our DI
            serviceCollection
                .AddSingleton(new LoggerFactory()
                    .AddConsole(LogLevel.Trace)
                    .AddDebug()
                    .AddElasticSearch(new Uri(@"http://localhost:9200/"), 
                        LogLevel.Trace)
                )
                .AddSingleton<IApp, App>();
            
            // add logging
            serviceCollection.AddLogging();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
