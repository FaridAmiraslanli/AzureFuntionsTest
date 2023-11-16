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
    public static class CreateCharacter
    {
        [FunctionName("CreateCharacter")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
           FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            var grantCharacterToUserRequest = new GrantCharacterToUserRequest
            {
                CharacterName = args["CharacterName"],
                CharacterType = args["CharacterType"],
                PlayFabId = args["PlayFabId"]
            };

            var settings = new PlayFabApiSettings
            {
                TitleId = args["TitleId"],
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = args["EntityToken"]
            };

            var serverApi = new PlayFabServerInstanceAPI(settings, authContext);
            var grantCharactertoUserResult = await serverApi.GrantCharacterToUserAsync(grantCharacterToUserRequest);
            

            return grantCharactertoUserResult.Result.CharacterId;
        }
    }
}
