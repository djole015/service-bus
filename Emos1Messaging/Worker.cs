using Emos1Messaging.DataContext;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emos1Messaging
{
    public class Worker : BackgroundService
    {
        public static void CheckEmails()
        {
            using (var result = new ApplicationDbContext())
            {
                var emails = result.Email.ToList().Take(10);

                foreach (var e in emails)
                {
                    Console.WriteLine($"{e.UserID} - {e.CreatedOn}");
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
