using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;
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
        protected AegisPersonInfo User;
        protected AegisPersonInfo Admin;
        protected IReadOnlyDictionary<Guid, byte[]> PrivateKeys;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

        [SetUp]
        public async Task SetUp()
        {
            var personsProvider = TestPersonsProvider.Create(out PrivateKeys);

            Persons = await personsProvider.GetPersonsAsync();
            User = Persons.First(x => x.Role.EqualsNoCase("user"));
            Admin = Persons.First(x => x.Role.EqualsNoCase("admin"));
            
            Factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddMocks(personsProvider);
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

            client.DefaultRequestHeaders.Add("client-id", User.Id.ToString("N"));
            
            return client;
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
            
            return await client.PostSignedAsync("conversations/create", PrivateKeys[User.Id], spec);
        }

        protected async Task<HttpResponseMessage> GetConversationsAsync()
        {
            using HttpClient client = CreateClient();

            return await client.GetSignedAsync("conversations", PrivateKeys[User.Id]);
        }
        
        protected async Task<HttpResponseMessage> SendMessageAsync(SendMessageSpec spec)
        {
            using HttpClient client = CreateClient();
            
            return await client.PostSignedAsync("messages/send", PrivateKeys[User.Id], spec);
        }

        protected async Task<HttpResponseMessage> GetMessagesAsync(Guid conversationId, long counter = 0L)
        {
            using HttpClient client = CreateClient();

            if (counter > 0L)
                return await client.GetSignedAsync($"messages/{conversationId:D}?counter={counter}", PrivateKeys[User.Id]);
            else
                return await client.GetSignedAsync($"messages/{conversationId:D}", PrivateKeys[User.Id]);
        }
    }
}