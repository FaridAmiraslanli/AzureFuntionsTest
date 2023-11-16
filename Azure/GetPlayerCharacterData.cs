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
        private const string DamageKey = "Damage";
        private const string HealthKey = "Health";
        private const string SpeedKey = "Speed";

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
                   DamageKey,HealthKey,SpeedKey
                }
            };

            try
            {
                var getCharacterDataResult = await serverApi.GetCharacterDataAsync(getCharacterDataRequest);
                
                return $"Damage = {getCharacterDataResult.Result.Data[DamageKey].Value} / Health = {getCharacterDataResult.Result.Data[HealthKey].Value} / Speed = {getCharacterDataResult.Result.Data[SpeedKey].Value}";
                // if (getCharacterDataResult.Result.Data.ContainsKey("YourCharacterDataKey"))
                // {
                //     var characterDataValue = getCharacterDataResult.Result.Data["YourCharacterDataKey"].Value;
                //     return new
                //     {
                //         success = true,
                //         characterData = characterDataValue
                //     };
                // }
                // else
                // {
                //     return new
                //     {
                //         success = false,
                //         error = "Character data key not found"
                //     };
                // }
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
