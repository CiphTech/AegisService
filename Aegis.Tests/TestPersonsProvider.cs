using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Service;

namespace Aegis.Tests
{
    internal class TestPersonsProvider : IAegisInitPersonsProvider
    {
        private readonly AegisPersonInfo[] _persons;

        public Task<AegisPersonInfo[]> GetPersonsAsync() => Task.FromResult(_persons);

        public TestPersonsProvider(AegisPersonInfo[] persons)
        {
            _persons = persons;
        }

        public static TestPersonsProvider Create(out IReadOnlyDictionary<Guid, byte[]> privateKeys)
        {
            var keys = new Dictionary<Guid, byte[]>(2);

            var yoba1 = GeneratePerson("Yoba Admin", "admin");
            var yoba2 = GeneratePerson("Yoba 1", "user");
            var yoba3 = GeneratePerson("Yoba 2", "user");

            keys[yoba1.person.Id] = yoba1.privateKey;
            keys[yoba2.person.Id] = yoba2.privateKey;
            keys[yoba3.person.Id] = yoba3.privateKey;

            privateKeys = keys;

            return new TestPersonsProvider(new[] {yoba1.person, yoba2.person, yoba3.person});
        }

        private static (AegisPersonInfo person, byte[] privateKey) GeneratePerson(string name, string role)
        {
            using var provider = new RSACryptoServiceProvider();

            byte[] publicKey = provider.ExportRSAPublicKey();
            byte[] privateKey = provider.ExportRSAPrivateKey();

            byte[] hash = SHA256.Create().ComputeHash(publicKey);

            return (new AegisPersonInfo(Guid.NewGuid(), name, role, hash, publicKey), privateKey);
        }
    }
}