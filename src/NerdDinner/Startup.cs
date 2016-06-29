using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NerdDinner.Web.Common;
using NerdDinner.Web.Models;
using NerdDinner.Web.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NerdDinner.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            if (env.IsEnvironment("Development"))
            {
                configuration.AddUserSecrets();
            }

            configuration.AddEnvironmentVariables();
            Configuration = configuration.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Add framework services.

            services.AddDbContext<NerdDinnerDbContext>(options =>
                options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddScoped<INerdDinnerRepository, NerdDinnerRepository>();

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<NerdDinnerDbContext>()
                .AddDefaultTokenProviders();

            // Add MVC services to the services container.
            services.AddMvcCore (options =>
            {
                var settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var formatter = new JsonOutputFormatter { SerializerSettings = settings };
                options.OutputFormatters.Insert(0, formatter);

                // Add validation filters
                options.Filters.Add(new ValidateModelFilterAttribute());
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.

            // Add the console logger.
            loggerfactory.AddConsole(minLevel: LogLevel.Warning);

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
               
            }

            app.UseStaticFiles();
            
            app.UseIdentity();
            ////app.UseFacebookAuthentication(new FacebookOptions {
            ////     AppId = Configuration["Authentication:Facebook:AppId"],
            ////     AppSecret = Configuration["Authentication:Facebook:AppSecret"]
            ////      }              
            ////    );
            ////app.UseGoogleAuthentication( new GoogleOptions {
            ////    ClientId = Configuration["Authentication:Google:AppId"],
            ////    ClientSecret =Configuration["Authentication:Google:AppSecret"]

            ////});
            //////app.UseMicrosoftAccountAuthentication(); Add later 
            ////app.UseTwitterAuthentication(new TwitterOptions
            ////{
            ////    ConsumerKey = Configuration["Authentication:Twitter:AppId"],
            ////    ConsumerSecret = Configuration["Authentication: Twitter:AppSecret"]
            ////});

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });

            //SampleData.InitializeNerdDinner(app.ApplicationServices).Wait();
        }
    }
}
