using eapi.Models.Dtos;
using eapi.Service;
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

                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
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