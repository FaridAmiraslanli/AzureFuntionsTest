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
    public static class GetInGamePlayerDatas
    {
        private const string CreditKey = "CreditCount";

        [FunctionName("GetInGamePlayerDatas")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string titleId = args["titleId"];
            string entityToken = args["entityToken"];

            var player1PlayfabId = args["player1PlayfabId"];
            var player2PlayfabId = args["player2PlayfabId"];
            var player3PlayfabId = args["player3PlayfabId"];

            List<object> playerDataValues = new List<object>();

            string[] playerIds = { player1PlayfabId, player2PlayfabId, player3PlayfabId };

            var settings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = entityToken
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);

            for (int i = 0; i < playerIds.Length; i++)
            {
                var getUserDataRequest = new GetUserDataRequest
                {
                    PlayFabId = playerIds[i],
                    Keys = new List<string>
                    {
                        CreditKey,
                    }
                };
                var getUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);
                if (!getUserDataResult.Result.Data.ContainsKey(CreditKey)) // that means the player is a new user. need to add data keys
                {
                    var updateUserDataRequest = new UpdateUserDataRequest
                    {
                        PlayFabId = playerIds[i],

                        Data = new Dictionary<string, string>()
                    {
                        {CreditKey, "0"}
                    }
                    };
                    await serverApi.UpdateUserDataAsync(updateUserDataRequest);

                    var getUpdatedUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);
                    playerDataValues.Add(getUpdatedUserDataResult.Result);
                }
                else
                {
                    playerDataValues.Add(getUserDataResult.Result);
                }
            }

            return new
            {
                success = true,
                code = 200,
                message = "Request successful",
                data = new {
                    playerData = playerDataValues
                }
            };
            }
            catch (PlayFabException ex)
            {
                log.LogError($"Playfab error while getting in game player data: {ex.Message}");
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
            catch (Exception ex)
            {
                log.LogError($"Error while getting in game player data: {ex.Message}");
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
    }
}
