using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DivisaAlexaSkill.Core;
using DivisaAlexaSkill.Domain;
using Newtonsoft.Json;

namespace DivisaAlexaSkill.Infrastructure
{
    public class DivisaProvider : IDivisaProvider
    {
        private const string API_KEY = "json?key=1496|30_kpDftA9pXzAVMRuqZR0XqH2Dtv~aS";
        private const string API_QUOTE = "quotes";
        public DivisaProvider()
        {

        }

        public async Task<double> GetDivisa(string divisaSource, string divisaTarget)
        {
            DivisaResultModel divisaResponse;

            using (var client = new HttpClient())
            {
                try
                {
                    //client.BaseAddress = new Uri("https://api.cambio.today/v1/quotes");
                    string path = $"/{divisaSource}/{divisaTarget}/{API_KEY}";
                    var response = await client.GetAsync("https://api.cambio.today/v1/quotes/BTC/EUR/json?key=1496|30_kpDftA9pXzAVMRuqZR0XqH2Dtv~aS");
                    response.EnsureSuccessStatusCode();

                    string divisaResultJSON = await response.Content.ReadAsStringAsync();
                    divisaResponse = JsonConvert.DeserializeObject<DivisaResultModel>(divisaResultJSON);
                    return divisaResponse.Divisa.Amount;
                }
                catch (HttpRequestException)
                {
                    return -1;
                }
            }
        }
    }
}
