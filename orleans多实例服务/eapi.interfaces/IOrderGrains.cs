using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eapi.interfaces
{
    public interface IOrderGrains:Orleans.IGrainWithIntegerKey
    {
        Task Create(string sku, int count);
        Task Shipment(int orderId);
        Task Rejected(int orderId);
        Task Completed(int orderId);
        Task CreateTestLock(string sku, int count);
        Task CreateDistLock(string sku, int count);
        Task CreateNetLock(string sku, int count);
        Task CreateLock(string sku, int count);
        void CreateLocalLock(string sku, int count);
        Task CreateNoLock(string sku, int count);
        Task CreateBlockingLock(string sku, int count);
    }
}
