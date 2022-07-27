using AuthFirst.Data;
using AuthFirst.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthFirst
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<AuthDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<UserAccess>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/denied";
                options.Events = new CookieAuthenticationEvents() { 
                     //OnValidatePrincipal = async context =>
                     //{
                     //    if(context.Principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                     //    {
                     //        if(context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "kayode")
                     //        {
                     //            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                     //            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                     //        }
                     //    }
                     //    await Task.CompletedTask;
                     //},

                    OnSigningIn = async context =>
                    {
                        var scheme = context.Properties.Items.Where(k => k.Key == ".AuthScheme").FirstOrDefault();
                        var claims = new Claim(scheme.Key, scheme.Value);
                        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                        var userService = context.HttpContext.RequestServices.GetService(typeof(UserAccess)) as UserAccess;
                        var nameIdentifier = claimsIdentity.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier)?.Value;
                       if (nameIdentifier != null && userService != null)
                        {
                            var appUser = userService.GetUserByExternalProvider(scheme.Value, nameIdentifier);
                            if(appUser == null)
                            {
                                appUser = userService.AddUser(claimsIdentity.Claims.ToList(), scheme.Value);
                            }

                            foreach (var r in appUser.RoleList)
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, r));
                            }
                        }
                        claimsIdentity.AddClaim(claims);
                        await Task.CompletedTask;
                    }
                };
            })
            .AddOpenIdConnect("google", options =>
            {
                options.Authority = Configuration["GoogleOpenId:Authority"];
                options.ClientId = Configuration["GoogleOpenId:ClientId"];
                options.ClientSecret = Configuration["GoogleOpenId:ClientSecret"];
                options.CallbackPath = Configuration["GoogleOpenId:CallbackPath"];
                options.SaveTokens = true;
                //options.Events = new OpenIdConnectEvents()
                //{
                //     OnTokenValidated = async context =>
                //     {
                //         if(context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value == "109294569079941472291")
                //         {
                //             var claims = new Claim(ClaimTypes.Role, "Admin");
                //             var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                //             claimsIdentity.AddClaim(claims);
                //         }
                //     }
                //};
            })
            .AddOpenIdConnect("okta", options =>
            {
                options.Authority = Configuration["OktaOpenId:Authority"];
                options.ClientId = Configuration["OktaOpenId:ClientId"];
                options.ClientSecret = Configuration["OktaOpenId:ClientSecret"];
                options.CallbackPath = Configuration["OktaOpenId:CallbackPath"];
                options.SignedOutCallbackPath = Configuration["OktaOpenId:SignedOutCallbackPath"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
            });
            //.AddGoogle(options =>
            //{
            //    options.ClientId = "142605705118-og31j8odg74ab1jmdo4rvv5dmjuofudm.apps.googleusercontent.com";
            //    options.ClientSecret = "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";
            //    options.CallbackPath = "/auth";
            //    options.AuthorizationEndpoint += "?prompt=consent";
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
