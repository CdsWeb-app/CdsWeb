using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CdsWeb.Extensions
{
    /// <summary>
    /// Required options to setup Power Apps Portals Authentication
    /// </summary>
    public class PowerAppsPortalsOptions
    {
        public string Domain { get; set; }

        public string ApplicationId { get; set; }
    }


    /// <summary>
    /// Implementation of OAuth 2.0 Implicit Grant Flow for Power Apps Portals
    /// https://docs.microsoft.com/en-us/powerapps/maker/portals/oauth-implicit-grant-flow
    /// ----
    /// Original implementation from @koolin
    /// https://github.com/koolin/dynamics-portal-buddy/blob/bouncycastle-auth/src/WebApp/Startup.cs
    /// ----
    /// Service Collection extension implementation from @CrazyTuna
    /// https://github.com/CrazyTuna/d365-portal-companion-app-auth
    /// </summary>
    public static class PowerAppsPortalsAuth
    {
        public static AuthenticationBuilder AddPowerAppsPortalAuthentication(this IServiceCollection services, Action<PowerAppsPortalsOptions> configureOptions)
        {
            PowerAppsPortalsOptions powerappsPortalOptions = new PowerAppsPortalsOptions();
            configureOptions(powerappsPortalOptions);

            return services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // JwtBearerEvents available for debugging or additional modification
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnChallenge = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.FromResult(0);
                    },
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Clock skew compensates for server time drift.
                    // We recommend 5 minutes or less:
                    ClockSkew = TimeSpan.FromMinutes(5),
                    // Specify the key used to sign the token:
                    RequireSignedTokens = true,
                    IssuerSigningKey = GetSecurityKey(powerappsPortalOptions).Result,
                    // Ensure the token hasn't expired:
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    // Ensure the token audience matches our audience value (default true):
                    ValidateAudience = false,
                    ValidAudience = powerappsPortalOptions.ApplicationId,
                    // Ensure the token was issued by a trusted authorization server (default true):
                    ValidateIssuer = true,
                    ValidIssuer = powerappsPortalOptions.Domain
                };
            });
        }

        private static async Task<SecurityKey> GetSecurityKey(PowerAppsPortalsOptions options)
        {
            string content = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://{options.Domain}/_services/auth/publickey").ConfigureAwait(false);
                content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var rs256Token = content.Replace("-----BEGIN PUBLIC KEY-----", "");
            rs256Token = rs256Token.Replace("-----END PUBLIC KEY-----", "");
            rs256Token = rs256Token.Replace("\n", "");
            var keyBytes = Convert.FromBase64String(rs256Token);

            var asymmetricKeyParameter = PublicKeyFactory.CreateKey(keyBytes);
            var rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;

            var rsa = new RSACryptoServiceProvider();
            var rsaParameters = new RSAParameters
            {
                Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned(),
                Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned()
            };
            rsa.ImportParameters(rsaParameters);

            return new RsaSecurityKey(rsa);
        }
    }

    /// <summary>
    /// Convinence methods to get currently signed in contact
    /// Original source Dynamics Portal Buddy by @koolin
    /// https://github.com/koolin/dynamics-portal-buddy/blob/master/src/WebApp/Extensions/OrganizationServiceExtensions.cs
    /// </summary>

    public static class OrganizationServiceExtensions
    {
        /// <summary>
        /// get Cds Contact based on <see cref="ClaimsIdentity"/>
        /// based on OID claim and fallback to nameidentifier used in Power Apps Portal auth
        /// </summary>
        /// <param name="service"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static Entity GetContact(this IOrganizationService service, ClaimsIdentity identity)
        {
            // Get by Azure AD B2C oid claim
            var oid = identity.FindFirst("oid")?.Value;
            var objId = identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (oid != null || objId != null)
            {
                return service.GetContactByExternalIdentityUsername(oid ?? objId);
            }

            // Get by Power Apps portal contactid nameidentifier claim
            var contactid = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (contactid != null)
            {
                return service.GetContactById(new Guid(contactid));
            }

            return null;
        }

        /// <summary>
        /// Execute a retrieve to contact based on contactid
        /// </summary>
        /// <param name="service"></param>
        /// <param name="contactid"></param>
        /// <returns>Contact <see cref="Entity"/> with all attributes</returns>
        public static Entity GetContactById(this IOrganizationService service, Guid contactid)
        {
            var contactResponse = service.Retrieve("contact", contactid, new ColumnSet(true));

            return contactResponse;
        }

        /// <summary>
        /// Execute retrieve multiple for contact based on adx_externalidentity linked-entity
        /// filter condition for the username of the external idetnity.  Return the Single item
        /// that is expected or default (null)
        /// </summary>
        /// <param name="service"></param>
        /// <param name="username"></param>
        /// <returns>Contact <see cref="Entity"/> with all attributes</returns>
        public static Entity GetContactByExternalIdentityUsername(this IOrganizationService service, string username)
        {
            var fetchxml =
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' nolock='true'>
                      <entity name='contact'>
                        <all-attributes />
                        <link-entity name='adx_externalidentity' from='adx_contactid' to='contactid' alias='ab'>
                          <filter type='and'>
                            <condition attribute='adx_username' operator='eq' value='{username}' />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>";

            var contactResponse = service.RetrieveMultiple(new FetchExpression(fetchxml));

            return contactResponse.Entities.SingleOrDefault();
        }
    }
}
