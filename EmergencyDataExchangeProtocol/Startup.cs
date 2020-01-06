using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading.Tasks;
using EmergencyDataExchangeProtocol.Auth;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.helper;
using EmergencyDataExchangeProtocol.Websocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace EmergencyDataExchangeProtocol
{
    public class Startup
    {
        public static Dictionary<EmergencyObjectDataTypes, Type> registeredDataTypes = new Dictionary<EmergencyObjectDataTypes, Type>()
        {
            [EmergencyObjectDataTypes.Einsatz] = typeof(EmergencyObjects.einsatz.Einsatz),
            [EmergencyObjectDataTypes.Einsatzmittel] = typeof(EmergencyObjects.einsatzmittel.Einsatzmittel)
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGenericDataStore, GenericDataStore>();
            services.AddSingleton<IWebsocketManager, WebsocketManager>();
            

            services
                .AddAuthentication("ApiKey")
                .AddScheme<ApiKeyAuthenticationHandlerOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

            services.AddAuthorization(options =>
            {                
                options.AddPolicy("ExternalApi", policy =>
                    policy.AddAuthenticationSchemes("ApiKey").RequireAuthenticatedUser());
                options.DefaultPolicy = options.GetPolicy("ExternalApi");
            });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                }); ;

            services.AddSwaggerGen(c =>
            {

                c.DocumentFilter<EmergencyObject_DocumentFilter<object>>();

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });

                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Emergency Data API",
                    Description = "A rest and websocket protocol for exchanging Emergency Data between government agencies",
                    TermsOfService ="https://example.com/terms",
                    Contact = new Contact
                    {
                        Name = "Holger Martiker",                        
                        Email = "h.martiker@einsatzverwaltung.de",
                        Url = "https://www.einsatzverwaltung.de",
                    },
                    License = new License
                    {
                        Name = "Use under LICX",
                        Url = "https://example.com/license",
                    }
                });

                c.DescribeAllEnumsAsStrings();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IGenericDataStore dataStore)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Emergency Data API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/ws"))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        // Websocket-Request received
                        var websocketManager = context.RequestServices.GetService<IWebsocketManager>();
                        await websocketManager.OnWebsocketConnected(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });

            OnApplicationStarted(dataStore);
        }

        public void OnApplicationStarted(IGenericDataStore dataStore)
        {
            dataStore.InitIdentity();
        }
    }
}
