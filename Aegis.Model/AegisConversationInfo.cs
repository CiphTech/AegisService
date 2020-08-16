using System;

namespace Aegis.Model
{
    public class AegisConversationInfo
    {
        public Guid Id { get; }
        public Guid Admin { get; }

        public Guid[] Participants { get; }

        public string Title { get; }

        public AegisConversationInfo(Guid id, Guid admin, Guid[] participants, string title)
        {
            Id = id;
            Admin = admin;
            Participants = participants;
            Title = title;
        }
    }
}