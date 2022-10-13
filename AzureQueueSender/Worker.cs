using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using AzureQueueSender.Services;

namespace AzureQueueSender
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private QueueService queue;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            queue = new QueueService(config);
        }

        int count = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var result = new ApplicationDbContext(_config))
                {
                    var emails = result.Email.ToList().Skip(5 * (count)).Take(5);
                    count++;

                    foreach (var email in emails)
                    {
                        _logger.LogInformation($"{email.UserID} - {email.CreatedOn}");
                        await queue.SendMessageToQueueAsync(email, "emailqueue");
                    }
                }

                await Task.Delay(5*1000, stoppingToken);
            }
        }
    }
}