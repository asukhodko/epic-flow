using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpicWorkflow.Models;
using Microsoft.Extensions.Configuration;

namespace EpicWorkflow.Repositories
{
    public class EpicFlowRepository : IEpicFlowRepository
    {
        private readonly IConfiguration _configuration;

        private List<Product> _products;

        public EpicFlowRepository(IConfiguration configuration)
        {
            _configuration = configuration;

            LoadProducts();
        }

        public Task<IEnumerable<Product>> GetProductsAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(_products);
        }

        public Task<Product> GetProductAsync(string projectShortName)
        {
            return Task.FromResult<Product>(_products.FirstOrDefault(p => p.ProjectShortName == projectShortName));
        }

        private void LoadProducts()
        {
            var productsInConfig = _configuration.GetSection("Products").GetChildren();
            _products = productsInConfig.Select(p => new Product
            {
                ProjectShortName = p["ProjectShortName"],
                Name = p["Name"],
                BacklogId = p["BacklogId"]
            }).ToList();
        }
    }
}