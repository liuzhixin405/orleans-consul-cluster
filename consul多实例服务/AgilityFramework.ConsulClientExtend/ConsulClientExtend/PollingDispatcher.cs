using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgilityFramework.ConsulClientExtend.ConsulClientExtend
{
    public class PollingDispatcher : IConsulDispatcher
    {
        private static int _iTotalCount = 0;
        private static int iTotalCount
        {
            get
            {
                return _iTotalCount;
            }
            set
            {
                _iTotalCount = value >= Int32.MaxValue ? 0 : value;
            }
        }
        public string ChooseAddress(string serviceName)
        {
            ConsulClient client = new ConsulClient(c=> {
                c.Address = new Uri("http://localhost:8500/");
                c.Datacenter = "dc1";
            });

            var response = client.Agent.Services().Result.Response;

            foreach (var item in response)
            {
                Console.WriteLine("***************************************");
                Console.WriteLine(item.Key);
                var service = item.Value;
                Console.WriteLine($"{service.Address}--{service.Port}--{service.Service}");
                Console.WriteLine("***************************************");
            }

            AgentService agentService = null;

            var serviceDictionary = response.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if(serviceDictionary.Length == 0)
            {
                throw new Exception("服务全都挂壁了,请检查哦!");
            }
            int index = iTotalCount++ % serviceDictionary.Length;
            agentService = serviceDictionary[index].Value;

            return $"{agentService.Address}:{agentService.Port}";

        }
    }
}
