namespace Reviews.Domain.Common;

using MongoDB.Bson;

public abstract class BaseEntity
{
    public ObjectId Id { get; protected set; } = ObjectId.GenerateNewId();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public int Version { get; protected set; }

    public void Touch() => UpdatedAt = DateTime.UtcNow;
    public void BumpVersion() => Version++;
}
