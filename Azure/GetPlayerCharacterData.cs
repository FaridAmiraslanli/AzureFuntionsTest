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

            string titleId = args["titleId"];
            string entityId = args["entityId"]; // This should be the PlayFabId of the player
            string characterId = args["characterId"]; // The specific character's ID for which you want to get data

            var settings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = args["entityToken"]
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);

            // Get character data
            var getCharacterDataRequest = new GetCharacterDataRequest
            {
                PlayFabId = entityId,
                CharacterId = characterId,
                // Keys = new List<string>
                // {
                //     "YourCharacterDataKey" // Replace with the key for the character data you want to retrieve
                // }
            };

            try
            {
                var getCharacterDataResult = await serverApi.GetCharacterDataAsync(getCharacterDataRequest);

                if (getCharacterDataResult.Result.Data.ContainsKey("YourCharacterDataKey"))
                {
                    var characterDataValue = getCharacterDataResult.Result.Data["YourCharacterDataKey"].Value;
                    return new
                    {
                        success = true,
                        characterData = characterDataValue
                    };
                }
                else
                {
                    return new
                    {
                        success = false,
                        error = "Character data key not found"
                    };
                }
            }
            catch (PlayFabException ex)
            {
                // Handle the exception
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
