using Emos1Messaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emos1Messaging.DataContext
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(@"Data Source=DESKTOP-0C8HRU2\SQLEXPRESS;Initial Catalog=DemoESR;Integrated Security=True");
        }

        public DbSet<EmailModel> Email { get; set; }
    }
}
