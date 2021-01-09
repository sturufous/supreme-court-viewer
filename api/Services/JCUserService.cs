using System;
using System.Threading.Tasks;
using JCCommon.Clients.UserService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Models.JCUserService;

namespace Scv.Api.Services
{
    public class JCUserService
    {
        #region Variables
        private UserServiceClient UserServiceClient { get; }
        private ILogger<JCUserService> Logger { get; }
        #endregion Variables

        #region Constructor
        public JCUserService(UserServiceClient userServiceClient, ILogger<JCUserService> logger)
        {
            UserServiceClient = userServiceClient;
            UserServiceClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            Logger = logger;
        }
        #endregion

        public async Task<GetUserLoginResponseType> GetUserInfo(UserInfoRequest userInfoRequest)
        {
            try 
            {
                var response = await UserServiceClient.UserGetuserloginAsync(userInfoRequest.DomainName,
                    userInfoRequest.DomainUserGuid,
                    userInfoRequest.DomainUserId,
                    userInfoRequest.DeviceName,
                    userInfoRequest.IpAddress,
                    userInfoRequest.TemporaryAccessGuid);

                if (response.ResponseCd != "1") return response;
                Logger.LogDebug("Returned response 1 (failed) from getUserLogin");
                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                return null;
            }
        }
    }
}
