﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Hosting;
using StringSearch.Infrastructure.Logging.Extensions;
using StringSearch.Versioning;
using ILogger = Serilog.ILogger;

namespace StringSearch.Infrastructure.Di
{
    internal static class LoggingRegistrar
    {
        public static IServiceCollection AddLogging(this IServiceCollection services)
        {
            services.AddSingleton<LoggerConfiguration>(provider => new LoggerConfiguration()
                .ReadFrom.Configuration(provider.GetService<IConfiguration>())
                .Enrich.FromLogContext()
                .Enrich.WithVersion(provider.GetService<IVersionProvider>()));

            services.AddSingleton<ILogger>(provider => provider.GetService<LoggerConfiguration>().CreateLogger());
            services.AddSingleton<ILoggerFactory>(provider => new SerilogLoggerFactory(provider.GetService<ILogger>()));

            return services;
        }
    }
}