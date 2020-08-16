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
    public class PersonsControllerTests : AegisIntegrationTest
    {
        [Test]
        public async Task GetAllPersons_ReturnsArray()
        {
            // Arrange
            
            using HttpClient client = CreateClient();
            
            // Act

            HttpResponseMessage response = await client.GetAsync("/persons");
            
            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            PersonInfoDto[] persons = await GetDataAsync<PersonInfoDto[]>(response);

            persons.Length.Should().Be(2);

            string[] expectedNames = Persons.Select(x => x.Name).ToArray();

            foreach (string name in expectedNames)
                persons.Should().Contain(dto => dto.Name.Equals(name));
        }

        [Test]
        public async Task GetFirstPerson_ReturnsValue()
        {
            // Arrange

            using HttpClient client = CreateClient();

            // Act

            HttpResponseMessage response = await client.GetAsync($"persons/{Persons[0].Id:D}");

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            PersonInfoDto person = await GetDataAsync<PersonInfoDto>(response);

            person.Name.Should().Be(Persons[0].Name);
        }
    }
}