using Ciclilavarizia.BLogic;
using Ciclilavarizia.Data;
using Ciclilavarizia.Models.Settings;
using DataAccessLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Ciclilavarizia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // bind IOptions
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(nameof(JwtSettings)));

            //db connection
            builder.Services.AddDbContext<AdventureWorksLTContext>(o =>
                o.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksLTDbHomelab")));

            builder.Services.AddDbSecure(
                builder.Configuration.GetConnectionString("AdventureWorksSecureDbHomelab"));

            // CORS
            var AnyOrigin = "_anyOrigin";
            var LiveServerOrigin = "_liveServerOrigin";
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(name: LiveServerOrigin,
                                      policy =>
                                      {
                                          policy.AllowAnyOrigin()
                                          //policy.WithOrigins(//"http://127.0.0.1:5500", // this was the live server 
                                          //                   "http://localhost:4200/") // this is the SPA made with angular
                                                .AllowAnyHeader()
                                                .AllowAnyMethod();
                                      });
                });
            }

            //
            // Start Custom Services Extentions
            //

            // Service for exercises
            builder.Services.AddCAndPStore();

            builder.Services.AddAuthenticationOptions();
            builder.Services.AddAuthorizationOptions();

            //
            // End Custom Services Extentions
            //

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            if (builder.Environment.IsDevelopment())
            {
                app.UseCors(LiveServerOrigin);
            }



            app.MapControllers();

            app.Run();
        }
    }
}
