using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;

namespace Aegis.Service
{
    public class InMemoryAegisService : IAegisService
    {
        private readonly IAegisMessageStorage _msgStorage;
        private readonly ConcurrentDictionary<Guid, AegisPersonInfo> _persons;
        private readonly ConcurrentDictionary<Guid, AegisConversation> _conversations;
        
        public async Task<AegisPersonInfo[]> GetAllPersonsAsync()
        {
            return _persons.Values.ToArray();
        }

        public async Task<AegisPersonInfo> GetPersonAsync(Guid id)
        {
            return _persons[id];
        }

        public async Task<MicInfo[]> GetMessagesAsync(Guid conversationId, long counter)
        {
            MicInfo[] stgMessages = new MicInfo[0];

            if (!_conversations.TryGetValue(conversationId, out AegisConversation conversation))
            {
                bool isInStorage = (await _msgStorage.GetConversationsAsync()).Any(x => x.Id == conversationId);

                if (!isInStorage)
                    throw new Exception($"Conversation '{conversationId:D}' is not found");
            }

            stgMessages = await _msgStorage.GetMessagesAsync(conversationId, counter);
            MicInfo[] inMemoryMessages = conversation?.GetMessages(counter);

            if (inMemoryMessages.IsNullOrEmpty())
                return stgMessages;
            else
                return inMemoryMessages.Concat(stgMessages).ToArray();
        }

        public Task<bool> ConversationHasParticipant(Guid conversationId, Guid personId)
        {
            return GetConversation(conversationId).HasParticipantAsync(personId);
        }

        public async Task<MicInfo> SendMessageAsync(AegisMessageInfo info)
        {
            AegisConversation conversation = GetConversation(info.ConversationId);
            
            return await conversation.SendMessage(info);
        }

        private AegisConversation GetConversation(Guid conversationId)
        {
            if (!_conversations.TryGetValue(conversationId, out AegisConversation conversation))
                throw new Exception($"Conversation '{conversationId:D}' is not found");
            return conversation;
        }

        public async Task CreateConversationAsync(AegisConversationInfo info)
        {
            if (_conversations.ContainsKey(info.Id))
                throw new Exception($"Conversation '{info.Id:D}' already exists");
            
            if (info.Admin.IsEmpty())
                throw new Exception("Cannot create a conversation without admin");
            
            if (!info.Title.IsSpecified())
                throw new Exception("Cannot create a conversation without title");
            
            if (!info.Participants.Contains(info.Admin))
                throw new Exception("Admin should be in participants");

            bool personsFound = info.Participants.All(x => _persons.ContainsKey(x));
            
            if (!personsFound)
                throw new Exception($"Some persons for conversation '{info.Id:D}' are not presented in service");

            var conversation = new AegisConversation(info, _msgStorage);

            _conversations[conversation.Id] = conversation;
        }

        public async Task<AegisConversationInfo[]> GetConversationsAsync()
        {
            return _conversations.Values.Select(x => x.Info).ToArray();
        }

        public InMemoryAegisService(IAegisInitPersonsProvider initPersonsProvider, IAegisMessageStorage msgStorage)
        {
            _msgStorage = msgStorage;
            _persons = new ConcurrentDictionary<Guid, AegisPersonInfo>();
            _conversations = new ConcurrentDictionary<Guid, AegisConversation>();

            foreach (AegisPersonInfo person in initPersonsProvider.GetPersonsAsync().Result)
                _persons[person.Id] = person;
        }
    }
}