using System;
using System.Net;
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

        private const double EurosToPesetas = 166.39;
        private const double PesetasToEuros = 0.006;

        public DivisaProvider()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }
        public double GetEurosPesetas(string divisaSource, string divisaTarget)
        {
            if (divisaSource.Equals("EUR") && divisaTarget.Equals("PST"))
                return EurosToPesetas;
            else if (divisaSource.Equals("PST") && divisaTarget.Equals("EUR"))
                return PesetasToEuros;
            return -1;
        } 

        public async Task<double> GetDivisa(string divisaSource, string divisaTarget)
        {
            DivisaResultModel divisaResponse;

            using (var client = new HttpClient())
            {
                try
                {
                    string path = $"http://api.cambio.today/v1/quotes/{divisaSource}/{divisaTarget}/{API_KEY}";
                    var response = await client.GetAsync(path);
                    response.EnsureSuccessStatusCode();

                    string divisaResultJSON = await response.Content.ReadAsStringAsync();
                    divisaResponse = JsonConvert.DeserializeObject<DivisaResultModel>(divisaResultJSON);
                    return Math.Round(divisaResponse.Result.Amount, 2);
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }
    }
}
