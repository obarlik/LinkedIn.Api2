using LinkedIn.Api2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LinkedIn.Api2
{
    public class LinkedInApi
    {
        public string PersonId { get; protected set; }
        public string PersonName { get; protected set; }
        public string PersonEmail { get; protected set; }
        public string PersonPhotoUrl { get; protected set; }

        public LinkedInAuth Api { get; }        

        Action AuthSuccess { get; }
        Action<string, string> AuthFailure { get; }

     
        public LinkedInApi(
            string clientId,
            string clientSecret,
            string callbackUrl,
            Action<LinkedInApi> success, 
            Action<LinkedInApi, string, string> failure = null)
        {
            Api = new LinkedInAuth(clientId, clientSecret, callbackUrl);

            AuthSuccess = () =>
            {
                var profileJson = JObject.Parse(Api.GetProfileJson());
                var emailJson = JObject.Parse(Api.GetEmailJson());

                PersonId = profileJson["id"].Value<string>();

                PersonName =
                    profileJson["firstName"]?["localized"].First.First.Value<string>()
                    + " " + profileJson["lastName"]?["localized"].First.First.Value<string>();

                PersonEmail =
                    emailJson["elements"].First["handle~"]?["emailAddress"].Value<string>();

                PersonPhotoUrl =
                    profileJson["profilePicture"]?["displayImage~"]?["elements"]
                    .Select(el =>
                        new
                        {
                            width = el["data"]?["com.linkedin.digitalmedia.mediaartifact.StillImage"]?["storageSize"]?["width"].Value<int>(),
                            url = el["identifiers"].First["identifier"].Value<string>()
                        })
                        .OrderByDescending(el => el.width)
                        .Select(el => el.url)
                        .FirstOrDefault();
                
                success(this);
            };

            if (failure != null)
                AuthFailure = (error, description) => failure(this, error, description);
        }

       
        public string GetAuthorizationUrl(string returnUrl)
        {
            return Api.GetAuthorizationUrl(returnUrl);
        }
        

        public void FinalizeCallback(NameValueCollection parameters)
        {
            if (parameters["state"] == null)
                throw new Exception("Invalid state value read from parameters!");

            Api.Callback(
                parameters["state"],
                parameters["code"],
                parameters["error"],
                parameters["error_description"],
                success: () =>
                {
                    AuthSuccess();
                },
                failure: (error, description) =>
                {
                    if (error != "user_cancelled_login"
                     && error != "user_cancelled_authorize")
                    {
                        if (AuthFailure == null)
                        {
                            throw new LinkedInError(
                                $"Error: {error}\n" +
                                $"Description {description}");
                        }
                        else
                        {
                            AuthFailure(error, description);
                        }
                    }
                });
        }
    }
}