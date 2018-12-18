using System.Threading.Tasks;

namespace DivisaAlexaSkill.Core
{
    public interface IDivisaProvider
    {
        Task<double> GetDivisa(string divisaSource, string divisaTarget);
    }
}
