using System;
using System.Threading;
using System.Threading.Tasks;
using AuditQueue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace AuditQueue.Services
{
    public class AuditQueueService : BackgroundService
    {
        private readonly ILogger<AuditQueueService> _logger;
        private readonly IConnection _connection;
        private IModel _channel;
        private const string QueueName = "ordering.auditqueue";
        private readonly IMessagesService _messagesService;

        public AuditQueueService(ILogger<AuditQueueService> logger, IConnection connection, IMessagesService messagesService)
        {
            _logger = logger;
            _connection = connection;
            _messagesService = messagesService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _connection.CreateModel();
            _channel.QueueDeclarePassive(QueueName);
            _channel.BasicQos(0, 1, false);
            _logger.LogInformation($"Queue [{QueueName}] is waiting for messages.");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (bc, ea) =>
            {
                _logger.LogInformation($"Auditing msg# '{ea.BasicProperties.MessageId}'.");

                try
                {
                    await _messagesService.Create(ParseMessage(ea));
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (AlreadyClosedException)
                {
                    _logger.LogInformation("RabbitMQ is closed!");
                }
                catch (Exception e)
                {
                    _logger.LogError(default, e, e.Message);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            await Task.CompletedTask;
        }

        private static Message ParseMessage(BasicDeliverEventArgs ea)
        {
            var msg = new Message
            {
                MessageId = ea.BasicProperties.MessageId,
                Exchange = ea.Exchange,
                Route = ea.RoutingKey,
                Body = ea.Body.ToArray(),
                AppId = ea.BasicProperties.AppId,
                UserId = ea.BasicProperties.UserId,
                TimestampUnix = ea.BasicProperties.Timestamp.UnixTime
            };

            if (ea.BasicProperties.Timestamp.UnixTime > 0)
            {
                var offset = DateTimeOffset.FromUnixTimeMilliseconds(ea.BasicProperties.Timestamp.UnixTime);
                msg.Timestamp = offset.UtcDateTime;
            }

            return msg;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
            _logger.LogInformation("RabbitMQ connection is closed.");
        }
    }
}
