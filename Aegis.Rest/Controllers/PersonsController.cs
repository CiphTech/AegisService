using System;
using System.Linq;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Rest.Dto;
using Aegis.Service;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Rest.Controllers
{
    [Route("persons")]
    public class PersonsController : Controller
    {
        private readonly IAegisService _service;

        [HttpGet()]
        public async Task<PersonInfoDto[]> GetPersonsAsync()
        {
            AegisPersonInfo[] persons = await _service.GetAllPersonsAsync();

            return persons.Select(p => new PersonInfoDto {Id = p.Id, Name = p.Name}).ToArray();
        }

        [HttpGet("{id}")]
        public async Task<PersonInfoDto> GetPersonAsync(Guid id)
        {
            AegisPersonInfo person = await _service.GetPersonAsync(id);
            return new PersonInfoDto {Id = person.Id, Name = person.Name};
        }

        public PersonsController(IAegisService service)
        {
            _service = service;
        }
    }
}