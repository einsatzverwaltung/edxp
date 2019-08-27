using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Auth
{
    public class ApiKeyAuthenticationHandlerOptions : AuthenticationSchemeOptions { }

    public class ApiAuthenticationPostConfigureOptions : IPostConfigureOptions<ApiKeyAuthenticationHandlerOptions>
    {
        public void PostConfigure(string name, ApiKeyAuthenticationHandlerOptions options)
        {

        }
    }
}
