using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Auth
{
    class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationHandlerOptions>
    {
        ILogger log;
        IGenericDataStore _data;

        /// <summary>
        /// Handler für Authentifizierung über IP Adresse des Schalters
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="ctx"></param>
        /// <param name="ctxDOA"></param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationHandlerOptions> options,
                    ILoggerFactory logger,
                    UrlEncoder encoder,
                    ISystemClock clock,
                    IGenericDataStore data)
                    : base(options, logger, encoder, clock)
        {
            log = logger.CreateLogger<ApiKeyAuthenticationHandler>();
            _data = data;
        }



        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.HttpContext.Request.Headers["Authorization"].ToString();
            var authHeaderParts = authHeader.Split(' ');

            if (authHeaderParts.Length >= 2
                && authHeaderParts[0].ToLower() == "bearer")
            {
                var entity = _data.GetEndpointIdentityByApiKey(authHeaderParts[1]);

                if (entity != null)
                {
                    var identity = new GenericIdentity(entity.uid.ToString());
                    Request.HttpContext.Items.Add("Identity", entity);
                    Request.HttpContext.User = new ClaimsPrincipal(identity);
                    return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "ApiKey"));
                }

                return AuthenticateResult.Fail("Missing or malformed 'Authorization' header.");
            }

            return AuthenticateResult.NoResult();
        }
    }

    public class ApiEndpointIdentity : IIdentity
    {
        EndpointIdentity _e;

        public ApiEndpointIdentity(EndpointIdentity e)
        {
            _e = e;
        }

        public EndpointIdentity Identity => _e;

        public string AuthenticationType => "ApiKey";

        public bool IsAuthenticated => true;

        public string Name => _e.name;
    }
}
