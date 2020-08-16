using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Rest;
using Aegis.Rest.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Aegis.Tests.RestTests
{
    [TestFixture]
    public class AegisIntegrationTest
    {
        protected WebApplicationFactory<Startup> Factory;
        protected AegisPersonInfo[] Persons;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

        [SetUp]
        public void SetUp()
        {
            Persons = new[]
            {
                new AegisPersonInfo(Guid.NewGuid(), "Person 1"),
                new AegisPersonInfo(Guid.NewGuid(), "Person 2")
            };
            
            Factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddMocks(Persons);
                });
            });
        }

        [TearDown]
        public void TearDown()
        {
            Factory.Dispose();
            Factory = null;
        }

        protected CreateConversationSpec CreateDefaultConvSpec()
        {
            return new CreateConversationSpec
            {
                Title = "Test conv",
                Admin = Persons[0].Id,
                Participants = Persons.Select(x => x.Id).ToArray()
            };
        }

        protected HttpClient CreateClient()
        {
            HttpClient client = Factory.CreateClient();
            
            return client;
        }

        protected HttpContent CreateJsonContent<T>(T data)
        {
            string json = JsonSerializer.Serialize(data, _jsonOptions);

            var content = new StringContent(json);
            
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return content;
        }

        protected async Task<T> GetDataAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        protected async Task<HttpResponseMessage> CreateConversationAsync(CreateConversationSpec spec)
        {
            using HttpClient client = CreateClient();

            using HttpContent content = CreateJsonContent(spec);

            return await client.PostAsync("conversations/create", content);
        }

        protected async Task<HttpResponseMessage> GetConversationsAsync()
        {
            using HttpClient client = CreateClient();

            return await client.GetAsync("conversations");
        }
        
        protected async Task<HttpResponseMessage> SendMessageAsync(SendMessageSpec spec)
        {
            using HttpClient client = CreateClient();

            using HttpContent content = CreateJsonContent(spec);

            return await client.PostAsync("messages/send", content);
        }

        protected async Task<HttpResponseMessage> GetMessagesAsync(Guid conversationId, long counter = 0L)
        {
            using HttpClient client = CreateClient();

            if (counter > 0L)
                return await client.GetAsync($"messages/{conversationId:D}?counter={counter}");
            else
                return await client.GetAsync($"messages/{conversationId:D}");
        }
    }
}