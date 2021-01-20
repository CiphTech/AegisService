using System;

namespace Aegis.Model
{
    public class AegisPersonInfo
    {
        public Guid Id { get; }
        public string Name { get; }
        public byte[] PublicKeyHash { get; }
        public byte[] PublicKey { get; }
        public string Role { get; }

        public AegisPersonInfo(Guid id, string name, string role, byte[] publicKeyHash, byte[] publicKey)
        {
            Id = id;
            Name = name;
            PublicKeyHash = publicKeyHash;
            PublicKey = publicKey;
            Role = role;
        }
    }
}