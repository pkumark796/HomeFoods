using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HomeFoods.Domain.Entities;
using HomeFoods.Domain.Repositories;

namespace HomeFoods.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query, int limit = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<Product>();
            }

            var normalizedQuery = Normalize(query);
            var tokens = Tokenize(normalizedQuery);

            var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);

            return products
                .Select(product => new
                {
                    Product = product,
                    SearchText = BuildSearchText(product)
                })
                .Where(x => tokens.All(token => x.SearchText.Contains(token)))
                .OrderByDescending(x => Score(x.Product, x.SearchText, normalizedQuery, tokens))
                .ThenBy(x => x.Product.Name)
                .Select(x => x.Product)
                .Take(limit);
        }

        private static string BuildSearchText(Product product)
        {
            var parts = new[]
            {
                product.Name,
                product.Description,
                product.SKU,
                product.Unit,
                product.Category?.Name,
                product.Brand?.Name
            };

            return Normalize(string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part))));
        }

        private static int Score(Product product, string searchText, string normalizedQuery, string[] tokens)
        {
            var score = 0;

            var normalizedName = Normalize(product.Name);

            if (normalizedName.Contains(normalizedQuery))
            {
                score += 100;
            }

            if (searchText.Contains(normalizedQuery))
            {
                score += 50;
            }

            score += tokens.Count(token => normalizedName.Contains(token)) * 10;
            score += tokens.Count(token => searchText.Contains(token)) * 5;

            return score;
        }

        private static string[] Tokenize(string value)
        {
            return value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length * 2);
            char? previous = null;

            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (previous.HasValue &&
                        ((char.IsDigit(previous.Value) && char.IsLetter(character)) ||
                         (char.IsLetter(previous.Value) && char.IsDigit(character))))
                    {
                        builder.Append(' ');
                    }

                    builder.Append(character);
                    previous = character;
                }
                else
                {
                    builder.Append(' ');
                    previous = null;
                }
            }

            return string.Join(' ', builder
                .ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
    }
}
