using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using AzureQueueSender.Services;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

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
                    var emails = result.Email.ToList().Skip(10 * (count)).Take(10).ToList();
                    //List<EmailModel> emails = new List<EmailModel>();
                    //var email = result.Email.Where(e => e.ID == 230595).FirstOrDefault();
                    //var messageBody = JsonSerializer.Serialize(email);
                    //_logger.LogInformation($"email Size : {Encoding.UTF8.GetBytes(messageBody).Length}");
                    //for (int i = 0; i < 2; i++)
                    //{
                    //    emails.Add(email);
                    //}
                    count++;

                    await queue.SendMessagesToSenderQueueAsync(emails, _config.GetValue<string>("ServiceBus:SenderQueueName"));
                }

                await Task.Delay(5*1000, stoppingToken);
            }
        }
    }
}