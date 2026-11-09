namespace Platform.Aggregator.Models;

public record FullOrderDto(
    OrderDto? Order,
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<ReviewDto> Reviews,
    IReadOnlyList<string> Warnings);
