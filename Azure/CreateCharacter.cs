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
        public static string CharacterID;

        [FunctionName("CreateCharacter")]
        public static async Task<dynamic> CreateCC(
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
            try
            {
                var grantCharactertoUserResult = await serverApi.GrantCharacterToUserAsync(grantCharacterToUserRequest);
                CharacterID = grantCharactertoUserResult.Result.CharacterId;
                await UpdateCC(req, log);


                CreateCharacterResultData resultData = new CreateCharacterResultData
                {
                    ResponseType = "CreateCharacter",
                    CharacterId = CharacterID
                };

                string json = JsonConvert.SerializeObject(resultData);

                if (grantCharactertoUserResult.Error == null)
                {
                    return new
                    {
                        success = true,
                        code = 200,
                        message = "Request successful",
                        data = grantCharactertoUserResult.Result
                    };

                } else{
                    int statusCodeForGrant = grantCharactertoUserResult.Error.HttpCode;
                    return new
                    {
                        success = false,
                        code = statusCodeForGrant,
                        message = "Request failed",
                        data = grantCharactertoUserResult.Result
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
            catch (Exception ex)
            {
                log.LogError($"Error while ctreating character: {ex.Message}");
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
        }

        //*************************************************************************************************************

        [FunctionName("UpdateCharacterDataServer")]
        public static async Task<dynamic> UpdateCC(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var args = context.FunctionArgument;

            string titleId = args["TitleId"];
            string playFabId = args["PlayFabId"];
            string characterId = CharacterID;

            int leftGunType = 1; // Medium Range
            int rightGunType = 1;

            float nitroValue = 1.7f;

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

            Engine engine = new Engine
            {
                Acceleration = 3.7f,
                MaxSpeed = 5.9f
            };

            string engineJsonData = JsonConvert.SerializeObject(engine);

            Steering steering = new Steering
            {
                Acceleration = 0.5f,
                MaxRotation = 52f
            };

            string steeringJsonData = JsonConvert.SerializeObject(steering);

            var updateCharacterDataRequest = new UpdateCharacterDataRequest
            {
                PlayFabId = playFabId,
                CharacterId = characterId,
                Data = new Dictionary<string, string>()
                    {
                        {DataKeys.LeftGunKey, $"{leftGunType}"},
                        {DataKeys.RightGunKey, $"{rightGunType}"},
                        {DataKeys.NitroKey, $"{nitroValue}"},
                        {DataKeys.EngineKey, engineJsonData},
                        {DataKeys.SteeringKey, steeringJsonData},
                    }
            };

            try
            {
                var updateCharacterDataResult = await serverApi.UpdateCharacterDataAsync(updateCharacterDataRequest);

                return new
                {
                    success = true,
                    code = 200,
                    message = "Request successful",
                    data = updateCharacterDataResult.Result
                };
            }
            catch (PlayFabException ex)
            {
                log.LogError($"Error getting character data: {ex.Message}");
                return new
                {
                    success = false,
                    message = ex.Message
                };
            }
            catch (Exception ex)
            {
                log.LogError($"Error while updating character: {ex.Message}");
                return new
                {
                    success = false,
                    message = ex.Message
                };
            }
        }
    }


    [Serializable]
    public class CreateCharacterResultData
    {
        public string ResponseType;

        public string CharacterId;
    }
}
