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
    public static class UpdateCharacterData
    {
        [FunctionName("UpdateCharacterData")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string titleId = args["TitleId"];
            string playFabId = args["PlayFabId"]; 
            string characterId = args["CharacterId"]; 

            string leftGunType = args["LeftGunType"];
            string rightGunType = args["RightGunType"];

            string nitroValue = "1.7";

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

            // Engine engine = new Engine
            // {
            //     Acceleration = 3.7f,
            //     MaxSpeed = 5.9f
            // };

            // string engineJsonData = JsonConvert.SerializeObject(engine);

            // Steering steering = new Steering
            // {
            //     Acceleration = 0.5f,
            //     MaxRotation = 52f
            // };

            // string steeringJsonData = JsonConvert.SerializeObject(steering);

            var updateCharacterDataRequest = new UpdateCharacterDataRequest
            {
                PlayFabId = playFabId,
                CharacterId = characterId,
                      Data = new Dictionary<string, string>()
                    {
                        {DataKeys.LeftGunKey, leftGunType},
                        {DataKeys.RightGunKey, rightGunType},
					    {DataKeys.NitroKey, nitroValue},
					    // {DataKeys.EngineKey, engineJsonData},
					    // {DataKeys.SteeringKey, steeringJsonData},
                    }
            };

            try
            {
                var updateCharacterDataResult = await serverApi.UpdateCharacterDataAsync(updateCharacterDataRequest);
                
                return updateCharacterDataResult.Result.ToString();
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
}