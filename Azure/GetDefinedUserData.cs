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
    public static class GetDefinedUserData
    {
        private const string CreditKey = "CreditCount";

        [FunctionName("GetDefinedUserData")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string titleId = args["titleId"];
            string entityToken = args["entityToken"];

            var getUserDataRequest = new GetUserDataRequest
            {
                PlayFabId = args["player1PlayfabId"],
                Keys = new List<string>
                    {
                        CreditKey,
                    }
            };

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
            var getUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);

            if (!getUserDataResult.Result.Data.ContainsKey(CreditKey)) // that means the player is a new user. need to add data keys
            {
                var updateUserDataRequest = new UpdateUserDataRequest
                {
                    PlayFabId = args["player1PlayfabId"],

                    Data = new Dictionary<string, string>()
                    {
                        {CreditKey, "0"}
                    }
                };
                await serverApi.UpdateUserDataAsync(updateUserDataRequest);

                var getUpdatedUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);
                return getUpdatedUserDataResult.Result.Data;
            }

            return getUserDataResult.Result.Data;
        }
    }
}
