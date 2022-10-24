using AzureQueueSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailClient
{
    public class MessageClientService
    {
        public void SendingMessage(EmailModel? message)
        {
            try
            {
                Console.WriteLine($"EmailClient: Message to {message.EmailTo} sent");
                OnMessageSent(message);
            }
            catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
            
        }

        public delegate void MessageSentEventHandler(EmailModel message);

        public event MessageSentEventHandler MessageSent;

        protected virtual void OnMessageSent(EmailModel message)
        {
            if (MessageSent != null)
            {
                MessageSent(message);
            }
        }
    }
}
