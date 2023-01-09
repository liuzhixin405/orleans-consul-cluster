
using Colder.DistributedLock.Hosting;
using eapi.Data;
using eapi.interfaces.Models;
using eapi.interfaces.Models.Dtos;
using eapi.Repositories;
using eapi.Service;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceStack.DataAnnotations;
using StackExchange.Redis;
using System.Reflection;
using System.Threading.Channels;

namespace eapi
{
    public class Program
    {
        private static readonly string connectionString = "Data Source=PC-202205262203;Initial Catalog=productdb;Persist Security Info=False;User ID=sa;Password=1230;MultipleActiveResultSets=true;TrustServerCertificate=true";
        //private static readonly string connectionString = "Data Source=IOS;Initial Catalog=productdb;Persist Security Info=False;User ID=sa;Password=1230;MultipleActiveResultSets=true;TrustServerCertificate=true";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Host.ConfigureDistributedLockDefaults();
            var gport = int.Parse(builder.Configuration["OrleansOptions:GatewayPort"]);
            var sport = int.Parse(builder.Configuration["OrleansOptions:SiloPort"]);
            builder.Host.UseOrleans(b => b.UseLocalhostClustering(sport, gport));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddDbContext<ProductDbContext>(options=>options.UseSqlServer(connectionString));
            builder.Services.AddScoped<Func<ProductDbContext>>(provider =>()=> provider.GetService<ProductDbContext>());
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.SetupDatabase();
            builder.Services.AddScoped<DbFactory>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IRepository<eapi.interfaces.Models.Order>,OrderRepository>();
            builder.Services.AddScoped<IRepository<eapi.interfaces.Models.Product>, ProductRepository>();
            builder.Services.AddScoped<OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(x =>
            {
                x.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .DisallowCredentials();
            });
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}