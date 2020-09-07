using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Chatta.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chatta.Infrastructure.DataContext;
using Chatta.Web.Areas.Identity.Policies;
using Microsoft.Extensions.Caching.Distributed;
using Chatta.Core.Interfaces;
using Chatta.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Chatta.Infrastructure.Services;
using System.Collections.Generic;
using Chatta.Web.Authorization;

namespace Chatta.Web
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
            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("ChattaDbConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                // Add Roles
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();

            //--- Custom Config ---
            services.AddDbContextPool<ChattaDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("ChattaDbConnection")));

            // Add identity policies which includes password constraints 
            services.Configure<IdentityOptions>(options => IdentityPolicy.AddChattaIdentityPolicies(options));


            services.ConfigureApplicationCookie(options => {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ReturnUrlParameter = "RedirectUrl";
            });

            // Require all users to be authenticated. This is preferred to repeatedly relying on controllers to specify [Authorize] attribute
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            // Add Distributed Cache
            services.AddDistributedRedisCache(options => {
                options.Configuration = this.Configuration.GetConnectionString("redisServerUrl");
                var cacheOptions = new DistributedCacheEntryOptions();
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<IChattaRepository, ChattaRepository>();
            services.AddTransient<IUserManagerService, UserManagerService>();

            services.AddHttpContextAccessor();
            services.AddScoped<IAuthorizationHandler, PostIsOwnerAuthorizationHandler>();
            //--- Custom Config ---
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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

            // Security Headers
            app.Use(async (context, next) => {
                Dictionary<string, string> headersList = new Dictionary<string, string>();
                headersList.Add("X-Frame-Options", "DENY");
                headersList.Add("X-Xss-Protection", "1; mode=block");
                headersList.Add("X-Content-Type-Options", "nosniff");
                headersList.Add("Referrer-Policy", "same-origin");
                headersList.Add("X-Permitted-Cross-Domain-Policies", "none");
                headersList.Add("Feature-Policy", "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
                headersList.Add("Content-Security-Policy", "default-src 'self'");

                foreach (var header in headersList)
                {
                    if (!context.Response.Headers.ContainsKey(header.Key))
                    {
                        context.Response.Headers.Add(header.Key, header.Value);
                    }
                }

                // Remove these headers to provide server cloaking
                context.Response.Headers.Remove("x-powered-by");
                context.Response.Headers.Remove("server");
                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Posts}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }
    }
}
