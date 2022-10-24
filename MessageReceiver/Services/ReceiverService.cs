using Azure.Messaging.ServiceBus;
using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using EmailClient;
using MessageDbLogger;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageReceiver.Services
{
    public class ReceiverService
    {
        private readonly IConfiguration _config;

        public ReceiverService(IConfiguration config)
        {
            _config = config;
        }

        public async Task ReceiveMessageFromQueueAsync(string queueName)
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
            // If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open

            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            var client = new ServiceBusClient(_config.GetValue<string>("ServiceBus:ConnectionString"));

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {

            string jsonString = Encoding.UTF8.GetString(args.Message.Body);
            var message = JsonSerializer.Deserialize<EmailModel>(jsonString);

            try
            {
                Console.WriteLine($"Receiver: Message {message.EmailTo} received");

                // send email
                var messageClientService = new MessageClientService();
                messageClientService.MessageSent += new MessageService(_config).OnMessageSent;

                messageClientService.SendingMessage(message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message); 
                return;    
            }
            
            //Console.WriteLine($"Received: {body}");


            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
