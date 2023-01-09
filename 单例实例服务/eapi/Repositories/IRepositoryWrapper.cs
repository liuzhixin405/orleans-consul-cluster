using eapi.Data;
using eapi.Models;

namespace eapi.Repositories
{
    public interface IRepositoryWrapper
    {
        IRepository<Order> OrderRepository { get;  }
        IRepository<Product> ProductRepository { get;  }
        Task Trans(Func<Task> func);
        //ProductDbContext context { get; }

    }
}
