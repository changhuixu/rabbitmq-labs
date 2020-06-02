using System.Collections.Generic;
using System.Threading.Tasks;
using AuditQueue.Models;
using MongoDB.Driver;

namespace AuditQueue.Services
{
    public interface IMessagesService
    {
        Task<List<Message>> GetAll();
        Task<List<Message>> Get(string msgId);
        Task Create(Message x);
        Task Update(string msgId, Message msg);
        Task Remove(Message msg);
        Task Remove(string msgId);
    }

    public class MessagesService : IMessagesService
    {
        private readonly IMongoCollection<Message> _messages;

        public MessagesService(IMongoOption mongoOption)
        {
            var client = new MongoClient(mongoOption.ConnectionString);
            var database = client.GetDatabase(mongoOption.DatabaseName);
            _messages = database.GetCollection<Message>(mongoOption.MessagesCollectionName);
        }

        public async Task<List<Message>> GetAll() => await _messages.Find(x => true).SortBy(x => x.Timestamp).ToListAsync();

        public async Task<List<Message>> Get(string msgId) => await _messages.Find(x => x.MessageId == msgId).SortBy(x => x.Timestamp).ToListAsync();

        public async Task Create(Message x) => await _messages.InsertOneAsync(x);

        public async Task Update(string msgId, Message msg) => await _messages.ReplaceOneAsync(x => x.MessageId == msgId, msg);

        public async Task Remove(Message msg) => await _messages.DeleteOneAsync(x => x.MessageId == msg.MessageId);

        public async Task Remove(string msgId) => await _messages.DeleteOneAsync(x => x.MessageId == msgId);
    }

    public interface IMongoOption
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string MessagesCollectionName { get; set; }
    }

    public class MongoOption : IMongoOption
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string MessagesCollectionName { get; set; }
    }
}
