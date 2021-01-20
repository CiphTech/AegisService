using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aegis.Model.Helpers;

namespace Aegis.Tests
{
    internal static class HttpExm
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
        
        public static async Task<HttpResponseMessage> GetSignedAsync(this HttpClient client, string url, byte[] privateKey)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.AddSignature(client, url, privateKey);

            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostSignedAsync<T>(this HttpClient client, string url, byte[] privateKey, T data)
        {
            HttpContent content = CreateJsonContent(data);
            
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            request.AddSignature(client, url, privateKey);

            request.Content = content;

            return await client.SendAsync(request);
        }

        private static void AddSignature(this HttpRequestMessage request, HttpClient client, string url, byte[] key)
        {
            string signature = CalcSignature(client, url, key);

            request.Headers.Add("Signature", signature);
        }

        private static string CalcSignature(HttpClient client, string url, byte[] key)
        {
            using var provider = new RSACryptoServiceProvider();
            
            provider.ImportRSAPrivateKey(key, out int _);

            string headers = client.DefaultRequestHeaders
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}:{x.Value.AggregateOrDefault((all, x) => $"{all},{x}")}")
                .AggregateOrDefault((all, x) => $"{all}|{x}");

            string strToSign = $"{url}|{headers}".Trim('/');
            
            byte[] signature = provider.SignData(Encoding.UTF8.GetBytes(strToSign), SHA256.Create());

            return Convert.ToBase64String(signature);
        }
        
        private static HttpContent CreateJsonContent<T>(T data)
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);

            var content = new StringContent(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return content;
        }
    }
}