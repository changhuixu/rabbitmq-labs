using System;
using System.Text;
using RabbitMQ.Client;

namespace OrderApi.RabbitMQ
{
    public interface IRabbitMQClient
    {
        void Publish(string exchange, string routingKey, string payload);
        void CloseConnection();
    }

    public class RabbitMQClient : IRabbitMQClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQClient(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _channel.ConfirmSelect();
        }

        public void Publish(string exchange, string routingKey, string payload)
        {
            var props = _channel.CreateBasicProperties();
            props.AppId = "OrderApi";
            props.Persistent = true;
            props.UserId = "ops0";
            props.MessageId = Guid.NewGuid().ToString("N");
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var body = Encoding.UTF8.GetBytes(payload);
            _channel.BasicPublish(exchange, routingKey, props, body);
            _channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        }

        public void CloseConnection()
        {
            _connection?.Close();
        }
    }
}
