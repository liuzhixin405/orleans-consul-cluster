using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eapi.Utility
{
    public static class ConsulHelper
    {

        public static async Task Regist(this IConfiguration configuration)
        {
            string ip = configuration["ip"];
            
            int port = int.Parse(configuration["port"]);
            int weight = string.IsNullOrWhiteSpace(configuration["weight"]) ? 1 : int.Parse(configuration["weight"]);
            using (ConsulClient client = new ConsulClient(c =>
            {
                c.Address = new Uri($"http://localhost:8500/");
                c.Datacenter = "dc1";

            }))
            {
                await client.Agent
                    .ServiceRegister(new AgentServiceRegistration()
                    {
                        ID = "Service" + "ip" + ":" + port,
                        Name = "test007_consul",
                        Address = ip,
                        Port = port,
                        Tags = new string[] { weight.ToString()},
                        Check = new AgentServiceCheck()
                        {
                            Interval = TimeSpan.FromSeconds(12),
                            HTTP = $"http://{ip}:{port}/Api/Health/Index",
                            Timeout = TimeSpan.FromSeconds(5),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5)
                        }

                    });

                Console.WriteLine($" http://{ip}:{port} --weight:{weight}完成注册");
            }
        }

    }
}
