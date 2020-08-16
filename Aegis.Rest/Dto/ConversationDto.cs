using System;

namespace Aegis.Rest.Dto
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public Guid[] Participants { get; set; }
    }
}