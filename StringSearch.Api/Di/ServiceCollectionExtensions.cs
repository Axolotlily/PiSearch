﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StringSearch.Api.Health;
using StringSearch.Api.Infrastructure.Config;
using StringSearch.Api.Infrastructure.Di;

namespace StringSearch.Api.Di
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterApiDependencies(this IServiceCollection services)
        {
            services
                .RegisterInfrastructureDependencies()
                .registerHealthDependencies();

            return services;
        }

        private static IServiceCollection registerHealthDependencies(this IServiceCollection services)
        {
            // Digits file (critical)
            services.AddSingleton<IHealthResource>(provider =>
            {
                StringSearchConfig config = provider.GetService<StringSearchConfig>();
                return new FileHealthResource("Absolute Digits File", true, config.AbsoluteDigitsPath);
            });

            // Suffix array file (critical)
            services.AddSingleton<IHealthResource>(provider =>
            {
                StringSearchConfig config = provider.GetService<StringSearchConfig>();
                return new FileHealthResource("Suffix array file", true, config.AbsoluteSuffixArrayPath);
            });

            // Precomputed search results directory (critical if specified, but optional)
            //  Note: despite being optional, not having this can cause massive performance degradation
            //  when searching a large number of digits for a small string 
            services.AddSingleton<IHealthResource>(provider =>
            {
                StringSearchConfig config = provider.GetService<StringSearchConfig>();
                return new OptionalDirectoryHealthResource("Precomputed search results directory", true,
                    config.AbsolutePrecomputedResultsDirPath);
            });
            
            // Database connection (critical)
            services.AddSingleton<IHealthResource>(provider =>
            {
                ILogger log = provider.GetService<ILogger>();
                MySqlConfig config = provider.GetService<MySqlConfig>();
                return new MySqlHealthResource("Database", true, log, config.ConnectionString);
            });

            // Logging database (non-critical)
            services.AddSingleton<IHealthResource>(provider =>
            {
                // Get the connection string of the Serilog MySQL sink
                // Note: this assumes that there won't be more than one MySQL sink
                IConfiguration config = provider.GetService<IConfiguration>();
                string connStr = config.GetSection("Serilog")?.GetSection("WriteTo")?.GetChildren()
                    ?.Where(s => s.GetValue<string>("Name") == "MySQL")?.FirstOrDefault()?.GetSection("Args")
                    ?.GetValue<string>("connectionString");

                if (connStr == null)
                {
                    // Note: null will still get registered as an IHealthResource.
                    //  It must be filtered out whenever they are injected.
                    return null;
                }

                ILogger log = provider.GetService<ILogger>();
                return new MySqlHealthResource("Logs Database", false, log, connStr);
            });
            
            return services;
        }
    }
}
