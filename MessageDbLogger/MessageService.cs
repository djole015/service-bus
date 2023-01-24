using Azure.Messaging.ServiceBus;
using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageDbLogger
{
    public class MessageService
    {
        private readonly IConfiguration _config;

        public MessageService(IConfiguration config)
        {
            _config = config;
        }

        public async void OnMessageSent(EmailModel message)
        {
            try
            {
                await SendMessageToLoggerQueueAsync(message, _config.GetValue<string>("ServiceBus:LoggerQueueName"));

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessageToLoggerQueueAsync(EmailModel serviceBusMessage, string queueName)
        {
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };

            var client = new ServiceBusClient(_config.GetValue<string>("ServiceBus:ConnectionString"), clientOptions);

            var sender = client.CreateSender(queueName);

            var messageBody = JsonSerializer.Serialize(serviceBusMessage);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

                // schedule message

                //var serialNumber = await sender.ScheduleMessageAsync(message, (DateTimeOffset)serviceBusMessages[i].SendAt);

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Message {serviceBusMessage.ID} has been published to the messageloggerqueue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        public async Task ReceiveMessageFromQueueAsync(string queueName)
        {
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            var client = new ServiceBusClient(_config.GetValue<string>("ServiceBus:ConnectionString"));

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += LogMessageToDatabaseHandler;

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
        private async Task LogMessageToDatabaseHandler(ProcessMessageEventArgs args)
        {

            string jsonString = Encoding.UTF8.GetString(args.Message.Body);
            var message = JsonSerializer.Deserialize<EmailModel>(jsonString);
            //var messageStatus = message.Status; 

            try
            {
                Console.WriteLine($"LoggerReceiver: Message {message.EmailTo} received");

                // log email
                // ovde mora da se uzem parametar koji je klijent u pitanju i da se loguje u njegovu bau: npr. demo, cem, hou,..
                // dinamicki se generise connection string za bazu na osnovu client code 
                string clientCode = "";
                string connectionString = "Data Source=emos-sql.eastus.cloudapp.azure.com;Initial Catalog=DemoESR;user id=emos1sa;password=Asdfghjklqwe1;MultipleActiveResultSets=true;Integrated Security=false;";

                connectionString = connectionString.Replace("Catalog=DemoESR", $"Catalog={clientCode}ESR");

                using (var result = new ApplicationDbContext(_config))
                {
                    message.ID = 0;
                    message.SentOn = DateTime.Now;

                    result.Add(message);
                    result.SaveChanges();

                    Console.WriteLine($"DbLogger: Message {message.ID} saved to db.");
                }

                //throw new Exception($"Message {message.ID} couldn't be saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await SendMessageToLoggerQueueAsync(message, _config.GetValue<string>("ServiceBus:LoggerQueueName"));
                return;
            }

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
