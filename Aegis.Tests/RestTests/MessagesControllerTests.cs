using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aegis.Model.Helpers;
using Aegis.Rest.Dto;
using FluentAssertions;
using NUnit.Framework;

namespace Aegis.Tests.RestTests
{
    public class MessagesControllerTests : AegisIntegrationTest
    {
        [Test]
        public async Task CreateMessage_ReturnsOk()
        {
            // Arrange

            CreateConversationSpec createConvSpec = CreateDefaultConvSpec();

            HttpResponseMessage createConvResponse = await CreateConversationAsync(createConvSpec);
            createConvResponse.EnsureSuccessStatusCode();

            ConversationDto conv = await GetDataAsync<ConversationDto>(createConvResponse);

            // Act

            var sendMsgSpec = new SendMessageSpec
            {
                ConversationId = conv.Id,
                SentBy = Persons[0].Id,
                Title = "Title",
                Body = "Body"
            };

            HttpResponseMessage sendMsgResponse = await SendMessageAsync(sendMsgSpec);

            // Assert

            sendMsgResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            MessageDto msg = await GetDataAsync<MessageDto>(sendMsgResponse);

            msg.Id.Should().NotBeEmpty();

            DateTime dt = msg.Timestamp.DateFromUnixTime();
            dt.Should().BeWithin(TimeSpan.FromMinutes(1)).Before(DateTime.UtcNow);

            msg.ConversationId.Should().Be(conv.Id);
            msg.SentBy.Should().Be(sendMsgSpec.SentBy);
            msg.Title.Should().Be(sendMsgSpec.Title);
            msg.Body.Should().Be(sendMsgSpec.Body);
        }

        [Test]
        public async Task GetMessages_WithoutCounter_ReturnsAll()
        {
            // Arrange

            CreateConversationSpec createConvSpec = CreateDefaultConvSpec();

            HttpResponseMessage createConvResponse = await CreateConversationAsync(createConvSpec);
            createConvResponse.EnsureSuccessStatusCode();

            ConversationDto conv = await GetDataAsync<ConversationDto>(createConvResponse);

            // Act

            var spec = new SendMessageSpec {ConversationId = conv.Id, SentBy = Persons[0].Id, Title = "Title", Body = "Body"};

            HttpResponseMessage[] sendResponses = await Task.WhenAll(SendMessageAsync(spec), SendMessageAsync(spec));
            sendResponses.Select(x => x.EnsureSuccessStatusCode()).ToArray();

            HttpResponseMessage getResponse = await GetMessagesAsync(conv.Id);
            MessageDto[] messages = await GetDataAsync<MessageDto[]>(getResponse);

            // Assert

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            messages.Length.Should().Be(2);

            foreach (MessageDto dto in messages)
            {
                dto.Id.Should().NotBeEmpty();
                dto.Counter.Should().BeGreaterThan(0L);
                dto.ConversationId.Should().Be(conv.Id);
                dto.SentBy.Should().Be(Persons[0].Id);
                dto.Title.Should().Be("Title");
                dto.Body.Should().Be("Body");
            }
        }

        [Test]
        public async Task GetMessages_WithCounter_ReturnsByCounter()
        {
            // Arrange

            CreateConversationSpec createConvSpec = CreateDefaultConvSpec();

            HttpResponseMessage createConvResponse = await CreateConversationAsync(createConvSpec);
            createConvResponse.EnsureSuccessStatusCode();

            ConversationDto conv = await GetDataAsync<ConversationDto>(createConvResponse);

            // Act
            var spec = new SendMessageSpec {ConversationId = conv.Id, SentBy = Persons[0].Id, Title = "Title", Body = "Body"};

            HttpResponseMessage[] sendResponses = await Task.WhenAll(SendMessageAsync(spec), SendMessageAsync(spec));
            sendResponses.Select(x => x.EnsureSuccessStatusCode());

            HttpResponseMessage getResponse = await GetMessagesAsync(conv.Id, 1L);
            MessageDto[] messages = await GetDataAsync<MessageDto[]>(getResponse);

            // Assert
            
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            messages.Length.Should().Be(1);

            messages[0].Id.Should().NotBeEmpty();
            messages[0].Counter.Should().Be(2L);
            messages[0].ConversationId.Should().Be(conv.Id);
            messages[0].SentBy.Should().Be(Persons[0].Id);
            messages[0].Title.Should().Be("Title");
            messages[0].Body.Should().Be("Body");
        }
    }
}