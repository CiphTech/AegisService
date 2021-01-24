using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Aegis.Model.Helpers;
using Aegis.Rest.Dto;
using FluentAssertions;
using NUnit.Framework;

namespace Aegis.Tests.RestTests
{
    public class ConversationsControllerTests : AegisIntegrationTest
    {
        [Test]
        public async Task CreateConversation_ReturnsOk()
        {
            // Arrange

            CreateConversationSpec spec = CreateDefaultConvSpec();
            
            // Act
            
            HttpResponseMessage response = await CreateConversationAsync(spec);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ConversationDto dto = await GetDataAsync<ConversationDto>(response);
            
            dto.Id.Should().NotBeEmpty();
            dto.Title.Should().Be(spec.Title);
            dto.Participants.Should().Contain(spec.Participants);
        }

        [Test]
        public async Task GetConversations_ReturnsCreatedConv()
        {
            // Arrange
            
            CreateConversationSpec spec = CreateDefaultConvSpec();
            
            // Act

            HttpResponseMessage createResponse = await CreateConversationAsync(spec);

            createResponse.EnsureSuccessStatusCode();

            HttpResponseMessage getResponse = await GetConversationsAsync();

            // Assert

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            ConversationDto[] dtos = await GetDataAsync<ConversationDto[]>(getResponse);

            dtos.Length.Should().Be(1);
            ConversationDto conv = dtos[0];

            conv.Id.Should().NotBeEmpty();
            conv.Title.Should().Be(spec.Title);
            conv.Participants.Length.Should().Be(spec.Participants.Length);
            conv.Participants.Should().Contain(spec.Participants);
        }

        [Test]
        public async Task GetConversations_ReturnsOnlyYours()
        {
            // Arrange

            var conv1 = new CreateConversationSpec {Admin = User.Id, Participants = new[] {User.Id, Admin.Id}, Title = "conv1"};
            var conv2 = new CreateConversationSpec {Admin = User2.Id, Participants = new[] {User.Id, User2.Id}, Title = "conv2"};

            await CreateConversationAsync(conv1, User.Id);
            await CreateConversationAsync(conv2, User2.Id);

            // Act

            HttpResponseMessage userResponse = await GetConversationsAsync(User.Id);
            HttpResponseMessage user2Response = await GetConversationsAsync(User2.Id);
            HttpResponseMessage adminResponse = await GetConversationsAsync(Admin.Id);

            // Assert

            userResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            user2Response.StatusCode.Should().Be(HttpStatusCode.OK);
            adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            ConversationDto[] userConversations = await GetDataAsync<ConversationDto[]>(userResponse);
            ConversationDto[] user2Conversations = await GetDataAsync<ConversationDto[]>(user2Response);
            ConversationDto[] adminConversations = await GetDataAsync<ConversationDto[]>(adminResponse);

            userConversations.Length.Should().Be(2);
            userConversations.Should().Contain(x => x.Title.EqualsNoCase(conv1.Title));
            userConversations.Should().Contain(x => x.Title.EqualsNoCase(conv2.Title));

            user2Conversations.Length.Should().Be(1);
            user2Conversations.Should().Contain(x => x.Title.EqualsNoCase(conv2.Title));

            adminConversations.Length.Should().Be(1);
            adminConversations.Should().Contain(x => x.Title.EqualsNoCase(conv1.Title));
        }
    }
}