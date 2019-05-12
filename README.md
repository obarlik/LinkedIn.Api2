# LinkedIn.Api2
Linkedin API v2 Authentication 


## Usage

```csharp

// LinkedIn login button click event
protected void LinkButton1_Click(object sender, EventArgs e)
{
    var linkedInApi = new LinkedInApi(
        "YOUR_LINKEDIN_APP_ID",
        "YOUR_LINKEDIN_APP_SECRET",
        "http://localhost:6625/auth/linkedin/default.aspx", // OAUTH Callback Url
        success: (ln) =>
        {
            FormsAuthentication.RedirectFromLoginPage(ln.PersonEmail, true);
        });

    Session["LinkedInAuth"] = linkedInApi;

    Response.Redirect(
        linkedInApi.GetAuthorizationUrl(Request.Params["ReturnUrl"]),
        true);
}


// ~/auth/linkedin/default.aspx.cs
// Page load event
protected void Page_Load(object sender, EventArgs e)
{
    var linkedInApi = Session["LinkedInAuth"] as LinkedInApi;

    if (linkedInApi != null)
        linkedInApi.FinalizeCallback(Request.Params);
}


```
 
