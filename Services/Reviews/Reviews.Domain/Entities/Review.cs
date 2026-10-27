namespace Reviews.Domain.Entities;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;
using Reviews.Domain.ValueObjects;

public class Review : BaseEntity
{
    private readonly List<Comment> _comments = new();

    public long ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public Author Author { get; private set; } = null!;
    public int Rating { get; private set; }
    public string Text { get; private set; } = string.Empty;

    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private Review() { } 

    public Review(long productId, string productName, Author author, int rating, string text)
    {
        if (productId <= 0)
            throw new DomainException("ProductId must be greater than zero.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required.");

        if (author is null)
            throw new DomainException("Author is required.");

        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Review text is required.");

        ProductId = productId;
        ProductName = productName.Trim();
        Author = author;
        Rating = rating;
        Text = text.Trim();
    }

    public Review(long productId, string productName, Author author, Rating rating, string text)
        : this(productId, productName, author, rating?.Value ?? throw new DomainException("Rating is required."), text)
    {
    }

    public void Update(int rating, string text)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Review text is required.");

        Rating = rating;
        Text = text.Trim();
        Touch();
    }

    public void Update(Rating rating, string text)
    {
        if (rating is null)
            throw new DomainException("Rating is required.");

        Update(rating.Value, text);
    }

    public void AddComment(string author, string text)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Comment author is required.");

        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Comment text is required.");

        _comments.Add(new Comment(author, text, DateTime.UtcNow));
        Touch();
    }
}
