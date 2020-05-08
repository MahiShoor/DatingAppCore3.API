using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingAppCore3.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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

        //before adding mvcservices to middle ware add mvc service to IservicesCollection
        // This method gets called by the runtime. Use this method to add services to the container.
        //this method add all services required services to the  dependency injection container
        //In this method ordering is not so important
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
           
           services.AddDbContextPool<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddCors();
            services.AddScoped<IAuthRepository, AuthRepository>();
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

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
             
        }
    }
}
