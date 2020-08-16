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
        public static void AddMocks(this IServiceCollection services, AegisPersonInfo[] persons)
        {
            var provider = new Mock<IAegisInitPersonsProvider>(MockBehavior.Strict);

            provider.Setup(x => x.GetPersonsAsync()).ReturnsAsync(persons);
            
            var stg = new Mock<IAegisMessageStorage>();
            stg.Setup(x => x.SaveMessageAsync(It.IsAny<MicInfo>())).Returns(Task.CompletedTask);
            
            services.AddSingleton(provider.Object);
            services.AddSingleton<IAegisService, InMemoryAegisService>();
            services.AddSingleton(stg.Object);
        }
    }
}