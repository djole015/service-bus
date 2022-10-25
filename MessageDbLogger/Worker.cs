using AzureQueueSender.DataContext;
using AzureQueueSender.Models;

namespace MessageDbLogger
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private MessageService service;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            service = new MessageService(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await service.ReceiveMessageFromQueueAsync(_config.GetValue<string>("ServiceBus:LoggerQueueName"));

                await Task.Delay(10*1000, stoppingToken);
            }
        }
    }
}