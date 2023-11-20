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

        [FunctionName("GetPlayerCharacterData")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string titleId = args["TitleId"];
            string playFabId = args["PlayFabId"]; 
            string characterId = args["CharacterId"]; 

            var settings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = args["EntityToken"]
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);

            
            var getCharacterDataRequest = new GetCharacterDataRequest
            {
                PlayFabId = playFabId,
                CharacterId = characterId,
                Keys = new List<string>
                {
                   DataKeys.LeftGunKey, DataKeys.RightGunKey, DataKeys.NitroKey, DataKeys.EngineKey, DataKeys.SteeringKey
                }
            };

            try
            {
                var getCharacterDataResult = await serverApi.GetCharacterDataAsync(getCharacterDataRequest);

                Engine engine = JsonConvert.DeserializeObject<Engine>(getCharacterDataResult.Result.Data[DataKeys.EngineKey].Value);
                Steering steering = JsonConvert.DeserializeObject<Steering>(getCharacterDataResult.Result.Data[DataKeys.SteeringKey].Value);
                Data data = new Data
                {
                    LeftGunType = getCharacterDataResult.Result.Data[DataKeys.LeftGunKey].Value,
                    RightGunType = getCharacterDataResult.Result.Data[DataKeys.RightGunKey].Value,
                    Nitro = float.Parse(getCharacterDataResult.Result.Data[DataKeys.NitroKey].Value),
                    Engine = engine,
                    Steering = steering
                };

                GetPlayerCharacterResultData resultData = new GetPlayerCharacterResultData
                {
                    ResponseType = "GetPlayerCharacterData",
                    Data = data
                };

                // string json = JsonConvert.SerializeObject(resultData);

                // return json;

                if (getCharacterDataResult.Error == null)
                {
                    return new
                    {
                        success = true,
                        code = 200,
                        message = "Request Successful",
                        data = resultData
                    };
                }
                else
                {
                int httpCodeForGetCharacterData = getCharacterDataResult.Error.HttpCode;
                    return new
                    {
                        success = false,
                        code = httpCodeForGetCharacterData,
                        message = "Bad Request",
                        data = resultData
                    };
                }
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

    public static class DataKeys
    {
        public const string LeftGunKey = "LeftGunType";
        public const string RightGunKey = "RightGunType";
        public const string NitroKey = "Nitro";
        public const string EngineKey = "Engine";
        public const string SteeringKey = "Steering";
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
