using System;

namespace Aegis.Model
{
    public class AegisMessageInfo
    {
        public Guid Id { get; }

        public Guid ConversationId { get; }

        public Guid SentBy { get; }
        
        public string Title { get; }

        public string Body { get; }

        public AegisMessageInfo(Guid id, Guid conversationId, Guid sentBy, string title, string body)
        {
            Id = id;
            ConversationId = conversationId;
            SentBy = sentBy;
            Title = title;
            Body = body;
        }
    }
}