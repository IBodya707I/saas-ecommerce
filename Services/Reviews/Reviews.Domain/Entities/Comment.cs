namespace Reviews.Domain.Entities;

using MongoDB.Bson;
using Reviews.Domain.Exceptions;

public class Comment
{
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
    public string Author { get; private set; } = "";
    public string Text { get; private set; } = "";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Comment() { }

    public Comment(string author, string text, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Comment author is required.");

        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Comment text is required.");

        Author = author.Trim();
        Text = text.Trim();
        CreatedAt = createdAt;
    }
}
