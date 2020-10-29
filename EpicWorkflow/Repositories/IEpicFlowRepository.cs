using System.Collections.Generic;
using System.Threading.Tasks;
using EpicWorkflow.Models;

namespace EpicWorkflow.Repositories
{
    public interface IEpicFlowRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product> GetProductAsync(string projectShortName);
    }
}