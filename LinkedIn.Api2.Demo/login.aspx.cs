using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LinkedIn.Api2.Demo
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            var linkedInApi = new LinkedInApi(
                "YOUR_LINKEDIN_APP_ID",
                "YOUR_LINKEDIN_APP_SECRET",
                "http://localhost:6625/auth/linkedin/default.aspx",
                success: (ln) =>
                {
                    FormsAuthentication.RedirectFromLoginPage(ln.PersonName, true);
                });

            Session["LinkedInAuth"] = linkedInApi;

            Response.Redirect(
                linkedInApi.GetAuthorizationUrl(Request.Params["ReturnUrl"]),
                true);
        }
    }
}