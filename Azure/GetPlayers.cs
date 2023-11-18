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
        [FunctionName("GetUserData")]
        public static async Task<dynamic> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            try
            {
                FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

                var args = context.FunctionArgument;

                string titleId = args["TitleId"];
                string playFabId = args["PlayFabId"];
                string entityToken = args["EntityToken"];
                string creditKey = args["CreditKey"];

                var getUserDataRequest = new GetUserDataRequest
                {
                    PlayFabId = playFabId,
                    Keys = new List<string>
                    {
                        creditKey
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

                if (!getUserDataResult.Result.Data.ContainsKey(creditKey)) 
                {
                    var updateUserDataRequest = new UpdateUserDataRequest
                    {
                        PlayFabId = playFabId,

                        Data = new Dictionary<string, string>()
                    {
                        {creditKey, "0"},
                       /*  {TokenKey, "0"},
                        {Tier1Key, "0"},
                        {Tier2Key, "0"},
                        {Tier3Key, "0"}, */
                    }
                    };
                    await serverApi.UpdateUserDataAsync(updateUserDataRequest);

                    var getUpdatedUserDataResult = await serverApi.GetUserDataAsync(getUserDataRequest);
                    // return getUpdatedUserDataResult.Result.Data;
                }

                if (getUserDataResult.Error == null)
                {
                    return new
                    {
                        success = true,
                        code = 200,
                        message = "Request Successful",
                        data = getUserDataResult.Result.Data
                    };
                }
                else
                {
                    int httpCodeForGetPlayer = getUserDataResult.Error.HttpCode;
                    return new
                    {
                        success = false,
                        code = httpCodeForGetPlayer,
                        message = "Bad Request",
                        data = getUserDataResult.Result.Data
                    };
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception " + ex.ToString());
                return new BadRequestObjectResult(ex);
            }
        }
    }
}