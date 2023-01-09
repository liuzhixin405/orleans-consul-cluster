using NConsul;
using NConsul.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AgilityFramework.ConsulClientExtend.ConsulClientExtend
{
    public class ConsulIDistributed : IConsulIDistributed
    {
        private static string prefix = "consullock_";  // 同步锁参数前缀
        private ConsulClient consulClient;

        public ConsulIDistributed()
        {
            this.consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri($"http://127.0.0.1:{8500}/");
                c.Datacenter = "dc1";
            });
        }
        public Task<IDistributedLock> AcquireLock(string key)
        {
            LockOptions opts = new LockOptions($"{prefix}{key}");//默认值
            return this.consulClient.AcquireLock(opts);
        }

        public void Dispose()
        {
            if (this.consulClient != null)
            {
                this.consulClient.Dispose();
            }
        }

        public Task ExecuteLocked(string key, Action action)
        {
            LockOptions options = new LockOptions($"{prefix}{key}");
            return this.consulClient.ExecuteLocked(options,action);
        }

        public void Show()
        {
            using (ConsulClient client = new ConsulClient(c=> {
                c.Address = new Uri($"http://127.0.0.1：{8500}/");
                c.Datacenter = "dc1";
            }))
            {
                client.KV.Put(new KVPair("test") { Value = Encoding.UTF8.GetBytes("This is Teacher") });
                Console.WriteLine(client.KV.Get("test"));
                client.KV.Delete("test");
            }
        }
    }
}
 