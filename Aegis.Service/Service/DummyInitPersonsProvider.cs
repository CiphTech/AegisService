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
                new AegisPersonInfo(Guid.NewGuid(), "Yoba 1", "admin",PrimitiveHelper.StringToByteArray("022bdf3d14dd7a21d648716c64b1c8d2ca31f8e57bf27ffd9f7abf4fcd2882c5"), new byte[0]),
                new AegisPersonInfo(Guid.NewGuid(), "Yoba 2", "user",PrimitiveHelper.StringToByteArray("44d803ae119e67ce6d598c258769071b3fe97c304a0e277325ba12350f4134ca"), new byte[0])
            };
        }
    }
}