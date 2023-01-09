
using eapi.interfaces.Models.Dtos;
using System.Threading.Channels;

namespace eapi.gateway
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
            builder.Services.AddHostedService<NotificationDispatcher>();
            builder.Services.AddSingleton(Channel.CreateUnbounded<CreateOrderDto>());
            builder.Host.UseOrleansClient(c => {
                c.UseLocalhostClustering(new int[] { 30001,30002,30003 });
                //c.UseLocalhostClustering(new int[] { 30001 });//µ¥»ú²âÊÔ
            });
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