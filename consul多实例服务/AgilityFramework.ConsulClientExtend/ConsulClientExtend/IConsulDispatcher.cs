using System;
using System.Collections.Generic;
using System.Text;

namespace AgilityFramework.ConsulClientExtend.ConsulClientExtend
{
    public interface IConsulDispatcher
    {
        /// <summary>
        /// 负载均衡
        /// </summary>
        /// <param name="servviceName"></param>
        /// <returns></returns>
        string ChooseAddress(string serviceName);
    }
}
