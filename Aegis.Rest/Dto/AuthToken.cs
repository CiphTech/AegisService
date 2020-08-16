namespace Aegis.Rest.Dto
{
    public class AuthToken
    {
        public string RealName { get; }

        public string Token { get; }

        public uint ExpirationTime { get; }
    }
}