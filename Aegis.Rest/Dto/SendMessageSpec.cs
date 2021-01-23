using System;
using System.Runtime.Serialization;

namespace Aegis.Rest.Dto
{
    public class SendMessageSpec
    {
        public Guid ConversationId { get; set; }
        
        public string Title { get; set; }

        public string Body { get; set; }
    }
}