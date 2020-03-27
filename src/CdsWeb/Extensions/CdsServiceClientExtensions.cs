using CdsWeb.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Powerplatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;

namespace CdsWeb.Extensions
{
    /// <summary>
    /// CdsServiceClient wrapper to be used as singleton registration
    /// </summary>
    public class CdsServiceClientWrapper
    {
        public readonly CdsServiceClient CdsServiceClient;

        public CdsServiceClientWrapper(string connectionString, ILogger<CdsServiceClientWrapper> logger, string traceLevel = "Off")
        {
            TraceControlSettings.TraceLevel =
                (System.Diagnostics.SourceLevels)Enum.Parse(
                    typeof(System.Diagnostics.SourceLevels), traceLevel);

            TraceControlSettings.AddTraceListener(
                new LoggerTraceListener(
                    "Microsoft.PowerPlatform.Cds.Client", logger
                    )
                );

            CdsServiceClient = new CdsServiceClient(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
        }
    }

    /// <summary>
    /// Configuration options class definition
    /// </summary>
    public class CdsServiceClientOptions
    {
        /// <summary>
        /// <see cref="CdsServiceClient"/> constructors for connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Parameter to allow for transient OrganizationServiceContext service based on Clone
        /// of singleton CdsServiceClient.
        /// </summary>
        /// <see cref="OrganizationServiceContext"/>
        public bool IncludeOrganizationServiceContext { get; set; }

        /// <summary>
        /// Define a Trace Level for the CdsServiceClient
        /// Values are: All, Off, Critical, Error, Warning, Information, Verbose, ActivityTracing
        /// </summary>
        /// <see cref="System.Diagnostics.SourceLevels"/>
        public string TraceLevel { get; set; }
    }

    public static class CdsServiceCollectionExtensions
    {
        /// <summary>
        /// Include a CdsServiceClient as a singleton service within the Service Collection
        /// Optional include transient services for IOrganizationService <see cref="IOrganizationService"/>
        /// and OrganizationServiceContext <see cref="OrganizationServiceContext"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"><see cref="CdsServiceClientOptions"/></param>
        public static void AddCdsServiceClient(this IServiceCollection services, Action<CdsServiceClientOptions> configureOptions)
        {
            CdsServiceClientOptions cdsServiceClientOptions = new CdsServiceClientOptions();
            configureOptions(cdsServiceClientOptions);

            services.AddSingleton(sp =>
                new CdsServiceClientWrapper(
                    cdsServiceClientOptions.ConnectionString,
                    sp.GetRequiredService<ILogger<CdsServiceClientWrapper>>(),
                    cdsServiceClientOptions.TraceLevel)
                );

            services.AddTransient<IOrganizationService, CdsServiceClient>(sp =>
                sp.GetService<CdsServiceClientWrapper>().CdsServiceClient.Clone());

            if (cdsServiceClientOptions.IncludeOrganizationServiceContext)
            {
                services.AddTransient(sp =>
                    new OrganizationServiceContext(sp.GetService<IOrganizationService>()));
            }
        }
    }
}
