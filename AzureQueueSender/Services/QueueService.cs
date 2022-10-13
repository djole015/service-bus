using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureQueueSender.Services
{
    public class QueueService
    {
        private readonly IConfiguration _config;

        public QueueService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMessageToQueueAsync<T>(T serviceBusMessage, string queueName)
        {
            var client = new ServiceBusClient(_config.GetConnectionString("AzureServiceBus"));

            var sender = client.CreateSender(queueName);

            var messageBody = JsonSerializer.Serialize(serviceBusMessage);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            await sender.SendMessageAsync(message);

        }
    }
}
