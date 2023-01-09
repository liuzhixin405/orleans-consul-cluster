using eapi.Data;
using eapi.interfaces.Models;
using eapi.Repositories;
using eapi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.EntityFrameworkCore;

namespace eapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService service;
        public ProductController(IProductService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await service.FindAll();
        }

        [HttpPost]
        public async Task Create(string sku,int count)
        {
             await service.Create(sku,count);
        }
    }
}
