using System;
using System.Threading.Tasks;
using Aegis.Model;

namespace Aegis.Service
{
    public class DummyInitPersonsProvider  : IAegisInitPersonsProvider
    {
        private readonly AegisPersonInfo[] _persons;
        
        public Task<AegisPersonInfo[]> GetPersonsAsync()
        {
            return Task.FromResult(_persons);
        }

        public DummyInitPersonsProvider()
        {
            _persons = new[]
            {
                new AegisPersonInfo(Guid.NewGuid(), "Yoba 1"),
                new AegisPersonInfo(Guid.NewGuid(), "Yoba 2")
            };
        }
    }
}