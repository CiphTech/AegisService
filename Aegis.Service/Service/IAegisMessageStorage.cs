using System;
using System.Threading.Tasks;
using Aegis.Model;

namespace Aegis.Service
{
    public interface IAegisMessageStorage
    {
        Task SaveMessageAsync(MicInfo mic);

        Task<AegisConversationInfo[]> GetConversationsAsync();

        Task<MicInfo[]> GetMessagesAsync(Guid conversationId, long counter);
    }
}