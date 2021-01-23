using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;
using Aegis.Rest.Dto;
using FluentAssertions;
using NUnit.Framework;

namespace Aegis.Tests.RestTests
{
    public class AegisAuthTests : AegisIntegrationTest
    {
        [Test]
        public async Task GetConversations_WrongKey_Returns401()
        {
            // Arrange
            
            using HttpClient client = CreateClient();

            // Act

            HttpResponseMessage response = await client.GetSignedAsync("conversations", PrivateKeys[Admin.Id]);

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetConversations_Unauthorized_ContainsHeader()
        {
            // Arrange
            
            using HttpClient client = CreateClient();

            // Act

            HttpResponseMessage response = await client.GetSignedAsync("conversations", PrivateKeys[Admin.Id]);

            // Assert

            string expected = $"conversations|client-id:{User.Id:N}";
            GetStrHeader(response).Should().Be(expected);
        }

        [Test]
        public async Task CreateConversation_WrongBody_Returns401()
        {
            // Arrange

            using HttpClient client = CreateClient();
            HttpContent content = HttpExm.CreateJsonContent(new CreateConversationSpec {Admin = User.Id, Participants = new[] {Admin.Id, User.Id}, Title = "Test"});
            var url = "conversations";
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url){Content = content};
            request.AddSignature(client, url, PrivateKeys[User.Id]);

            // Act
            
            request.Content = HttpExm.CreateJsonContent(new CreateConversationSpec {Admin = User.Id, Participants = new[] {Admin.Id, User.Id}, Title = "Test2"});

            HttpResponseMessage response = await client.SendAsync(request);

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private static string GetStrHeader(HttpResponseMessage response)
        {
            response.Headers.Should().Contain(x => x.Key.EqualsNoCase("str"));
            string[] values = response.Headers.GetValues("str").ToArray();
            values.Length.Should().Be(1);
            return values[0];
        }
    }
}