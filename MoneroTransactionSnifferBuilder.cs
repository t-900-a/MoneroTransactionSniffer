using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer
{
    class MoneroTransactionSnifferBuilder
    {
        public static IHost BuildContext()
        {
            return Host.CreateDefaultBuilder().ConfigureLogging(logger =>
            {
                logger.ClearProviders();
                logger.AddConsole();
                logger.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            { 
                services.AddSingleton<IMoneroTransactionSniffer, MoneroTransactionSnifferTool>();

                services.AddOptions();
                services.Configure<TwitterApiCredentials>(context.Configuration.GetSection("TwitterApiCredentials"));
                services.Configure<MoneroNodeSettings>(context.Configuration.GetSection("MoneroNodeSettings"));
            }).Build();
        }
    }
}
