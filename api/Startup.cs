using api.Models;
using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using bp.ot.s.API.Models.TransEu;
using bp.ot.s.API.Services;
using bp.shared;
using bp.shared.Email;
using bp.shared.ErrorsHelper;
using bp.sharedLocal.Pdf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace api
{
    public class Startup
    {
        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BpKpirContextDane>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("Dane"));
            });

            services.AddCors(opt=> {
                opt.AddPolicy("allowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<BpKpirContextDane>()
                .AddDefaultTokenProviders();


            services.AddAuthentication(cfg=> {
                //cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // cfg.DefaultAuthenticateScheme= CookieAuthenticationDefaults.AuthenticationScheme;
                //cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddCookie(cfg => cfg.SlidingExpiration = true)
                .AddJwtBearer(opt =>
                {
                    opt.RequireHttpsMetadata = false;
                    opt.SaveToken = true;
                    opt.Audience = Configuration["Tokens:Audience"];
                    opt.ClaimsIssuer = Configuration["Tokens: Issuer"];
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["Tokens: Issuer"],
                        ValidAudience = Configuration["Tokens: Audience"],
                        RequireExpirationTime = false,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:key"]))
                    };
                });

            services
                .AddMvc(opt =>
                {
                    //    var AuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    //                opt.Filters.Add(new AuthorizeFilter("Authenticated"));
                    opt.Filters.Add(new CorsAuthorizationFilterFactory("allowAll"));
                })
            .AddJsonOptions((opt =>
             {
                 opt.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
             }));

            // Add application services.
            services.AddSingleton<List<TransEuAccessCredentialsDTO>>();
            services.AddSingleton<PdfRaports>();
            services.AddSingleton<CommonFunctions>();
            services.AddTransient<CompanyService>();
            services.AddTransient<InvoiceService>();
            services.AddTransient<OfferTransDbContextInitialDataIdent>();
            services.AddTransient<ContextErrorHelper>();
            services.Configure<bp.shared.Email.EmailConfig>(Configuration.GetSection("Email"));
            services.AddTransient<IEmailService, EmailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OfferTransDbContextInitialDataIdent dbIdentInit)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseDeveloperExceptionPage();
            //app.UseBrowserLink();
            //app.UseDatabaseErrorPage();


            //app.UseDeveloperExceptionPage();
            //app.UseBrowserLink();
            //app.UseDatabaseErrorPage();

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pl-PL"),
                RequestCultureProviders = null
                //SupportedCultures=new[] { new CultureInfo("pl-PL")},
                //SupportedUICultures  = new[] { new CultureInfo("pl-PL") }
            });

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            //dbIdentInit.Initialize().Wait();
        }
    }


    public class DesignTimeServicesDane : IDesignTimeDbContextFactory<BpKpirContextDane>
    {
        public BpKpirContextDane CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                //                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .Build();

            var builder = new DbContextOptionsBuilder<BpKpirContextDane>();
            var connectionString = configuration.GetConnectionString("Dane");
            builder.UseSqlServer(connectionString);
            return new BpKpirContextDane(builder.Options);
        }
    }
}
