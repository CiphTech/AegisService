using System;
using System.Threading.Tasks;
using Aegis.Model;
using Aegis.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Aegis.Tests.RestTests
{
    internal static class TestExtentions
    {
        public static void AddMocks(this IServiceCollection services, TestPersonsProvider provider)
        {
            var stg = new Mock<IAegisMessageStorage>();
            stg.Setup(x => x.SaveMessageAsync(It.IsAny<MicInfo>())).Returns(Task.CompletedTask);
            
            services.AddSingleton<IAegisInitPersonsProvider>(provider);
            services.AddSingleton<IAegisService, InMemoryAegisService>();
            services.AddSingleton(stg.Object);
        }
    }
}