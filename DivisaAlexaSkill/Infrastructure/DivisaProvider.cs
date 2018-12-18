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
            string path = $"{divisaSource}/{divisaTarget}/json?key=1496|30_kpDftA9pXzAVMRuqZR0XqH2Dtv~aS";
            try
            {
                HttpClient Client = new HttpClient();
                Client.BaseAddress = new Uri("https://api.cambio.today/v1/quotes/");
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await Client.GetAsync(path);
                string divisaResultJSON = await response.Content.ReadAsStringAsync();
                divisaResponse = JsonConvert.DeserializeObject<DivisaResultModel>(divisaResultJSON);
            }
            catch
            {
                return -1;
            }
            return divisaResponse.Divisa.Amount;
            if (divisaResponse != null && divisaResponse.Status.Equals("OK"))
                return divisaResponse.Divisa.Value;

            return -1;
        }
    }
}
