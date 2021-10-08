using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAdvert.Web
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

            string poolId = Environment.GetEnvironmentVariable("WebAdvertCognitoPoolId");
            string clientId = Environment.GetEnvironmentVariable("WebAdvertCognitoClientId");
            string clientSecret = Environment.GetEnvironmentVariable("WebAdvertCognitoClientSecret");
            string accessKey = Environment.GetEnvironmentVariable("WebAdvertAwsAccessKey");
            string secretKey = Environment.GetEnvironmentVariable("WebAdvertAwsSecretKey");
            string region = "ap-southeast-1";
            AmazonCognitoIdentityProviderClient provider = new AmazonCognitoIdentityProviderClient(
                accessKey, secretKey, RegionEndpoint.GetBySystemName(region));
            CognitoUserPool userPool = new CognitoUserPool(poolId, clientId, provider, clientSecret);
            services.AddSingleton<IAmazonCognitoIdentityProvider>(provider);
            services.AddSingleton<CognitoUserPool>(userPool);
            services.AddCognitoIdentity();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });
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
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
