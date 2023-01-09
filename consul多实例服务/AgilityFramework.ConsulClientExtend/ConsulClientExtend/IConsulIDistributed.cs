using NConsul.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgilityFramework.ConsulClientExtend.ConsulClientExtend
{
    public interface IConsulIDistributed:IDisposable
    {
        void Show();
        Task<IDistributedLock> AcquireLock(string key);
        Task ExecuteLocked(string key, Action action);
    }
}
