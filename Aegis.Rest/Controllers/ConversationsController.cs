using System;
using System.Linq;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Rest.Dto;
using Aegis.Service;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Rest.Controllers
{
    [Route("conversations")]
    public class ConversationsController : Controller
    {
        private readonly IAegisService _aegisService;

        [HttpPost]
        public async Task<ConversationDto> CreateConversation([FromBody] CreateConversationSpec spec)
        {
            var info = new AegisConversationInfo(Guid.NewGuid(), spec.Admin, spec.Participants, spec.Title);
            await _aegisService.CreateConversationAsync(info);

            return new ConversationDto
            {
                Id = info.Id,
                Participants = info.Participants,
                Title = info.Title
            };
        }

        [HttpGet]
        public async Task<ConversationDto[]> GetAllConversations()
        {
            AegisConversationInfo[] info = await _aegisService.GetConversationsAsync();

            return info.Select(x => new ConversationDto
                {
                    Id = x.Id,
                    Participants = x.Participants,
                    Title = x.Title
                })
                .ToArray();
        }

        public ConversationsController(IAegisService aegisService)
        {
            _aegisService = aegisService;
        }
    }
}