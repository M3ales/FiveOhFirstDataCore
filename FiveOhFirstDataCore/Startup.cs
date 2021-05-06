using FiveOhFirstDataCore.Areas.Identity;
using FiveOhFirstDataCore.Core.Account;
using FiveOhFirstDataCore.Core.Database;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using AspNet.Security.OpenId;
using FiveOhFirstDataCore.Core.Services;
using System.Web;

namespace FiveOhFirstDataCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var Secrets = new ConfigurationBuilder()
                .AddJsonFile(Path.Join("Config", "website_config.json"))
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Secrets.GetConnectionString("database")));
            services.AddIdentity<Trooper, TrooperRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddRoleManager<TrooperRoleManager>()
                .AddSignInManager<TrooperSignInManager>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddAuthentication()
                .AddSteam("Steam", "Steam", options =>
                {
                    options.Events = new OpenIdAuthenticationEvents
                    {
                        OnTicketReceived = async context =>
                        {
                            if (context.Request.Cookies.TryGetValue("token", out var token))
                            {
                                if (context.Request.QueryString.HasValue)
                                {
                                    var query = context.Request.Query["openid.identity"];

                                    var link = context.HttpContext.RequestServices.GetRequiredService<AccountLinkService>();

                                    await link.BindSteamUserAsync(token, query);
                                }
                            }
                        }
                    };
                })
                .AddOAuth("Discord", "Discord", options =>
                {
                    options.AuthorizationEndpoint = $"{Configuration["Config:Discord:Api"]}/oauth2/authorize";

                    options.CallbackPath = new PathString("/authorization-code/discord-callback"); // local auth endpoint
                    options.AccessDeniedPath = new PathString("/api/link/denied");

                    options.ClientId = Configuration["Config:Discord:ClientId"];
                    options.ClientSecret = Secrets["Discord:ClientSecret"];
                    options.TokenEndpoint = $"{Configuration["Config:Discord:TokenEndpoint"]}";

                    options.Scope.Add("identify");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            // get user data
                            var client = new DiscordRestClient(new()
                            {
                                Token = context.AccessToken,
                                TokenType = TokenType.Bearer
                            });

                            var user = await client.GetCurrentUserAsync();

                            if (context.Request.Cookies.TryGetValue("token", out var token))
                            {
                                var link = context.HttpContext.RequestServices.GetRequiredService<AccountLinkService>();

                                try
                                { // verify the user information grabbed matches the user info
                                  // saved from the inital command
                                    await link.BindDiscordAsync(token, user.Id);
                                }
                                catch (Exception ex)
                                {
                                    context.Response.Cookies.Append("error", ex.Message);
                                }

                                context.Success();
                            }
                            else
                            {
                                context.Response.Cookies.Append("error", "No token found.");
                            }
                        },
                        OnRemoteFailure = async context =>
                        {
                            // TODO remove token from cookies and delete server token cache.


                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireTrooper", policy =>
                {
                    policy.RequireRole("Admin", "Trooper");
                });

                options.AddPolicy("RequireLinked", policy =>
                {
                    policy.RequireRole("Linked", "Admin");
                });
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 2;
                options.Password.RequiredUniqueChars = 0;
            });

            services.AddSingleton<AccountLinkService>();

#if DEBUG
            #region Example Tools
            services.AddScoped<TestDataService>();
            #endregion
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ApplyDatabaseMigrations(db);

            var roleManager = scope.ServiceProvider.GetRequiredService<TrooperRoleManager>();
            roleManager.SeedRoles<WebsiteRoles>().GetAwaiter().GetResult();

#if DEBUG
            var data = scope.ServiceProvider.GetRequiredService<TestDataService>();
            data.InitalizeAsync().GetAwaiter().GetResult();
#endif

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private static void ApplyDatabaseMigrations(DbContext database)
        {
            if (!(database.Database.GetPendingMigrations()).Any())
            {
                return;
            }

            database.Database.Migrate();
            database.SaveChanges();
        }
    }
}