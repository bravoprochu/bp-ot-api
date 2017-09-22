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
using Microsoft.IdentityModel.Tokens;
using bp.ot.s.API.Entities.Context;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using bp.Pomocne.DTO;
using bp.ot.s.API.Services;
using bp.Pomocne.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using bp.Pomocne.IdentityHelp.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
            services.AddDbContext<OfferTransDbContextDane>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("Dane"));
            });

            services.AddDbContext<OfferTransDbContextIdent>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("Ident"));
            });

            services.AddCors(opt=> {
                opt.AddPolicy("allowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<OfferTransDbContextIdent>()
                .AddDefaultTokenProviders();
                


            // Add application services.
            services.Configure<bp.Pomocne.Email.EmailConfig>(Configuration.GetSection("Email"));
            services.AddTransient<IEmailService, EmailService>();

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Audience"],
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };
                });
            

            services.AddMvc(opt=> {
                opt.Filters.Add(new CorsAuthorizationFilterFactory("allowAll"));
            });

            services.AddScoped<IDbInitializer, OfferTransDbContextInitialDataIdent>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OfferTransDbContextIdent identContext, RoleManager<IdentityRole> roleManager)
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

            new OfferTransDbContextInitialDataIdent(identContext).Initialize();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            


        }

    }
}
