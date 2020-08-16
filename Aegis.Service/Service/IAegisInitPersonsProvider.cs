using System.Threading.Tasks;
using Aegis.Model;

namespace Aegis.Service
{
    public interface IAegisInitPersonsProvider
    {
        Task<AegisPersonInfo[]> GetPersonsAsync();
    }
}