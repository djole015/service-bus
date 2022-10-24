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

        public async Task SendMessageToQueueAsync<T>(IList<T> serviceBusMessages, string queueName)
        {
            if(serviceBusMessages.Count == 0)
            {
                return;
            }

            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };

            var client = new ServiceBusClient(_config.GetValue<string>("ServiceBus:ConnectionString"), clientOptions);

            var sender = client.CreateSender(queueName);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 0; i < serviceBusMessages.Count; i++)
            {
                    
                var messageBody = JsonSerializer.Serialize(serviceBusMessages[i]);

                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

                // try adding a message to the batch
                if (messageBatch.TryAddMessage(message))
                {
                    serviceBusMessages.RemoveAt(i);
                }
                else
                {
                    //await sender.SendMessageAsync(message);
                    // if it is too large for the batch
                    //throw new Exception($"The message {i} is too large to fit in the batch.");

                    break;
                }
                
            }
            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {messageBatch.Count} messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
                await SendMessageToQueueAsync(serviceBusMessages, queueName);
            }
        }
    }
}
