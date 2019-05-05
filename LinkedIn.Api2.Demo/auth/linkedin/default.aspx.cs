﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LinkedIn.Api2.Demo.auth.linkedin
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var linkedInApi = Session["LinkedInAuth"] as LinkedInApi;

            if (linkedInApi != null)
                linkedInApi.FinalizeCallback(Request.Params);
        }
    }
}