using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Reviews.Infrastructure.Persistence;

public class ProcessedEventDocument
{
    [BsonId]
    public string EventId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
