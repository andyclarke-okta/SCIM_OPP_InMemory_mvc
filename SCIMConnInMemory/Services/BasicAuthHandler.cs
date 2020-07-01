using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OktaSCIMConn.Services
{
    public class BasicAuthHandler : DelegatingHandler
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string BasicRealm { get; set; }
        protected string userName { get; set; }
        protected string passWord { get; set; }

        OktaAPI oktaApi = null;

        //public BasicAuthHandler(string username, string password)
        //{
        //    this.userName = username;
        //    this.passWord = password;
        //}
        public BasicAuthHandler()
        {
            logger.Debug("Enter BasicAuthHandler");
            oktaApi = new OktaAPI();
        }

        private const string SCHEME = "Basic";
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                               System.Threading.CancellationToken
                                                                                   cancellationToken)
        {
            try
            {
                // Request Processing
                var headers = request.Headers;
                if (headers.Authorization != null && SCHEME.Equals(headers.Authorization.Scheme))
                {

                    bool validateRsp = ValidateCredentials(headers.Authorization);
                    if (validateRsp)
                    {
                        var claims = new List<System.Security.Claims.Claim>
                            {
                                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, userName),
                                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.AuthenticationMethod, System.IdentityModel.Tokens.AuthenticationMethods.Password)
                            };
                        var principal = new System.Security.Claims.ClaimsPrincipal(new[] { new System.Security.Claims.ClaimsIdentity(claims, SCHEME) });
                        Thread.CurrentPrincipal = principal;
                        if (HttpContext.Current != null)
                            HttpContext.Current.User = principal;
                    }

                }

                var response = await base.SendAsync(request, cancellationToken);
                // Response processing
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(SCHEME));
                }
                return response;
            }
            catch (Exception)
            {
                // Error processing
                var response = request.CreateResponse(HttpStatusCode.Unauthorized);
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(SCHEME));
                return response;
            }
        }


        //Method to validate credentials from Authorization Basic Header
        private bool ValidateCredentials(AuthenticationHeaderValue authenticationHeaderVal)
        {
            try
            {
                if (authenticationHeaderVal != null && !String.IsNullOrEmpty(authenticationHeaderVal.Parameter))
                {

                    System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
                    string credentials = encoding.GetString(Convert.FromBase64String(authenticationHeaderVal.Parameter));
                    string[] parts = credentials.Split(':');
                    userName = parts[0].Trim();
                    passWord = parts[1].Trim();

                    bool authRsp = false;
                    authRsp = oktaApi.AuthenticateUser(userName, passWord);
                    return authRsp;
                }
                return false;//request not authenticated.
            }
            catch (Exception e)
            {
                logger.Error("Error authenticating User " + e.ToString());
                return false;
            }
        }
    }
}