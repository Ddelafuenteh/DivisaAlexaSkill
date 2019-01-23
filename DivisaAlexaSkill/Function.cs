using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using DivisaAlexaSkill.Core;
using DivisaAlexaSkill.Domain;
using DivisaAlexaSkill.Infrastructure;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DivisaAlexaSkill
{
    public class Function
    {

        private readonly IDivisaProvider divisaProvider = new DivisaProvider();

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            try
            {
                if (input.GetRequestType() == typeof(LaunchRequest))
                {
                    log.LogLine($"Default LaunchRequest made");
                    innerResponse = new PlainTextOutputSpeech();
                    (innerResponse as PlainTextOutputSpeech).Text = "Conversor de divisas listo";
                }
                else if (input.GetRequestType() == typeof(IntentRequest))
                {
                    var intentRequest = (IntentRequest)input.Request;

                    switch (intentRequest.Intent.Name)
                    {
                        case "AMAZON.CancelIntent":
                            log.LogLine($"AMAZON.CancelIntent: enviando mensaje de cancelación.");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "Cerrando Divisa";
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.StopIntent":
                            log.LogLine($"AMAZON.StopIntent: enviando mensaje de stop.");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "Cerrando Divisa";
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.HelpIntent":
                            log.LogLine($"AMAZON.HelpIntent: enviando mensaje de ayuda");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "Indica una cantidad, su moneda de origen y su moneda de destino";
                            break;
                        case "AskCurrencyConversion":
                            log.LogLine($"Pidiendo cambio de cantidad");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = GetCurrencyConversion(intentRequest.Intent);

                            break;

                        case "AskExchangeRate":
                            log.LogLine($"Pidiendo divisa");
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = GetExchangeRate(intentRequest.Intent);

                            break;
                        default:
                            log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                            innerResponse = new PlainTextOutputSpeech();
                            (innerResponse as PlainTextOutputSpeech).Text = "No entiendo lo que me estas pidiendo, por favor, prueba de otra forma.";
                            break;
                    }

                }
            }
            catch (Exception)
            {
                (innerResponse as PlainTextOutputSpeech).Text = $"Lo siento, se ha producido un error al leer los datos introducidos. Por favor, prueba de otra forma.";
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));

            return response;
        }

        private double GetDivisa(string divisaSource, string divisaTarget)
        {
            return divisaProvider.GetDivisa(divisaSource, divisaTarget).Result;
        }

        private string GetExchangeRate(Intent intent)
        {
            string sourceId = intent.Slots["divisaSource"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id;
            string targetId = intent.Slots["divisaDest"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id;

            double conversion;
            if (sourceId.Equals("PST") || targetId.Equals("PST"))
            {
                conversion = divisaProvider.GetEurosPesetas(sourceId, targetId);
                if (conversion == -1)
                    return "Este cambio solo es valido entre euros y pesetas";
            }
            else
            {
                conversion = GetDivisa(sourceId, targetId);
                if (conversion == -1)
                    return "Lo siento, no he encontrado alguna de las monedas que solicitas";
            }

            string exchangeRateResult = $"La tasa actual de cambio es un {intent.Slots["divisaSource"].Value} son {conversion} {intent.Slots["divisaDest"].Value}";
            return exchangeRateResult;
        }

        private string GetCurrencyConversion(Intent intent)
        {
            string amount = intent.Slots["amount"].Value;
            string partialAmount = intent.Slots["partialAmount"].Value;
            string sourceId = intent.Slots["divisaSource"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id;
            string targetId = intent.Slots["divisaDest"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id;

            if (!String.IsNullOrEmpty(partialAmount))
                amount = $"{amount}.{partialAmount}";

            double conversion;
            if (sourceId.Equals("PST") || targetId.Equals("PST"))
            {
                conversion = divisaProvider.GetEurosPesetas(sourceId, targetId);
                if (conversion == -1)
                    return "Este cambio solo es valido entre euros y pesetas";
            }
            else
            {
                conversion = GetDivisa(sourceId, targetId);
                if (conversion == -1)
                    return "Lo siento, no he encontrado alguna de las monedas que solicitas";
            }
            conversion *= Double.Parse(amount);



            string conversionResult = $"El resultado es que {amount} {intent.Slots["divisaSource"].Value} son: {conversion} {intent.Slots["divisaDest"].Value}";

            return conversionResult;
        }
        
    }
}
