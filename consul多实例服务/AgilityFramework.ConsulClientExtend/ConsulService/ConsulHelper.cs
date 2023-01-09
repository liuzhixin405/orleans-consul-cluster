using Consul;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AgilityFramework.ConsulClientExtend.ConsulService
{
    public static class ConsulHelper
    {
        private static async Task UseConsul(this IApplicationBuilder app, ConsulConfigModel consulService,HealthConfigModel healthService)
        {
            string ip = healthService.IP;
            int port = healthService.Port;

            using(ConsulClient client = new ConsulClient(c=>
            {
                c.Address = new Uri($"http://{consulService.IP}:{consulService.Port}/");
                c.Datacenter = "dc1";
               
            }))
            {
                await client.Agent
                    .ServiceRegister(new AgentServiceRegistration()
                    {
                        ID = "grpcService" + "ip" + ":" + port,
                        Name = healthService.GroupName,
                        Address = ip,
                        Port = port,
                        Tags = healthService.Tag,
                        Check = new AgentServiceCheck()
                        {
                            Interval = TimeSpan.FromSeconds(12),
                            HTTP=$"http://{ip}:{healthService.CheckPort}/Health",
                            Timeout = TimeSpan.FromSeconds(5),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5)
                        }

                    });

                Console.WriteLine($" http://{ip}:{port} 完成注册");
            }
        }
    }
}
