
using AgilityFramework.ConsulClientExtend.ConsulClientExtend;
using Colder.DistributedLock.Hosting;
using eapi.Data;
using eapi.Models.Dtos;
using eapi.Repositories;
using eapi.Service;
using eapi.Utility;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceStack;
using System.Reflection;
using System.Threading.Channels;

namespace eapi
{
    public class Program
    {
        //private static readonly string connectionString = "Data Source=PC-202205262203;Initial Catalog=productdb;Persist Security Info=False;User ID=sa;Password=1230;MultipleActiveResultSets=true;TrustServerCertificate=true";
        private static readonly string connectionString = "Data Source=IOS;Initial Catalog=productdb;Persist Security Info=False;User ID=sa;Password=1230;MultipleActiveResultSets=true;TrustServerCertificate=true";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Host.ConfigureDistributedLockDefaults();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.SetupDatabase();
            builder.Services.AddHostedService<NotificationDispatcher>();
            builder.Services.AddSingleton(Channel.CreateUnbounded<CreateOrderDto>());
            builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<IConsulIDistributed, ConsulIDistributed>();
          
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            ConsulHelper.Regist(builder.Configuration).GetAwaiter().GetResult();

            app.MapControllers();

            app.Run();
        }
    }
}