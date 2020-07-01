using log4net;
using Okta.Core;
using Okta.Core.Clients;
using Okta.Core.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace OktaSCIMConn.Services
{

    public class OktaAPI
    {

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        NameValueCollection appSettings = ConfigurationManager.AppSettings;
        //private AuthClient _authclient = new AuthClient(appSettings["okta.baseURL"]);
        public bool AuthenticateUser(string userName, string passWord)
        {
            //declare local variables
            string myStatus = null;
            string myStateToken;
            string mySessionToken;

            UserAuthnRequest userAuthnRequest = new UserAuthnRequest();
            userAuthnRequest.username = userName;
            userAuthnRequest.password = passWord;

            try
            {
                var client = new RestClient(appSettings["okta.baseURL"] + "/api/v1/authn");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(userAuthnRequest);
                IRestResponse<userAuthnResponse> response = client.Execute<userAuthnResponse>(request);

                myStatus = response.Data.status;
                mySessionToken = response.Data.sessionToken;
                myStateToken = response.Data.stateToken;
            }
            catch (OktaException ex)
            {
                if (ex.ErrorCode == "E0000004")
                {
                    logger.Error("Invalid Credentials for User: " + userName);
                    throw ex;
                }
                else if (ex.ErrorCode == "E0000085")
                {
                    logger.Error("Access Denied by Polciy for User: " + userName);
                    throw ex;
                }
                else
                {
                    logger.Error(userName + " = " + ex.ErrorCode + ":" + ex.ErrorSummary);
                    throw ex;
                }

            }//end catch

            switch (myStatus)
            {
                //ref http://developer.okta.com/docs/api/resources/authn.html#transaction-state

                case "PASSWORD_WARN":  //password about to expire
                    logger.Debug("User’s password was successfully validated but expires in ");
                    break;
                case "PASSWORD_EXPIRED":  //password has expired
                    logger.Debug("User’s password was successfully validated but is expired. ");
                    break;
                case "LOCKED_OUT":  //user account is locked, unlock required
                    logger.Debug("Your account has been locked");
                    break;
                case "MFA_ENROLL":   //user must select and enroll an available factor 
                    logger.Debug("user must select and enroll an available factor");
                    break;
                case "MFA_REQUIRED":    //user must provide second factor with previously enrolled factor
                    logger.Debug("user must provide second factor with previously enrolled factor");
                    break;
                case "SUCCESS":      //authentication is complete
                    logger.Debug(" Successful login");
                    return true;
                //break;
                default:
                    logger.Debug(" Unhandled Status: " + myStatus);

                    break;
            }//end of switch
            return false;
        }


        public class UserAuthnRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class UserValidateResponse
        {
            public string id { get; set; }
            public string userId { get; set; }
            public string login { get; set; }

            public string status { get; set; }

            public bool mfaActive { get; set; }
        }





        /// <summary>
        /// ///////////////////////////////////////////////////////////
        /// </summary>
        public class userAuthnResponse
        {
            public string stateToken { get; set; }
            public string sessionToken { get; set; }
            public DateTime expiresAt { get; set; }
            public string status { get; set; }
            public _Embedded _embedded { get; set; }
            public _Links1 _links { get; set; }
        }

        public class _Embedded
        {
            public User user { get; set; }
            public Factor[] factors { get; set; }
            public Policy policy { get; set; }
        }

        public class User
        {
            public string id { get; set; }
            public Profile profile { get; set; }
        }

        public class Profile
        {
            public string login { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string locale { get; set; }
            public string timeZone { get; set; }
        }

        public class Policy
        {
            public bool allowRememberDevice { get; set; }
            public int rememberDeviceLifetimeInMinutes { get; set; }
            public bool rememberDeviceByDefault { get; set; }
        }

        public class Factor
        {
            public string id { get; set; }
            public string factorType { get; set; }
            public string provider { get; set; }
            public string vendorName { get; set; }
            public Profile1 profile { get; set; }
            public _Links _links { get; set; }
        }

        public class Profile1
        {
            public string phoneNumber { get; set; }
        }

        public class _Links
        {
            public Verify verify { get; set; }
        }

        public class Verify
        {
            public string href { get; set; }
            public Hints hints { get; set; }
        }

        public class Hints
        {
            public string[] allow { get; set; }
        }

        public class _Links1
        {
            public Cancel cancel { get; set; }
        }

        public class Cancel
        {
            public string href { get; set; }
            public Hints1 hints { get; set; }
        }

        public class Hints1
        {
            public string[] allow { get; set; }
        }
    }


}
