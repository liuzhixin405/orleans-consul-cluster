using Colder.DistributedLock.Abstractions;
using eapi.Models;
using eapi.RedisHelper;
using eapi.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Threading;
using Order = eapi.Models.Order;

namespace eapi.Service
{
    public class OrderService : IOrderService
    {
        private readonly IRepositoryWrapper repositoryWrapper;
        private readonly IDistributedLock _distributedLock;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IRepositoryWrapper repositoryWrapper, IDistributedLock distributedLock, ILogger<OrderService> logger)
        {
            this.repositoryWrapper = repositoryWrapper;
            _distributedLock = distributedLock;
            _logger = logger;
        }

        public async Task Completed(int orderId)
        {
            var order = await repositoryWrapper.OrderRepository.GetById(orderId);
            if (order == null)
            {
                throw new Exception("订单号错误");
            }
            if (order.Status != OrderStatus.Shipment)
            {
                throw new Exception("处置错误");
            }
            order.Status = OrderStatus.Completed;
            order.ShipMentTime = DateTime.Now;
            await repositoryWrapper.OrderRepository.Update(order);
        }

        public async Task Create(string sku, int count) //channel版本
        {
            try
            {
                var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                if (product == null || product.Count < count)
                {
                    _logger.LogInformation("库存不足,稍后重试");
                    return;
                }
                else
                {
                    product.Count -= count;
                }
                await repositoryWrapper.Trans(async () =>
                {
                    await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                    //throw new Exception("2"); //测试用
                    await repositoryWrapper.ProductRepository.Update(product);
                });
            }
            catch
            {
                throw;
            }
            finally
            {
            }
        }

        #region
        public async Task CreateTestLock(string sku, int count)  //自定义版本
        {

            var reKey = $"DataLock:{sku}_";
            using (var client = new ConnectionHelper().Conn())
            {
                bool isLocked = client.Add<string>(reKey, sku, TimeSpan.FromSeconds(100));//时间太小无效
                if (isLocked)
                {
                    try
                    {
                        var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                        if (product == null || product.Count < count)
                        {
                            _logger.LogInformation("库存不足,稍后重试");
                            return;
                        }
                        else
                        {
                            //getProductFromCache.Count -= count;
                            product.Count -= count;
                        }
                        await repositoryWrapper.Trans(async () =>
                        {
                            await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                            //throw new Exception("2"); //测试用                          
                            await repositoryWrapper.ProductRepository.Update(product);

                        });
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        client.Remove(reKey);
                    }
                }
                else
                {
                    Console.WriteLine($"失败: 没有拿到锁");
                }
            }
        }


        #endregion
        public async Task CreateDistLock(string sku, int count) //分布式锁版本
        {
            try
            {
                using var _ = await this._distributedLock.Lock(sku);
                var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                if (product == null || product.Count < count)
                {
                    _logger.LogInformation("库存不足,稍后重试");
                    return;
                }
                else
                {
                    product.Count -= count;
                }
                await repositoryWrapper.Trans(async () =>
                {
                    await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                    //throw new Exception("2"); //测试用
                    await repositoryWrapper.ProductRepository.Update(product);
                });
            }
            catch
            {
                throw;
            }
            finally
            {
            }
        }


