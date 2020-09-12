using CdsWeb.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Threading.Tasks;

namespace CdsWeb.Extensions
{
    public class CdsServiceClientCore : IOrganizationService
    {
        public readonly CdsServiceClient _client;

        public CdsServiceClientCore(CdsServiceClientWrapper client)
        {
            _client = client.CdsServiceClient.Clone();
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            _client.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public Guid Create(Entity entity)
        {
            return _client.Create(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            _client.Delete(entityName, id);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            _client.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return _client.Execute(request);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return _client.Retrieve(entityName, id, columnSet);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return _client.RetrieveMultiple(query);
        }

        public void Update(Entity entity)
        {
            _client.Update(entity);
        }
    }

    /// <summary>
    /// CdsServiceClient wrapper to be used as singleton registration
    /// </summary>
    public class CdsServiceClientWrapper
    {
        public readonly CdsServiceClient CdsServiceClient;

        public CdsServiceClientWrapper(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            var cdsOptions = configuration.GetSection("CdsServiceClient").Get<CdsServiceClientOptions>();

            TraceControlSettings.TraceLevel =
                (System.Diagnostics.SourceLevels)Enum.Parse(
                    typeof(System.Diagnostics.SourceLevels), cdsOptions.TraceLevel);

            TraceControlSettings.AddTraceListener(
                new LoggerTraceListener(
                    "Microsoft.PowerPlatform.Cds.Client", loggerFactory.CreateLogger<CdsServiceClientWrapper>()
                    )
                );

            CdsServiceClient = new CdsServiceClient(cdsOptions.ConnectionString);
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
        /// Includes a CdsServiceClient as a singleton service within the Service Collection
        /// Optional include transient services for IOrganizationService <see cref="IOrganizationService"/>
        /// and OrganizationServiceContext <see cref="OrganizationServiceContext"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"><see cref="CdsServiceClientOptions"/></param>
        public static void AddCdsServiceClient(this IServiceCollection services)
        {
            services.AddSingleton<CdsServiceClientWrapper>();

            services.AddTransient<IOrganizationService, CdsServiceClientCore>();
        }

        /// <summary>
        /// Initiate the CdsServiceClient
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCdsServiceClient(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CdsServiceClientMiddleware>();
        }
    }

    public class CdsServiceClientMiddleware
    {
        private readonly RequestDelegate _next;

        public CdsServiceClientMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, CdsServiceClientWrapper svc)
        {
            await _next(httpContext);
        }
    }
}
