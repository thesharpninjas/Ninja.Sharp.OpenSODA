using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ninja.Sharp.OpenSODA.Driver.Rest.Extensions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Extensions;

namespace Ninja.Sharp.OpenSODA.Client
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync();

            IExecutorService myService = host.Services.GetRequiredService<IExecutorService>();
            await myService.ExecuteAsync(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureServices((hostContext, services) =>
             {
                 var builder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json", optional: false)
                   .AddUserSecrets(typeof(Program).Assembly)
                   .AddEnvironmentVariables();

                 var configuration = builder.Build();

                 services
                    .AddSingleton<IConfiguration>(x => configuration)
                    //.AddOracleSodaRestServices(configuration)
                    //.AddOracleSodaSqlNativeServices(configuration)
                    .AddOracleSodaSqlQbeServices(configuration)
                    .AddScoped<IExecutorService, ExecutorService>()
                    .BuildServiceProvider();
             });
    }
}