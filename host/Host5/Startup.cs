using System.IdentityModel.Tokens.Jwt;
using Duende.Bff;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Host5
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBff()
                .AddCookieTicketStore();

            // local APIs
            services.AddControllers();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options => { options.Cookie.SameSite = SameSiteMode.Strict; })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5005";
                    options.ClientId = "spa";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("scope1");
                    options.Scope.Add("offline_access");
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // app.Use(async (ctx, next) =>
            // {
            //     await next();
            //
            //     if (ctx.Response.StatusCode == 302)
            //     {
            //         var isAjax = ctx.Request.Headers["X-Requested-With"];
            //     }
            // });

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // local APIs
                endpoints.MapControllers()
                    .RequireAuthorization();
                    //.AsBffApiEndpoint();

                // login, logout, user, backchannel logout...
                endpoints.MapBffManagementEndpoints();

                // proxy endpoint for cross-site APIs
                endpoints.MapBffApiEndpoint("/api", "https://localhost:5002")
                    .RequireAccessToken();
            });
        }
    }
}