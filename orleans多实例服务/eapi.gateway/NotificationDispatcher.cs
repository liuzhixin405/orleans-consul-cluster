using eapi.interfaces;
using eapi.interfaces.Models.Dtos;
using System.Threading.Channels;

namespace eapi
{
    internal class NotificationDispatcher : BackgroundService
    {
       
        private readonly ILogger<NotificationDispatcher> logger;
        private readonly Channel<CreateOrderDto> channel;
        private readonly IServiceProvider serviceProvider;
        public NotificationDispatcher( ILogger<NotificationDispatcher> logger, Channel<CreateOrderDto> channel, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.channel = channel;
            this.serviceProvider = serviceProvider;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!channel.Reader.Completion.IsCompleted)
            {
                var createOrderDto = await channel.Reader.ReadAsync();
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {

                        var client = scope.ServiceProvider.GetRequiredService<IClusterClient>();
                        var orderService = client.GetGrain<IOrderGrains>(Random.Shared.Next()); //设置为0导致指挥获取一个服务,随机的话就是多服务负载
                        //var orderService = client.GetGrain<IOrderGrains>(0);
                        await orderService.Create(createOrderDto.sku, createOrderDto.count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "notification failed");
                }

            }
        }
    }
}