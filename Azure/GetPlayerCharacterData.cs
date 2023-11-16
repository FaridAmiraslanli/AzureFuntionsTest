using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab.ServerModels;
using System.Collections.Generic;
using PlayFab;

namespace DynamicBox.CloudScripts
{
    public static class GetPlayerCharacterData
    {
        private const string LeftGunKey = "LeftGunType";
        private const string RightGunKey = "RightGunType";
        private const string NitroKey = "Nitro";
        private const string EngineKey = "Engine";
        private const string SteeringKey = "Steering";

        [FunctionName("GetPlayerCharacterData")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string TitleId = args["TitleId"];
            string PlayFabId = args["PlayFabId"]; 
            string CharacterId = args["CharacterId"]; 

            var settings = new PlayFabApiSettings
            {
                TitleId = TitleId,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = args["EntityToken"]
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);

            
            var getCharacterDataRequest = new GetCharacterDataRequest
            {
                PlayFabId = PlayFabId,
                CharacterId = CharacterId,
                Keys = new List<string>
                {
                   LeftGunKey, RightGunKey, NitroKey, EngineKey, SteeringKey
                }
            };

            try
            {
                var getCharacterDataResult = await serverApi.GetCharacterDataAsync(getCharacterDataRequest);
                
                Engine engine = JsonConvert.DeserializeObject<Engine>(getCharacterDataResult.Result.Data[EngineKey].Value);
                Steering steering = JsonConvert.DeserializeObject<Steering>(getCharacterDataResult.Result.Data[SteeringKey].Value);
                Data data = new Data
                {
                    LeftGunType = getCharacterDataResult.Result.Data[LeftGunKey].Value,
                    RightGunType = getCharacterDataResult.Result.Data[RightGunKey].Value,
                    Nitro = float.Parse(getCharacterDataResult.Result.Data[NitroKey].Value),
                    Engine = engine,
                    Steering = steering
                };

                GetPlayerCharacterResultData resultData = new GetPlayerCharacterResultData
                {
                    ResponseType = "GetPlayerCharacterData",
                    Data = data
                };

                string json = JsonConvert.SerializeObject(resultData);

                return json;

            }
            catch (PlayFabException ex)
            {
                log.LogError($"Error getting character data: {ex.Message}");
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
    }

    [Serializable]
    public class GetPlayerCharacterResultData
    {
        public string ResponseType;

        public Data Data;

    }

    [Serializable]
    public class Data
    {
        public string LeftGunType;
        public string RightGunType;
        public float Nitro;
        public Engine Engine;
        public Steering Steering;
    }

    [Serializable]
    public class Engine
    {
        public float Acceleration;
        public float MaxSpeed;
    }

    [Serializable]
    public class Steering
    {
        public float Acceleration;
        public float MaxRotation;
    }
}
