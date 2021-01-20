using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Model.Helpers;
using Aegis.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Aegis.Rest.Middleware
{
    public class AegisAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAegisInitPersonsProvider _personsProvider;

        public async Task Invoke(HttpContext context)
        {
            StringValues headerValues;

            Guid clientId;

            if (context.Request.Headers.TryGetValue("client-id", out headerValues))
            {
                clientId = Guid.Parse(headerValues[0]);
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Client is not specified");
                return;
            }

            string signature;

            if (context.Request.Headers.TryGetValue("signature", out headerValues))
            {
                signature = headerValues[0];
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Signature is not specified");
                return;
            }
            
            string headers = context.Request.Headers
                .Where(x => x.Key.EqualsNoCase("client-id"))
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}:{x.Value.AggregateOrDefault((all, x) => $"{all},{x}")}")
                .AggregateOrDefault((all, x) => $"{all}|{x}");

            string strToSign = $"{context.Request.Path}{context.Request.QueryString}|{headers}".Trim('/');
            
            AegisPersonInfo person = (await _personsProvider.GetPersonsAsync()).FirstOrDefault(x => x.Id == clientId);

            if (person != null)
            {
                bool result = CheckSignature(strToSign, signature, person.PublicKey);

                if (result)
                    await _next.Invoke(context);
                else
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Authorization failed");
                    // context.Response.Headers.Add("str", strToSign);
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync($"Cannot find person '{clientId}'");
                return;
            }
        }

        public AegisAuthMiddleware(RequestDelegate next, IAegisInitPersonsProvider personsProvider)
        {
            _next = next;
            _personsProvider = personsProvider;
        }

        private static bool CheckSignature(string str, string signature, byte[] publicKey)
        {
            using RSACryptoServiceProvider provider = new RSACryptoServiceProvider();

            provider.ImportRSAPublicKey(publicKey, out _);

            return provider.VerifyData(Encoding.UTF8.GetBytes(str), SHA256.Create(), Convert.FromBase64String(signature));
        }
    }
}