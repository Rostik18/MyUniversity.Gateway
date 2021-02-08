using MyUniversity.Gateway.Services.Configs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MyUniversity.Gateway.Services.MessageClient
{
    public class MessageClient : IMessageClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;

        private readonly ILogger<MessageClient> _logger;
        private readonly MessageClientConfigs _messageClientConfigs;

        public MessageClient(ILogger<MessageClient> logger,
            MessageClientConfigs messageClientConfigs)
        {
            _logger = logger;
            _messageClientConfigs = messageClientConfigs;

            var factory = new ConnectionFactory()
            {
                HostName = _messageClientConfigs.HostName,
                Port = _messageClientConfigs.Port,
                UserName = _messageClientConfigs.UserName,
                Password = _messageClientConfigs.Password
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            _logger.LogInformation($"MessageClient registered by address {_messageClientConfigs.HostName}:{_messageClientConfigs.Port}");
        }

        public Task<TRespond> RequestAsync<TRequest, TRespond>(string requestQueue, TRequest requestObject)
        {
            var replyQueueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);

            var props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            string response = string.Empty;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    response = Encoding.UTF8.GetString(body);
                }
            };

            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObject));
            channel.BasicPublish(
                exchange: "",
                routingKey: requestQueue,
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return Task.FromResult(JsonConvert.DeserializeObject<TRespond>(response));
        }
    }
}
