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
using PlayFab;
using PlayFab.AdminModels;
using System.Collections.Generic;

namespace DynamicBox.CloudScripts
{
    public static class VirtualCurrencyModule
    {
        [FunctionName("AddUserVirtualCurrency")]
        public static async Task<dynamic> AddUserVirtualCurrency(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            
            var args =context.FunctionArgument;
            string titleId = args["TitleId"];
            string playFabId = args["PlayFabId"];

            #region PlayfabServerApiSettings

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

            #endregion
            
            PlayFab.ServerModels.AddUserVirtualCurrencyRequest addUserVirtualCurrencyRequest = new PlayFab.ServerModels.AddUserVirtualCurrencyRequest
            {
                Amount = args["Amount"],
                PlayFabId = playFabId,
                VirtualCurrency = args["VirtualCurrency"]
            };

            var addUserVirtualCurrencyResult = await serverApi.AddUserVirtualCurrencyAsync (addUserVirtualCurrencyRequest);

            return  addUserVirtualCurrencyResult;
            // return new
            // {
            //     success = true,
            //     code = addUserVirtualCurrencyResult.Error.HttpStatus,
            //     message = "Currency Added Successfully",
            //     data = new
            //     {
            //         playfabId = addUserVirtualCurrencyResult.Result.PlayFabId,
            //         virtualCurrency = addUserVirtualCurrencyResult.Result.VirtualCurrency,
            //         balance = addUserVirtualCurrencyResult.Result.Balance,
            //         balanceChange = addUserVirtualCurrencyResult.Result.BalanceChange,
            //     }
            // };
        }

//*****************************************************************************************************************
         [FunctionName("CreateVirtualCurrency")]
        public static async Task<dynamic> CreateVirtualCurrency(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            
            var args = context.FunctionArgument;
            string titleId = args["TitleId"];

            #region PlayfabAdminApiSettings

             var settings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY", EnvironmentVariableTarget.Process)
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityToken = args["EntityToken"]
            };

            var serverApi = new PlayFabAdminInstanceAPI(settings, authContext);

            #endregion
            
            VirtualCurrencyData virtualCurrency = new VirtualCurrencyData
            {
                DisplayName = args["DisplayName"],
                InitialDeposit = args["InitialDeposit"],
                CurrencyCode = args["CurrencyCode"]
            };
            List<VirtualCurrencyData> virtualCurrencies = new List<VirtualCurrencyData>
            {
                virtualCurrency
            };

            AddVirtualCurrencyTypesRequest addVirtualCurrencyTypesRequest = new AddVirtualCurrencyTypesRequest
            {
                VirtualCurrencies = virtualCurrencies
            };

            var AddVirtualCurrencyTypesResult = await serverApi.AddVirtualCurrencyTypesAsync (addVirtualCurrencyTypesRequest);

            return  AddVirtualCurrencyTypesResult;
            // return new
            // {
            //     success = true,
            //     code = addUserVirtualCurrencyResult.Error.HttpStatus,
            //     message = "Currency Added Successfully",
            //     data = new
            //     {
            //         playfabId = addUserVirtualCurrencyResult.Result.PlayFabId,
            //         virtualCurrency = addUserVirtualCurrencyResult.Result.VirtualCurrency,
            //         balance = addUserVirtualCurrencyResult.Result.Balance,
            //         balanceChange = addUserVirtualCurrencyResult.Result.BalanceChange,
            //     }
            // };
        }
    }
}
