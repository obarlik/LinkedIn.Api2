using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace LinkedIn.Api2
{
    public class LinkedInAuth
    {
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string CallbackUrl { get; }

        string CallbackUrl2;

        public string AccessToken { get; private set; }

        public string PersonName { get; private set; }
        public string PersonSurname { get; private set; }
        public string PersonEmail { get; private set; }
        public string PersonImage { get; private set; }

        public string StateId { get; private set; }
        public string AuthorizationCode { get; private set; }


        public LinkedInAuth(string clientId, string clientSecret, string callbackUrl)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            CallbackUrl = callbackUrl;
            
            StateId = Guid.NewGuid().ToString().Replace("-", "");
        }


        public string GetAuthorizationUrl(string returnUrl)
        {
            var cbUri = new UriBuilder(CallbackUrl);

            cbUri.Query += 
                string.IsNullOrWhiteSpace(returnUrl) ? "" :
                    ((string.IsNullOrWhiteSpace(cbUri.Query) ? "" : "&")
                   + $"ReturnUrl={Uri.EscapeDataString(returnUrl)}");

            CallbackUrl2 = cbUri.Uri.AbsoluteUri;

            return "https://www.linkedin.com/oauth/v2/authorization" +
                    "?response_type=code" +
                   $"&client_id={Uri.EscapeDataString(ClientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(CallbackUrl2)}" +
                   $"&state={StateId}" +
                    "&scope=r_liteprofile%20r_emailaddress";
        }


        public void Callback(
            string state, string code, 
            string error, string error_description,
            Action success,
            Action<string, string> failure)
        {
            if (state != StateId)
                failure("Error", "Invalid state!");

            if (!string.IsNullOrWhiteSpace(error))
                failure(error, error_description);

            AuthorizationCode = code;

            var cli = new WebClient();

            cli.Headers["Content-Type"] = "application/x-www-form-urlencoded";

            var json = cli.UploadString(
                "https://www.linkedin.com/oauth/v2/accessToken",
                $"client_id={ClientId}" +
                $"&client_secret={ClientSecret}" +
                $"&grant_type=authorization_code" +
                $"&redirect_uri={Uri.EscapeDataString(CallbackUrl2)}" +
                $"&code={AuthorizationCode}");
            
            AccessToken = Regex.Matches(json, "\"access_token\" *: *\"(.+?)\"")
                .OfType<Match>()
                .Select(m => m.Groups[1].Value)
                .FirstOrDefault();

            if (AccessToken != null)
                success();
            else
                failure("TokenError", "Token retrieval failure!");
        }


        public WebClient NewClient()
        {
            var cli = new WebClient();

            cli.Headers["Authorization"] = "Bearer " + AccessToken;
            cli.Encoding = Encoding.UTF8;
            
            return cli;
        }


        public string GetProfileJson()
        {
            return NewClient().DownloadString(
                "https://api.linkedin.com/v2/me" +
                "?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))");
        }


        public string GetEmailJson()
        {
            return NewClient().DownloadString(
                "https://api.linkedin.com/v2/emailAddress" +
                "?q=members&projection=(elements*(handle~))");
        }            
    }
}
