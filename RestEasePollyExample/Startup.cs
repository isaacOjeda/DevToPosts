using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RestEase;

namespace RestEasePollyExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public const string GitHubClientName = "github";
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // HttpClient para GitHub
            services.AddHttpClient(GitHubClientName, options =>
            {
                options.BaseAddress = new Uri("https://api.github.com");
            })
            .AddTransientHttpErrorPolicy(builder =>
            {
                var retries = new List<TimeSpan>()
                {
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                };

                return builder.WaitAndRetryAsync(retries);
            });

            // Cliente para GitHub
            services.AddTransient<IGitHubApi>(provider =>
            {
                var factory = provider.GetService<IHttpClientFactory>();
                var httpClient = factory.CreateClient(GitHubClientName);

                var restClient = new RestClient(httpClient).For<IGitHubApi>();

                return restClient;
            });

            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
