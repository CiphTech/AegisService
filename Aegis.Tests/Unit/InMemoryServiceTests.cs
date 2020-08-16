using System;
using System.Linq;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Service;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Aegis.Tests.Unit
{
    public class InMemoryServiceTests
    {
        private IAegisService _aegis;
        private AegisPersonInfo[] _persons;
        private Mock<IAegisMessageStorage> _msgStorageMock;
        
        [SetUp]
        public void Setup()
        {
            var personProvider = new Mock<IAegisInitPersonsProvider>(MockBehavior.Strict);

            _persons = new[]
            {
                new AegisPersonInfo(Guid.NewGuid(), "Test1"),
                new AegisPersonInfo(Guid.NewGuid(), "Test2") 
            };

            personProvider.Setup(x => x.GetPersonsAsync()).ReturnsAsync(_persons);
            
            _msgStorageMock = new Mock<IAegisMessageStorage>();
            _msgStorageMock.Setup(x => x.SaveMessageAsync(It.IsAny<MicInfo>())).Returns(Task.CompletedTask);
            _msgStorageMock.Setup(x => x.GetConversationsAsync()).ReturnsAsync(new AegisConversationInfo[0]);
            _msgStorageMock.Setup(x => x.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<long>())).ReturnsAsync(new MicInfo[0]);

            _aegis = new InMemoryAegisService(personProvider.Object, _msgStorageMock.Object);
        }

        [Test]
        public void CreateConvWithoutAdmin_Throws()
        {
            // Arrange
            
            // Act
            
            Func<Task> action = async () =>
            {
                await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), Guid.Empty, _persons.Select(x => x.Id).ToArray(), "Title"));
            };

            // Assert
            action.Should().Throw<Exception>();
        }

        [Test]
        public void CreateConvWithoutTitle_Throws()
        {
            // Arrange
            
            // Act
            
            Func<Task> action = async () =>
            {
                await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), string.Empty));
            };

            // Assert
            action.Should().Throw<Exception>();
        }

        [Test]
        public void CreateConv_ParticipantsWithoutAdmin_Throws()
        {
            // Arrange
            
            // Act

            Func<Task> action = async () =>
            {
                await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, new[] {_persons[1].Id}, "Title"));
            };

            // Assert

            action.Should().Throw<Exception>();
        }

        [Test]
        public async Task CreateConv_ParticipantsNotInService_Throws()
        {
            // Arrange

            AegisPersonInfo[] persons =
            {
                new AegisPersonInfo(Guid.NewGuid(), "Person1"),
                new AegisPersonInfo(Guid.NewGuid(), "Person2")
            };
            
            // Act

            Func<Task> action = async () =>
            {
                await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), persons[0].Id, persons.Select(x => x.Id).ToArray(), "Title"));
            };

            // Assert

            action.Should().Throw<Exception>().WithMessage("*person*");
        }

        [Test]
        public async Task SendMessage_WithoutConvId_Throws()
        {
            // Arrange
            
            await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title"));

            // Act

            Func<Task> action = async () =>
            {
                await _aegis.SendMessageAsync(new AegisMessageInfo(Guid.NewGuid(), Guid.Empty, _persons[0].Id, "Title", "Body"));
            };

            // Assert

            action.Should().Throw<Exception>().WithMessage("*conversation*");
        }

        [Test]
        public async Task SendMessage_WithIncorrectConvId_Throws()
        {
            // Arrange
            
            await _aegis.CreateConversationAsync(new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title"));
            
            Guid incorrectId = Guid.NewGuid();

            // Act

            Func<Task> action = async () =>
            {
                await _aegis.SendMessageAsync(new AegisMessageInfo(Guid.NewGuid(), incorrectId, _persons[0].Id, "Title", "Body"));
            };

            // Assert

            action.Should().Throw<Exception>().WithMessage($"*conversation '{incorrectId:D}'*");
        }

        [Test]
        public async Task SendMessage_WithIncorrectUserId_Throws()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            await _aegis.CreateConversationAsync(conv);

            Guid incorrectId = Guid.NewGuid();

            // Act

            Func<Task> action = async () =>
            {
                await _aegis.SendMessageAsync(new AegisMessageInfo(Guid.NewGuid(), conv.Id, incorrectId, "Title", "Body"));
            };

            // Assert

            action.Should().Throw<Exception>()
                .WithMessage($"*conversation '{conv.Id:D}'*");
        }

        [Test]
        public async Task SendMessage_WithCorrectParams_ReturnsOk()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            await _aegis.CreateConversationAsync(conv);

            // Act

            var msg = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");
            MicInfo mic = await _aegis.SendMessageAsync(msg);

            // Assert

            mic.Message.Id.Should().NotBeEmpty();
            mic.Message.ConversationId.Should().Be(conv.Id);
            mic.UtcTime.Should().BeWithin(TimeSpan.FromMinutes(1)).Before(DateTime.UtcNow);
            mic.Message.SentBy.Should().Be(_persons[0].Id);
            mic.Message.Title.Should().Be("Title");
            mic.Message.Body.Should().Be("Body");
            mic.Counter.Should().Be(1L);
        }

        [Test]
        public async Task GetMessagesByCounter_ReturnsNextMsg()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            
            var m1 = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");
            var m2 = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");

            // Act

            await _aegis.CreateConversationAsync(conv);
            MicInfo mic1 = await _aegis.SendMessageAsync(m1);
            MicInfo mic2 = await _aegis.SendMessageAsync(m2);
            
            MicInfo[] messages = await _aegis.GetMessagesAsync(conv.Id, 1L);

            // Assert
            mic1.Counter.Should().Be(1L);
            mic2.Counter.Should().Be(2L);
            messages.Length.Should().Be(1);

            messages[0].Message.Id.Should().Be(m2.Id);
            messages[0].Message.Title.Should().Be(m2.Title);
            messages[0].Message.Body.Should().Be(m2.Body);
            messages[0].Counter.Should().Be(2L);
        }

        [Test]
        public async Task GetMessagesByLastCounter_ReturnsEmpty()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            
            var m1 = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "First", "Body");
            
            var m2 = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Second", "Body");

            // Act

            await _aegis.CreateConversationAsync(conv);
            MicInfo mic1 = await _aegis.SendMessageAsync(m1);
            MicInfo mic2 = await _aegis.SendMessageAsync(m2);
            
            MicInfo[] messages = await _aegis.GetMessagesAsync(conv.Id, 2L);

            // Assert
            mic1.Counter.Should().Be(1L);
            mic2.Counter.Should().Be(2L);
            messages.Should().BeEmpty();
        }

        [Test]
        public async Task GetMessages_WithIncorrectConvId_Throws()
        {
            // Arrange
            
            // Act

            Func<Task> action = async () => { await _aegis.GetMessagesAsync(Guid.NewGuid(), 0L); };

            // Assert

            action.Should().Throw<Exception>().WithMessage("*conversation*");
        }

        [Test]
        public async Task SendMessage_SavesItInStorage()
        {
            // Arrange
            
            // Act

            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            await _aegis.CreateConversationAsync(conv);

            var msg = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");
            MicInfo mic = await _aegis.SendMessageAsync(msg);

            // Assert

            mic.Should().NotBeNull();
            mic.Message.Id.Should().NotBeEmpty();

            await Task.Delay(5000);
            
            _msgStorageMock.Verify(x => x.SaveMessageAsync(It.IsNotNull<MicInfo>()), Times.Once());
        }

        [Test]
        public async Task GetMessage_ConvExistsInMemory_GetsFromMemoryAndStorage()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");
            var msg = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");
            
            // Act

            await _aegis.CreateConversationAsync(conv);

            await _aegis.SendMessageAsync(msg);

            MicInfo[] messages = await _aegis.GetMessagesAsync(conv.Id, 0L);

            // Assert

            messages.Length.Should().Be(1);
            _msgStorageMock.Verify(x => x.GetMessagesAsync(conv.Id, 0L), Times.Once());
        }

        [Test]
        public async Task GetMessage_DoesntExistInMemory_GetsFromStorage()
        {
            // Arrange
            var personProvider = new Mock<IAegisInitPersonsProvider>(MockBehavior.Strict);

            var persons = new[]
            {
                new AegisPersonInfo(Guid.NewGuid(), "Test1"),
                new AegisPersonInfo(Guid.NewGuid(), "Test2") 
            };

            personProvider.Setup(x => x.GetPersonsAsync()).ReturnsAsync(persons);
            
            var msgStorage = new Mock<IAegisMessageStorage>(MockBehavior.Strict);

            Guid convId = Guid.NewGuid();

            msgStorage.Setup(x => x.SaveMessageAsync(It.IsAny<MicInfo>())).Returns(Task.CompletedTask);
            msgStorage.Setup(x => x.GetConversationsAsync()).ReturnsAsync(new[]
                {new AegisConversationInfo(convId, persons[0].Id, persons.Select(x => x.Id).ToArray(), "Conv")});
            msgStorage.Setup(x => x.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(new[]
                {
                    new MicInfo(new AegisMessageInfo(Guid.NewGuid(), convId, Guid.NewGuid(), "Title", "Body"), DateTime.UtcNow, 1L)
                });
            
            var svc = new InMemoryAegisService(personProvider.Object, msgStorage.Object);

            // Act

            MicInfo[] messages = await svc.GetMessagesAsync(convId, 0L);

            // Assert

            messages.Length.Should().Be(1);
            msgStorage.Verify(x => x.GetMessagesAsync(convId, 0L), Times.Once());
            msgStorage.Verify(x => x.GetConversationsAsync(), Times.Once());

            messages[0].Message.Title.Should().Be("Title");
            messages[0].Message.Body.Should().Be("Body");
            messages[0].Message.ConversationId.Should().Be(convId);
            messages[0].Counter.Should().Be(1L);
        }

        [Test]
        public async Task CreateConv_Twice_Throws()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");

            // Act

            Func<Task> action = async () => await _aegis.CreateConversationAsync(conv);

            await action();

            // Assert

            action.Should().Throw<Exception>().WithMessage($"Conversation '{conv.Id:D}' already exists");
        }

        [Test]
        public async Task SendMessage_Twice_Throws()
        {
            // Arrange
            
            var conv = new AegisConversationInfo(Guid.NewGuid(), _persons[0].Id, _persons.Select(x => x.Id).ToArray(), "Title");

            var msg = new AegisMessageInfo(Guid.NewGuid(), conv.Id, _persons[0].Id, "Title", "Body");
            
            // Act

            await _aegis.CreateConversationAsync(conv);

            Func<Task<MicInfo>> action = async () => await _aegis.SendMessageAsync(msg);

            MicInfo mic = await action();

            // Assert

            mic.Should().NotBeNull();
            action.Should().Throw<Exception>().WithMessage($"Cannot send message '{msg.Id:D}' twice");
        }
    }
}