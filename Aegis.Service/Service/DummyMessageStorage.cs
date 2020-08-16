using System;
using System.Threading.Tasks;
using Aegis.Model;

namespace Aegis.Service
{
    public class DummyMessageStorage : IAegisMessageStorage
    {
        public Task SaveMessageAsync(MicInfo mic)
        {
            return Task.CompletedTask;
        }

        public Task<AegisConversationInfo[]> GetConversationsAsync()
        {
            return Task.FromResult(new AegisConversationInfo[0]);
        }

        public Task<MicInfo[]> GetMessagesAsync(Guid conversationId, long counter)
        {
            return Task.FromResult(new MicInfo[0]);
        }
    }
}