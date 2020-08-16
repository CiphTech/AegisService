using System;

namespace Aegis.Rest.Dto
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        
        public Guid ConversationId { get; set; }

        public Guid SentBy { get; set; }

        public long Counter { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public uint Timestamp { get; set; }
    }
}