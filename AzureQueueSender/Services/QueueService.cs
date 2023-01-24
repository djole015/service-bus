using Azure.Messaging.ServiceBus;
using AzureQueueSender.Models;
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

        // emos1 PutIntoSendingQueue()

        // email model from Emos1
        public static void PutEmailIntoSendingQueue(EmailModel emailModel )
        { 
        // salje jedan message
        }

        // email model from Emos1
        public static void CreateMessageBatchForSendingQueue(List<EmailModel> emailModel)
        {
            // salje batch
        }

        // test method 
        public async Task SendMessagesToSenderQueueAsync(IList<EmailModel> serviceBusMessages, string queueName)
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

                //var serialNumber = await sender.ScheduleMessageAsync(message, (DateTimeOffset)serviceBusMessages[i].SendAt);

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
                Console.WriteLine($"A batch of {messageBatch.Count} messages has been published to the messagesenderqueue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
                await SendMessagesToSenderQueueAsync(serviceBusMessages, queueName);
            }
        }
    }
}