        static readonly string resource = "CreateNetLock_Lock_";
        static readonly TimeSpan expiry = TimeSpan.FromSeconds(30);
        static readonly TimeSpan wait = TimeSpan.FromSeconds(10);
        static readonly TimeSpan retry = TimeSpan.FromSeconds(1);
        public async Task CreateNetLock(string sku, int count)      //分布式锁版本二
        {
            var redlockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
                                                                                    {
                                                                                        ConnectionMultiplexer.Connect("localhost:6379"),
                                                                                    });
            try
            {
                // blocks until acquired or 'wait' timeout
                await using (var redLock = await redlockFactory.CreateLockAsync(resource, expiry, wait, retry)) // there are also non async Create() methods
                {
                    // make sure we got the lock
                    if (redLock.IsAcquired)
                    {
                        // do stuff
                        var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                        if (product == null || product.Count < count)
                        {
                            _logger.LogInformation("库存不足,稍后重试");
                            return;
                        }
                        else
                        {
                            product.Count -= count;
                        }
                        await repositoryWrapper.Trans(async () =>
                        {
                            await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                            //throw new Exception("2"); //测试用
                            await repositoryWrapper.ProductRepository.Update(product);
                        });
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                redlockFactory.Dispose();
            }
        }



        public async Task CreateLock(string sku, int count)
        {

            try
            {
                var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                if (product == null || product.Count < count)
                {
                    _logger.LogInformation("库存不足,稍后重试");
                    return;
                }
                else
                {
                    product.Count -= count;
                }
                await repositoryWrapper.Trans(async () =>
                {
                    await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                    //throw new Exception("2"); //测试用
                    await repositoryWrapper.ProductRepository.Update(product);
                });
            }
            catch
            {
                throw;
            }
            finally
            {
            }
        }

        static readonly object local_lock = new object();
        public void CreateLocalLock(string sku, int count) //本地锁版本
        {

            lock (local_lock)
            {
                var product = (repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).ConfigureAwait(false).GetAwaiter().GetResult().SingleOrDefault();

                if (product == null || product.Count < count)
                {
                    _logger.LogInformation("库存不足,稍后重试");
                    return;
                }
                else
                {
                    product.Count -= count;
                }
                repositoryWrapper.Trans(() =>
               {
                   repositoryWrapper.OrderRepository.Create(Order.Create(sku, count)).ConfigureAwait(false).GetAwaiter().GetResult();
                   //throw new Exception("2"); //测试用
                   repositoryWrapper.ProductRepository.Update(product).ConfigureAwait(false).GetAwaiter().GetResult();
                   return Task.CompletedTask;
               }).ConfigureAwait(false).GetAwaiter().GetResult();
            }

        }
        public async Task Rejected(int orderId)
        {
            var order = await repositoryWrapper.OrderRepository.GetById(orderId);
            if (order == null)
            {
                throw new Exception("订单号错误");
            }
            if (order.Status == OrderStatus.Completed)
            {
                throw new Exception("已完成无法拒收");
            }

            order.Status = OrderStatus.Rejected;
            order.RejectedTime = DateTime.Now;

            var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(order.Sku))).SingleOrDefault();
            if (product == null)
            {
                throw new Exception("product号错误");
            }
            product.Count += order.Count;

            await repositoryWrapper.Trans(async () =>
            {
                await repositoryWrapper.OrderRepository.Update(order);
                await repositoryWrapper.ProductRepository.Update(product);
            });

        }

        public async Task Shipment(int orderId)
        {
            var order = await repositoryWrapper.OrderRepository.GetById(orderId);
            if (order == null)
            {
                throw new Exception("订单号错误");
            }
            if (order.Status != OrderStatus.Created)
            {
                throw new Exception("处置错误");
            }
            order.Status = OrderStatus.Shipment;
            order.ShipMentTime = DateTime.Now;
            await repositoryWrapper.OrderRepository.Update(order);
        }

        public async Task CreateNoLock(string sku, int count)
        {
            try
            {
                var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                if (product == null || product.Count < count)
                {
                    _logger.LogInformation("库存不足,稍后重试");
                    return;
                }
                else
                {
                    product.Count -= count;
                }
                await repositoryWrapper.Trans(async () =>
                {
                    await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                    //throw new Exception("2"); //测试用
                    await repositoryWrapper.ProductRepository.Update(product);
                });
            }
            catch
            {
                throw;
            }
            finally
            {
            }
        }

        public async Task CreateBlockingLock(string sku, int count)
        {
            var reKey = $"Block_DataLock:{sku}_";
            using (var client = new ConnectionHelper().Conn())
            {
                using (var dataLock = client.AcquireLock(reKey, TimeSpan.FromSeconds(100)))
                {
                    try
                    {
                        var product = (await repositoryWrapper.ProductRepository.FindByCondition(x => x.Sku.Equals(sku))).SingleOrDefault();

                        if (product == null || product.Count < count)
                        {
                            _logger.LogInformation("库存不足,稍后重试");
                            return;
                        }
                        else
                        {
                            //getProductFromCache.Count -= count;
                            product.Count -= count;
                        }
                        await repositoryWrapper.Trans(async () =>
                        {
                            await repositoryWrapper.OrderRepository.Create(Order.Create(sku, count));
                            //throw new Exception("2"); //测试用                          
                            await repositoryWrapper.ProductRepository.Update(product);

                        });
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                    }
                }
            }
        }
    }
}
