using Infrastructure.SqlServer.Repositories;
using Infrastructure.SqlServer.Repositories.SqlServer.DataContext;
using Infrastructure.SqlServer.Repositories.SqlServer.MapperProfile;
using Infrastructure.SqlServer.UnitOfWork;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using UseCases;
using UseCases.Repositories;
using UseCases.UnitOfWork;
using Infrastructure.Services;
using VNPay;
using Microsoft.AspNetCore.Authentication.Cookies;
using NuGet.Packaging.Signing;

namespace Infrastructure
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var clientId = builder.Configuration["GoogleKeys:ClientId"];
            var clientSecret = builder.Configuration["GoogleKeys:ClientSecret"];

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Authentication/Login";
                options.AccessDeniedPath = "/Authentication/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
            .AddGoogle(options =>
            {
                options.ClientId = clientId ?? throw new Exception("Google CLientId is missing in appSetting.json");
                options.ClientSecret = clientSecret ?? throw new Exception("Google CLientSecret is missing in appSetting.json");
            });

            RegisterInfrastructureServices(builder.Configuration, builder.Services);

            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AddAreaPageRoute("Admin", "/Admin/Books/Index", "Admin/Books/Index");
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages();
            app.Run();
        }

        private static void RegisterInfrastructureServices(ConfigurationManager configuration, IServiceCollection services)
        {
            var repositoryOptions = configuration.GetSection("Repository").Get<RepositoryOptions>() ?? throw new Exception("No RepositoryOptions found.");
            if (repositoryOptions.RepositoryType == RepositoryTypes.SqlServer)
            {
                services.AddAutoMapper(typeof(SqlServer2EntityProfile));

                services.AddDbContext<BookShopDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("IBookDb")).UseLazyLoadingProxies());

                services.AddTransient<IBookRepository, SqlServerBookRepository>();
                services.AddTransient<ICartItemRepository, SqlServerCartItemRepository>();
                services.AddTransient<ICategoryRepository, SqlServerCategoryRepository>();
                services.AddTransient<IFeedbackRepository, SqlServerFeedbackRepository>();
                services.AddTransient<IOrderItemRepository, SqlServerOrderItemRepository>();
                services.AddTransient<IOrderRepository, SqlServerOrderRepository>();
                services.AddTransient<IPublisherRepository, SqlServerPublisherRepository>();
                services.AddTransient<IUserRepository, SqlServerUserRepository>();
                services.AddTransient<IPaymentTransactionRepository, SqlServerPaymentTransactionRepository>();

                services.AddTransient<ICategoryUnitOfWork, SqlServerCategoryUnitOfWork>();
                services.AddTransient<IOrderUnitOfWork, SqlServerOrderUnitOfWork>();
                services.AddTransient<IPublisherUnitOfWork, SqlServerPublisherUnitOfWork>();
                services.AddTransient<IPaymentTransactionUnitOfWork, SqlServerPaymentTransactionUnitOfWork>();
            }
            else
            {
                throw new Exception("Cannot register infrastructure services.");
            }

            services.AddTransient<AuthenticationManager>();
            services.AddTransient<BookManager>();
            services.AddTransient<CartItemManager>();
            services.AddTransient<CategoryManager>();
            services.AddTransient<FeedbackManager>();
            services.AddTransient<OrderManager>();
            services.AddTransient<PublisherManager>();
            services.AddTransient<UserManager>();
            services.AddTransient<PaymentTransactionManager>();

            services.AddTransient<BookProcessingService>();
            services.AddTransient<BookMappingService>();
            services.AddTransient<ImageService>();
            services.AddTransient<FeedbackMappingService>();
            services.AddTransient<OrderMappingService>();

            services.AddTransient<IVNPay, VNPayImpl>();
        }
    }
}
