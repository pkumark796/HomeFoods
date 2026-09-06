using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeFoods.Domain.Entities;

namespace HomeFoods.Application.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<Product>> SearchProductsAsync(string query, int limit = 20, CancellationToken cancellationToken = default);
    }
}
