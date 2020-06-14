using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingAppCore3.API.Data;
using DatingAppCore3.API.Helpers;
using DatingAppCore3.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
namespace DatingAppCore3.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContextPool<DataContext>(x => {
                x.UseLazyLoadingProxies();
            x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
         
            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            services.AddDbContextPool<DataContext>(x => {
                x.UseLazyLoadingProxies();
                x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            //services.AddDbContextPool<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            ConfigureServices(services);
        }


        //before adding mvcservices to middle ware add mvc service to IservicesCollection
        // This method gets called by the runtime. Use this method to add services to the container.
        //this method add all services required services to the  dependency injection container
        //In this method ordering is not so important
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityBuilder builder = services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);

            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration
                      .GetSection("Appsettings:Token").Value)),
                      ValidateIssuer = false,
                      ValidateAudience = false


                  };
              });
            services.AddAuthorization(options =>
           {
               options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
               options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin","Moderator"));
               options.AddPolicy("VipOnly", policy => policy.RequireRole("VIP"));
           });
            services.AddControllers( options=>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            }
            );

           
            services.AddCors();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddAutoMapper(typeof (DatingRepository).Assembly);
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
          //  services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<LogUserActivity>();
          

        }
        //this method configure middele ware as in sequence they are added
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //In this request pipelining method ordeValidateAudience = falser is extremely important
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler(builder =>
                //{
                //    builder.Run(async context =>
                //    {
                //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                //        var error = context.Features.Get<IExceptionHandlerFeature>();
                //        if (error != null)
                //        {
                //            context.Response.AdApplicationError(error.Error.Message);
                //            await context.Response.WriteAsync(error.Error.Message);
                //        }

                //    });
                //});
                app.UseHsts();
            }
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
               
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback"); 
            });
             
        }
    }
}
