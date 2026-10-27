namespace Reviews.Domain.Entities;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;

public class ProductDiscussion : BaseEntity
{
    public long ProductId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Open";
    public int MessageCount { get; private set; }

    private ProductDiscussion() { }

    public ProductDiscussion(long productId, string title)
    {
        if (productId <= 0)
            throw new DomainException("ProductId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        ProductId = productId;
        Title = title.Trim();
        Status = "Open";
        MessageCount = 0;
    }

    public void IncrementMessageCount()
    {
        MessageCount++;
        Touch();
    }

    public void Close()
    {
        Status = "Closed";
        Touch();
    }
}
