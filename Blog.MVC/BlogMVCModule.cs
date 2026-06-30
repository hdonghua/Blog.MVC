using BeaverX.Core.Modules;
using BeaverX.EntityFrameworkCore.DependencyInjection;
using BeaverX.EntityFrameworkCore.MySql;
using BeaverX.WebMvc;
using Blog.MVC.Data;
using Blog.MVC.IServices.Storage;
using Blog.MVC.Services.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;

namespace Blog.MVC
{
    [DependsOn(
        typeof(BeaverXEntityFrameworkCoreMySqlModule),
        typeof(BeaverXWebMvcModule)
        )]
    public class BlogMVCModule : BeaverXModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            var configuration = context.Configuration;

            services.AddControllersWithViews();

            var connectionString = configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddBeaverXDbContext<BlogDbContext>(connectionString);
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            services.AddAuthorization();

            ConfigureMinio(services, configuration);

            services.Replace(ServiceDescriptor.Singleton<IDbDriverOptionsBuilder, BlogDbDriverOptionsBuilder>());
        }

        private static void ConfigureMinio(IServiceCollection services, IConfiguration configuration)
        {
            var minioOptions = configuration
                .GetSection(MinioOptions.SectionName)
                .Get<MinioOptions>();

            if (minioOptions == null ||
                string.IsNullOrWhiteSpace(minioOptions.Endpoint) ||
                string.IsNullOrWhiteSpace(minioOptions.AccessKey) ||
                string.IsNullOrWhiteSpace(minioOptions.SecretKey))
            {
                return;
            }

            var (endpoint, useSsl) = MinioEndpointHelper.Parse(minioOptions.Endpoint, minioOptions.UseSsl);
            services.AddMinio(client => client
                .WithEndpoint(endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
                .WithSSL(useSsl)
                .Build());
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = (WebApplication)context.App;

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllers();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            SeedDatabase(app);
        }

        private static void SeedDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetService<BlogDbSeeder>();
            if (seeder == null)
            {
                return;
            }

            seeder.SeedAsync().GetAwaiter().GetResult();
        }
    }
}
