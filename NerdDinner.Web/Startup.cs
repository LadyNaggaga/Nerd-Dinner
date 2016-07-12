using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NerdDinner.Web.Common;
using NerdDinner.Web.Models;
using NerdDinner.Web.Persistence;


namespace NerdDinner.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                //builder.AddApplicationInsightsSettings(developerMode: true);
                builder.AddUserSecrets();
               
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }
        //Use this method to add services to container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));


            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<NerdDinnerDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));
            

            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Cookies.ApplicationCookie.AccessDeniedPath = "/Home/AccessDenied";
            })
                    .AddEntityFrameworkStores<NerdDinnerDbContext>()
                    .AddDefaultTokenProviders();


            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://example.com");
                });
            });

            services.AddLogging();

            // Add MVC services to the services container
            services.AddMvc();

            // Add memory cache services
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // Add session related services.
            services.AddSession();

            // Add the system clock service
            services.AddSingleton<ISystemClock, SystemClock>();

            // Configure Auth
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ManageDinner",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("ManageDinner", "Allowed");
                    });
            });
        }      

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddConsole(minLevel: LogLevel.Warning);
            loggerFactory.AddDebug();
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            // Configure Session.
            app.UseSession();

            // Add static files to the request pipeline
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline
            app.UseIdentity();

            app.UseFacebookAuthentication(new FacebookOptions()
            {
                AppId = "609270052582677",
                AppSecret = "3d9a853452f18ca5e928e96602307525"
            });

            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = "296076155798-af8jfrst9kihv14ra8tf4la5ndtjtk5j.apps.googleusercontent.com ",
                ClientSecret = "tvisgn24YAafcqKEmsfEfrJQ"
            });

            //app.UseTwitterAuthentication(new TwitterOptions()
            //{
            //    ConsumerKey = "lDSPIu480ocnXYZ9DumGCDw37",
            //    ConsumerSecret = "fpo0oWRNc3vsZKlZSq1PyOSoeXlJd7NnG4Rfc94xbFXsdcc3nH"
            //});

           
            //app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
            //{
            //    DisplayName = "MicrosoftAccount - Requires project changes",
            //    ClientId = "000000004012C08A",
            //    ClientSecret = "GaMQ2hCnqAC6EcDLnXsAeBVIJOLmeutL"
            //});

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}"
                     //defaults: new { controller = "Home", action = "Index" }
                     );

            });

            //Populates the sample data 
            SampleData.InitializeNerdDinner(app.ApplicationServices).Wait();
        }
    }
}