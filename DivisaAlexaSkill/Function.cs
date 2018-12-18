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

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = "Divisa listo para usarse";
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
                        (innerResponse as PlainTextOutputSpeech).Text = GetExchangeRate(intentRequest.Intent).ToString();

                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = "No entiendo lo que me estas pidiendo, pero si sigues así te daré una patada voladora.";
                        break;
                }

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

        private double GetExchangeRate(Intent intent)
        {
            return GetDivisa(
                    intent.Slots["divisaSource"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id,
                    intent.Slots["divisaDest"].Resolution.Authorities?.FirstOrDefault().Values.FirstOrDefault().Value.Id);
        }

        private string GetCurrencyConversion(Intent intent)
        {
            ;
            double conversionResult = GetExchangeRate(intent) * Double.Parse(intent.Slots["amount"].Value);
            return conversionResult.ToString();
        }
    }
}
