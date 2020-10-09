using System;
using System.Net.Http.Headers;
using AnimusHeart2Influx.Animus;
using AnimusHeart2Influx.Influx;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace AnimusHeart2Influx
{
    public class Program
    {
        private static readonly char[] Token = "".ToCharArray();

        public static int Main(string[] args)
        {




            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Logger = host.Services.GetRequiredService<ILogger>();

                Log.Information("Starting service host");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File("logs//log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var animusUrl = hostContext.Configuration.GetValue<string>("AnimusUrl");
                    var animusKey = hostContext.Configuration.GetValue<string>("AnimusKey");
                    var influxDbUrl = hostContext.Configuration.GetValue<string>("InfluxDbUrl");
                    var maxWebSocketMessagesPerHour = hostContext.Configuration.GetValue<int>("MaxWebSocketMessagesPerHour");
                    var animusConfiguration = new AnimusConfiguration(animusKey, animusUrl);

                    services.AddHostedService<Worker>();
                    services.AddTransient<IAnimusWebSocketEventHandler, AnimusWebSocketEventHandler>();
                    services.AddHttpClient<AnimusHeartHttpClient>(c =>
                    {
                        c.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer",animusKey);
                        c.BaseAddress = new Uri(animusUrl);
                    });
                    services.AddTransient<IInfluxService, InfluxServiceService>();
                    services.AddSingleton(new MessageCounter(maxWebSocketMessagesPerHour));
                    services.AddTransient<SlotCounter>();
                    services.AddTransient<IRight, Right>();
                    services.AddTransient<IAnimusWebSocketHandler, AnimusWebSocketHandler>(f => new AnimusWebSocketHandler(animusConfiguration));
                    services.AddTransient(sp => InfluxDBClientFactory.Create(influxDbUrl, Token));
                });
    }
}
