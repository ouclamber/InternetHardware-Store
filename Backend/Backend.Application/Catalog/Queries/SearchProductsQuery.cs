using MediatR;
using Backend.Application.DTOs;

namespace Backend.Application.Catalog.Queries;

public class SearchProductsQuery : IRequest<IReadOnlyList<ProductDto>>
{
    public string Query { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public SearchProductsQuery(string query)
    {
        Query = query;
    }
}