using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emos1Messaging.Models
{
    public class EmailModel
    {
        public long ID { get; set; }
        public int EmailTemplateID { get; set; }
        public int? UserID { get; set; }
        public int? GroupID { get; set; }
        public string? Body { get; set; }
        public string? EmailTo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? SendAt { get; set; }
        public DateTime? SentOn { get; set; }
        public int? Status { get; set; }
        public string? ExceptionDesc { get; set; }
        public int? Priority { get; set; }
        public int? RetryCount { get; set; }
        public string? ReplyTo { get; set; }
    }
}
