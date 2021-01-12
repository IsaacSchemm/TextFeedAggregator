using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TextFeedAggregator.Data;

namespace TextFeedAggregator {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    o => o.EnableRetryOnFailure()));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddAuthentication()
                .AddDeviantArt(d => {
                    d.Scope.Add("browse");
                    d.Scope.Add("message");
                    d.Scope.Add("user.manage");
                    d.ClientId = Configuration["Authentication:DeviantArt:ClientId"];
                    d.ClientSecret = Configuration["Authentication:DeviantArt:ClientSecret"];
                    d.SaveTokens = true;
                })
                .AddMastodon("mastodon.social", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("write:statuses");
                    o.Scope.Add("read:accounts");
                    o.Scope.Add("read:notifications");
                    o.ClientId = Configuration["Authentication:Mastodon:mastodon.social:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:mastodon.social:client_secret"];
                    o.SaveTokens = true;
                })
                .AddMastodon("mastodon.technology", o => {
                    o.Scope.Add("read:statuses");
                    o.Scope.Add("write:statuses");
                    o.Scope.Add("read:accounts");
                    o.Scope.Add("read:notifications");
                    o.ClientId = Configuration["Authentication:Mastodon:mastodon.technology:client_id"];
                    o.ClientSecret = Configuration["Authentication:Mastodon:mastodon.technology:client_secret"];
                    o.SaveTokens = true;
                });

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
