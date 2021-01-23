using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;

namespace Aegis.Service
{
    public class AegisConversation
    {
        private readonly int _maxMessagesInMemory;
        
        private readonly IAegisMessageStorage _msgStorage;
        private long _counter;
        
        private readonly ConcurrentDictionary<long, MicInfo> _messages;
        public AegisConversationInfo Info { get; }

        public Guid Id => Info.Id;

        public MicInfo[] GetMessages(long counter) => GetMessagesByCounter(counter);

        public async Task<bool> HasParticipantAsync(Guid id) => Info.Participants.Contains(id);

        public async Task<MicInfo> SendMessage(AegisMessageInfo info)
        {
            if (_messages.Values.Any(x => x.Message.Id == info.Id))
                throw new Exception($"Cannot send message '{info.Id:D}' twice");
            
            if (!await HasParticipantAsync(info.SentBy))
                throw new Exception($"Conversation '{Info.Id}' doesn't contain participant '{info.SentBy}'");

            var mic = new MicInfo(info, DateTime.UtcNow, GetCounter());

            _messages[mic.Counter] = mic;

            return mic;
        }

        public AegisConversation(AegisConversationInfo info, IAegisMessageStorage msgStorage)
        {
            _msgStorage = msgStorage;
            Info = info;
            _messages = new ConcurrentDictionary<long, MicInfo>();
            _counter = 0L;
            _maxMessagesInMemory = 100;

            _ = SaveToStorageLoop();
        }

        private long GetCounter() => Interlocked.Increment(ref _counter);

        private MicInfo[] GetMessagesByCounter(long counter) => _messages.Where(x => x.Key > counter).Select(x => x.Value).ToArray();

        private async Task SaveToStorageLoop()
        {
            var current = 0L;
            TimeSpan timeout = TimeSpan.FromSeconds(3);
            
            while (true)
            {
                try
                {
                    MicInfo[] items = GetMessagesByCounter(current);

                    if (!items.IsNullOrEmpty())
                    {
                        await Task.WhenAll(items.Select(x => _msgStorage.SaveMessageAsync(x)));
                        current = items.Max(x => x.Counter);
                    }

                    if (_messages.Count > _maxMessagesInMemory)
                    {
                        long[] msgToRemove = _messages.Where(x => x.Key < current - _maxMessagesInMemory).Select(x => x.Key).ToArray();

                        bool success = msgToRemove.Select(x => _messages.TryRemove(x, out _)).Aggregate((all, x) => all && x);
                        
                        // TODO: log if fail
                    }

                    await Task.Delay(timeout);
                }
                catch
                {
                    // TODO: save to log
                    throw;
                }
            }
        }
    }
}