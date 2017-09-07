using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using api.Models;
using api.Services;
using Microsoft.IdentityModel.Tokens;
using bp.ot.s.API.Entities.Context;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using bp.Pomocne.DTO;

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
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<OfferTransDbContextDane>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("Dane"));
            });

            services.AddDbContext<OfferTransDbContextIdent>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("Ident"));
            });

            services.AddOptions();
            services.Configure<ConfigurationDTO>(Configuration);
            services.AddCors(opt=> {
                opt.AddPolicy("allowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<OfferTransDbContextIdent>()
                .AddDefaultTokenProviders();
            services.Configure<bp.Pomocne.Email.EmailConfig>(Configuration.GetSection("Email"));

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddAuthentication()
                .AddJwtBearer(cfg=> {
                    cfg.RequireHttpsMetadata = true;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidIssuer = Configuration["Token:Issuer"],
                        ValidAudience = Configuration["Token:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Token:Key"]))
                    };
                });

            services.AddMvc(opt=> {
                opt.Filters.Add(new CorsAuthorizationFilterFactory("allowAll"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OfferTransDbContextIdent identContext)
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


            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();
            app.UseDatabaseErrorPage();

            app.UseStaticFiles();

            app.UseAuthentication();

           // OfferTransDbContextInitialDataIdent.Initialize(identContext);
                

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


        }

    }
}
