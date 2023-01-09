using eapi.interfaces;
using eapi.interfaces.Models;
using eapi.interfaces.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace eapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IClusterClient orderService;
        public OrderController(IClusterClient orderService)
        {
            this.orderService = orderService;//500请求 并发50 . 100库存
        }

        [HttpPost]
        public async Task Create([FromServices] Channel<CreateOrderDto> channel, string sku, int count)
        {
            await channel.Writer.WriteAsync(new CreateOrderDto(sku, count));   //高并发高效解决方案  并发测试工具postjson_windows 10s
        }

        [HttpPost]
        public async Task CreateTestLock(string sku, int count)//非阻塞锁
        {
            await orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateTestLock(sku, count); //执行时间快,库存少量扣减 10s
        }
        [HttpPost]
        public async Task CreateBlockingLock(string sku, int count)//阻塞锁
        {
            await orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateBlockingLock(sku, count); //卖不完,时间长 50s
        }
        [HttpPost]
        public async Task CreateDistLock(string sku, int count) //colder组件 分布式锁
        {
            await orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateDistLock(sku, count); //库存扣完，时间长 50s
        }

        [HttpPost]
        public async Task CreateNetLock(string sku, int count)   //netlock.net锁 
        {
            await orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateNetLock(sku, count); //库存扣完，时间长 50s
        }

        static System.Threading.SpinLock semaphore = new SpinLock(false);
        [HttpPost]
        public async Task CreateLock(string sku, int count)   //卖不完
        {
            bool lockTaken = false;
            try
            {
                semaphore.Enter(ref lockTaken);
                await orderService.GetGrain<IOrderGrains>(0).CreateLock(sku, count);
            }
            finally
            {
                if (lockTaken)
                    semaphore.Exit();
            }
        }

        [HttpPost]
        public  void CreateLocalLock(string sku, int count)  //能卖完
        {
             orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateLocalLock(sku, count); //
        }

        [HttpPost]
        public async Task CreateNoLock(string sku, int count)
        {
           await orderService.GetGrain<IOrderGrains>(Random.Shared.Next()).CreateNoLock(sku, count); //乱的
        }
        [HttpGet]
        public async Task ChangeOrderStatus(int orderId, OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Shipment:
                    await orderService.GetGrain<IOrderGrains>(0).Shipment(orderId);
                    break;
                case OrderStatus.Completed:
                    await orderService.GetGrain<IOrderGrains>(0).Completed(orderId);
                    break;
                case OrderStatus.Rejected:
                    await orderService.GetGrain<IOrderGrains>(0).Rejected(orderId);
                    break;
                default:
                    break;
            }
        }
    }
}
