using System;
using System.Threading.Tasks;
using Aegis.Model;

namespace Aegis.Service
{
    public interface IAegisService
    {
        Task<AegisPersonInfo[]> GetAllPersonsAsync();

        Task<AegisPersonInfo> GetPersonAsync(Guid id);

        Task<MicInfo[]> GetMessagesAsync(Guid conversationId, long counter);

        Task<bool> ConversationHasParticipant(Guid conversationId, Guid personId);

        Task<MicInfo> SendMessageAsync(AegisMessageInfo info);

        Task CreateConversationAsync(AegisConversationInfo info);

        Task<AegisConversationInfo[]> GetConversationsAsync();
    }
}