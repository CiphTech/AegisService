using System;
using System.Linq;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;
using Aegis.Rest.Dto;
using Aegis.Service;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Rest.Controllers
{
    [Route("messages")]
    public class MessagesController : Controller
    {
        private readonly IAegisService _aegisService;

        [HttpPost("send")]
        public async Task<MessageDto> SendMessage([FromBody] SendMessageSpec spec)
        {
            var msg = new AegisMessageInfo(Guid.NewGuid(), spec.ConversationId, spec.SentBy, spec.Title, spec.Body);
            MicInfo mic = await _aegisService.SendMessageAsync(msg);

            MessageDto dto = CreateDto(mic);

            return dto;
        }

        [HttpGet("{conversationId:guid}")]
        public async Task<MessageDto[]> GetMessages(Guid conversationId,[FromQuery] long counter)
        {
            MicInfo[] messages = await _aegisService.GetMessagesAsync(conversationId, counter);

            return messages.Select(CreateDto).ToArray();
        }

        public MessagesController(IAegisService aegisService)
        {
            _aegisService = aegisService;
        }
        
        private static MessageDto CreateDto(MicInfo mic)
        {
            var dto = new MessageDto
            {
                Body = mic.Message.Body,
                ConversationId = mic.Message.ConversationId,
                Id = mic.Message.Id,
                Title = mic.Message.Title,
                Timestamp = mic.UtcTime.ToUnixTime(),
                SentBy = mic.Message.SentBy,
                Counter = mic.Counter
            };
            
            return dto;
        }
    }
}