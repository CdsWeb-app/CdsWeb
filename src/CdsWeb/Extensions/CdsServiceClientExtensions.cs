using CdsWeb.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Powerplatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;

namespace CdsWeb.Extensions
{
    public class CdsServiceClientOptions
    {
        /// <summary>
        /// <see cref="CdsServiceClient"/> constructors for connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Parameter to allow for transient IOrganizationService service based on Clone
        /// of singleton CdsServiceClient.
        /// </summary>
        /// <see cref="IOrganizationService"/>
        public bool IncludeIOrganizationService = true;

        /// <summary>
        /// Parameter to allow for transient OrganizationServiceContext service based on Clone
        /// of singleton CdsServiceClient.
        /// </summary>
        /// <see cref="OrganizationServiceContext"/>
        public bool IncludeOrganizationServiceContext = false;

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
                new LoggerTraceListener(
                    "Microsoft.PowerPlatform.Cds.Client",
                    sp.GetRequiredService<ILogger<LoggerTraceListener>>())
                );

            TraceControlSettings.TraceLevel = (System.Diagnostics.SourceLevels)Enum.Parse(typeof(System.Diagnostics.SourceLevels), cdsServiceClientOptions.TraceLevel);
            TraceControlSettings.AddTraceListener(services.BuildServiceProvider().GetRequiredService<LoggerTraceListener>());

            services.AddSingleton(sp =>
                new CdsServiceClient(cdsServiceClientOptions.ConnectionString));

            if (cdsServiceClientOptions.IncludeIOrganizationService)
                services.AddTransient<IOrganizationService>(sp => sp.GetService<CdsServiceClient>().Clone());

            if (cdsServiceClientOptions.IncludeIOrganizationService && cdsServiceClientOptions.IncludeOrganizationServiceContext)
            {
                services.AddTransient(sp =>
                    new OrganizationServiceContext(sp.GetService<IOrganizationService>()));
            }
            else if (cdsServiceClientOptions.IncludeOrganizationServiceContext)
            {
                services.AddTransient(sp =>
                    new OrganizationServiceContext(sp.GetService<CdsServiceClient>().Clone()));
            }
        }
    }
}
