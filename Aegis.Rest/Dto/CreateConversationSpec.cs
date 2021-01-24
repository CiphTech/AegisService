using System;

namespace Aegis.Rest.Dto
{
    public class CreateConversationSpec
    {
        public string Title { get; set; }
        public Guid[] Participants { get; set; }
    }
}