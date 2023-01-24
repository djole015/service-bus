using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using AzureQueueSender.Services;
using MessageReceiver.Services;
using System.Text.Json;

namespace MessageReceiver
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private ReceiverService queue;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            queue = new ReceiverService(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await queue.SetupMessageSenderAsync(_config.GetValue<string>("ServiceBus:SenderQueueName"));

                await Task.Delay(5 * 1000, stoppingToken);
            }
        }
    }
}