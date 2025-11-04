using Microsoft.EntityFrameworkCore;
using Ciclilavarizia.BLogic;
using Ciclilavarizia.Data;
using Ciclilavarizia.Models.ServicesExtentions;


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

            //db connection
            builder.Services.AddDbContext<AdventureWorksLTContext>(o =>
                o.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksLTDbHomelab")));

            var AnyOrigin = "_anyOrigin";
            var LiveServerOrigin = "_liveServerOrigin";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: LiveServerOrigin,
                                  policy =>
                                  {
                                      policy.WithOrigins("http://127.0.0.1:5500", // this was the live server 
                                          "https://localhost:4200") // this is the SPA made with angular
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                                  });
            });

            builder.Services.AddCAndPStore();
            builder.Services.AddDbSecure(
                builder.Configuration.GetConnectionString("AdventureWorksSecureDbHomelab"));



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
