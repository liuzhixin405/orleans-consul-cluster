using eapi.Models;
using eapi.Models.Dtos;
using eapi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace eapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;//500请求 并发50 . 100库存
        }
        //注意 网关服务请求地址 http://localhost:5000/consul/Order/Create?sku=10112354&count=1 ,多服务实例只能用分布式锁。
        [HttpPost]
        public async Task Create([FromServices]Channel<CreateOrderDto> channel,string sku, int count)
        {
           await channel.Writer.WriteAsync(new CreateOrderDto(sku,count));   //高并发高效解决方案  并发测试工具postjson_windows 10s
        }

        [HttpPost]
        public async Task CreateTestLock(string sku, int count)//非阻塞锁
        {
            await orderService.CreateTestLock(sku, count); //执行时间快,库存少量扣减 10s
        }
        [HttpPost]
        public async Task CreateBlockingLock(string sku, int count)//阻塞锁
        {
            await orderService.CreateBlockingLock(sku, count); //卖不完,时间长 50s
        }
        [HttpPost]
        public async Task CreateDistLock(string sku, int count) //colder组件 分布式锁
        {
            await orderService.CreateDistLock(sku, count); //库存扣完，时间长 50s
        }

        [HttpPost]
        public async Task CreateNetLock(string sku, int count)   //netlock.net锁 
        {
            await orderService.CreateNetLock(sku, count); //库存扣完，时间长 50s
        }

        static System.Threading.SpinLock semaphore = new SpinLock(false);
        [HttpPost]
        public async Task CreateLock(string sku, int count)   //卖不完
        {
            bool lockTaken = false;
            try
            {
                semaphore.Enter(ref lockTaken);
                await orderService.CreateLock(sku, count);
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
             orderService.CreateLocalLock(sku, count); //
        }

        [HttpPost]
        public async Task CreateNoLock(string sku, int count)
        {
           await orderService.CreateNoLock(sku, count); //乱的
        }
        [HttpGet]
        public async Task ChangeOrderStatus(int orderId, OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Shipment:
                    await orderService.Shipment(orderId);
                    break;
                case OrderStatus.Completed:
                    await orderService.Completed(orderId);
                    break;
                case OrderStatus.Rejected:
                    await orderService.Rejected(orderId);
                    break;
                default:
                    break;
            }
        }
    }
}
