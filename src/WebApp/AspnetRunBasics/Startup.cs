using AspnetRunBasics.ApiCollection;
using AspnetRunBasics.ApiCollection.Interfaces;
using AspnetRunBasics.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AspnetRunBasics
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
            #region Configuration Dependencies

            services.Configure<ApiSettings>(Configuration.GetSection(nameof(ApiSettings)));

            services.AddSingleton<IApiSettings>(sp => sp.GetRequiredService<IOptions<ApiSettings>>().Value);

            #endregion        

            #region Project Dependencies

            // add for httpClient factory
            services.AddHttpClient();

            // add api dependecy
            services.AddTransient<ICatalogApi, CatalogApi>();
            services.AddTransient<IBasketApi, BasketApi>();
            services.AddTransient<IOrderApi, OrderApi>();

            #endregion

            services.AddRazorPages();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
