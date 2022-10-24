using AzureQueueSender.DataContext;
using AzureQueueSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void OnMessageSent(EmailModel message)
        {
            try
            {
                using (var result = new ApplicationDbContext(_config))
                {
                    message.ID = 0;
                    message.SentOn = DateTime.Now;

                    result.Add(message);
                    result.SaveChanges();

                    Console.WriteLine($"DbLogger: Message {message.ID} saved to db.");
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
