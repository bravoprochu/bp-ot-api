using api.Models;
using bp.ot.s.API.Entities.Context;
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
using System.Data.SqlClient;
using System.Text;


namespace api
{
    public class Startup
    {
        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //} 

        private readonly string CORS_POLICY_NAME = "allowAll";
        private readonly string CONNECTION_STRING_DATABASE_NAME = "Dane";
        private readonly string CONNECTION_STRING_CONFIGURATION_PASSWORD = "offerDbPassword";
        private readonly string CONNECTION_STRING_CONFIGURATION_USER_ID = "offerDbUserId";
        private readonly string CONFIGURATION_NAME_TOKEN_KEY = "offerTokenKey";
        private readonly string CONFIGURATION_NAME_TOKEN_ISSUER = "offerTokenIssuer";
        private readonly string CONFIGURATION_NAME_TOKEN_AUDIENCE = "offerTokenAudience";
        private string _connection = null;


        public IHostingEnvironment HostingEnvironment { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }


            Configuration = builder.Build();
            HostingEnvironment = env;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            if (HostingEnvironment.IsDevelopment())
            {
                var connString = new SqlConnectionStringBuilder(Configuration.GetConnectionString(this.CONNECTION_STRING_DATABASE_NAME));

                connString.UserID = Configuration[CONNECTION_STRING_CONFIGURATION_USER_ID];
                connString.Password = Configuration[CONNECTION_STRING_CONFIGURATION_PASSWORD];

                this._connection = connString.ConnectionString;
            }


            services.AddDbContext<BpKpirContextDane>(options =>
            {
                if (HostingEnvironment.IsDevelopment())
                {
                    options.UseSqlServer(this._connection);
                }
                else
                {
                    options.UseSqlServer(Configuration.GetConnectionString("Dane"));
                }

            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<BpKpirContextDane>()
                .AddDefaultTokenProviders();


            services.AddAuthentication(cfg =>
            {
                //cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //cfg.DefaultAuthenticateScheme= CookieAuthenticationDefaults.AuthenticationScheme;
                //cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddCookie(cfg => cfg.SlidingExpiration = true)
                .AddJwtBearer(opt =>
                {
                    opt.RequireHttpsMetadata = false;
                    opt.SaveToken = true;
                    opt.Audience = Configuration[CONFIGURATION_NAME_TOKEN_AUDIENCE];
                    opt.ClaimsIssuer = Configuration[CONFIGURATION_NAME_TOKEN_ISSUER];
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration[CONFIGURATION_NAME_TOKEN_ISSUER],
                        ValidAudience = Configuration[CONFIGURATION_NAME_TOKEN_AUDIENCE],
                        RequireExpirationTime = false,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration[CONFIGURATION_NAME_TOKEN_KEY]))

                    };
                });


            services.AddCors(opt =>
            {
                opt.AddPolicy(CORS_POLICY_NAME, builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                );
            });

            services
                .AddMvc(opt =>
                {
                    //    var AuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    //                opt.Filters.Add(new AuthorizeFilter("Authenticated"));
                    opt.Filters.Add(new CorsAuthorizationFilterFactory(CORS_POLICY_NAME));
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

            app.UseDeveloperExceptionPage();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("pl-PL"),
                RequestCultureProviders = null,
            });

            app.UseCors(CORS_POLICY_NAME);
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
