using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace angular
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //
            services.AddApplicationInsightsTelemetry(Configuration); 
            services.AddAuthorization(auth => 
            { 
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder() 
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) 
                    .RequireAuthenticatedUser().Build()); 
            }); 

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsRequestTelemetry(); 
 
            app.UseApplicationInsightsExceptionTelemetry(); 

            app.UseStaticFiles();

            #region Handle Exception 
            app.UseExceptionHandler(appBuilder => 
            { 
                appBuilder.Use(async (context, next) => 
                { 
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature; 
        
                    if (error != null && error.Error is SecurityTokenExpiredException) 
                    { 
                        context.Response.StatusCode = 401; 
                        context.Response.ContentType = "application/json"; 
        
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new RequestResult 
                        { 
                            State = RequestState.NotAuth, 
                            Msg = "token expired" 
                        })); 
                    } 
                    else if (error != null && error.Error != null) 
                    { 
                        context.Response.StatusCode = 500; 
                        context.Response.ContentType = "application/json"; 
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new RequestResult 
                        { 
                            State = RequestState.Failed, 
                            Msg = error.Error.Message 
                        })); 
                    } 
                    else await next(); 
                }); 
            }); 
            #endregion 

            #region UseJwtBearerAuthentication 
            app.UseJwtBearerAuthentication(new JwtBearerOptions() 
            {
                TokenValidationParameters = new TokenValidationParameters() 
                { 
                    IssuerSigningKey = TokenAuthOption.Key, 
                    ValidAudience = TokenAuthOption.Audience, 
                    ValidIssuer = TokenAuthOption.Issuer, 
                    ValidateIssuerSigningKey = true, 
                    ValidateLifetime = true, 
                    ClockSkew = TimeSpan.FromMinutes(0) 
                }
            });
            #endregion 

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
