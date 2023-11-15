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
    public static class GetUserData
    {
        private const string titleId = "F4A07";
        private const string playFabId = "6B53F263D81D61F5";
        private const string entityToken  = "NHxEdlV4YStsUVZyR0s1VS9mclJ2bWZwZ2dTclpjV1lPbmpEYlU4cVJZcTQ4PXx7ImkiOiIyMDIzLTExLTE0VDE2OjEwOjAwWiIsImlkcCI6IkN1c3RvbSIsImUiOiIyMDIzLTExLTE1VDE2OjEwOjAwWiIsImZpIjoiMjAyMy0xMS0xNFQxNjoxMDowMFoiLCJ0aWQiOiJWYkVSVTRwWFppQSIsImlkaSI6IjE4RDc2OTU1MjRBOUZFREMiLCJoIjoiQjRGNzQyMjZBOTU3OTM4QyIsImVjIjoidGl0bGVfcGxheWVyX2FjY291bnQhMzVBNzUxMTM1Njc5MTlDRS9GNEEwNy82QjUzRjI2M0Q4MUQ2MUY1L0MwQ0Q2NTk2Q0ZCMjk1MkUvIiwiZWkiOiJDMENENjU5NkNGQjI5NTJFIiwiZXQiOiJ0aXRsZV9wbGF5ZXJfYWNjb3VudCJ9";
        private const string CreditKey  = "alma";
        // private const string TokenKey = "TokenCount";
        // private const string Tier1Key = "Tier1";
        // private const string Tier2Key = "Tier2";
        // private const string Tier3Key = "Tier3";

        [FunctionName("GetUserData")]
        public static async Task<dynamic> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;
           

            var getUserDataRequest = new GetUserDataRequest
            {
                PlayFabId = playFabId /* context.CallerEntityProfile.Lineage.MasterPlayerAccountId */,
                Keys = new List<string>
                    {
                        CreditKey/* ,TokenKey,Tier1Key,Tier2Key,Tier3Key */
                    }
            };

            var settings = new PlayFabApiSettings
            {
                TitleId = titleId/* context.TitleAuthenticationContext.Id */,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = entityToken/* context.TitleAuthenticationContext.EntityToken */
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);
            var getUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);

            if (!getUserDataResult.Result.Data.ContainsKey(CreditKey)) // that means the player is a new user. need to add data keys
            {
                var updateUserDataRequest = new UpdateUserDataRequest
                {
                    PlayFabId = playFabId /* context.CallerEntityProfile.Lineage.MasterPlayerAccountId */,

                    Data = new Dictionary<string, string>()
                    {
                        {CreditKey, "0"},
                       /*  {TokenKey, "0"},
                        {Tier1Key, "0"},
                        {Tier2Key, "0"},
                        {Tier3Key, "0"}, */
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