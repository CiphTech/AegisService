using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
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
    }
}