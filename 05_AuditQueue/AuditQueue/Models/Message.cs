using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuditQueue.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Exchange { get; set; }
        public string Route { get; set; }
        public string MessageId { get; set; }
        public DateTime Timestamp { get; set; }
        public long TimestampUnix { get; set; }
        public string AppId { get; set; }
        public string UserId { get; set; }
        public byte[] Body { get; set; }
    }
}
