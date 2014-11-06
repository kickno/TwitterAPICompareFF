using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TweetSharp;
using TweetSharp.Model;

namespace test
{
    public partial class TweetSharpTest : System.Web.UI.Page
    {
        private string consumer_key = ConfigurationManager.AppSettings["consumer_key"];
        private string consumer_secret = ConfigurationManager.AppSettings["consumer_secret"];
        protected OAuthRequestToken reqToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            string from = Request.QueryString["from"];
           // string uniqueID = Request.QueryString["uniqueID"];
            string error = Request.QueryString["Error"];
            if (error != null)
            {
                if (error == "The_Same_Account")
                {
                    divError.InnerText = "Did you send request to your self?";
                }
                else if (error == "No_Token")
                {
                     divError.InnerText = "No access token";
                }
                else if (error == "Wrong_FROM")
                {
                    divError.InnerText = "From query is too long";
                }
                else
                { }
            }
            else
            {
                if (!String.IsNullOrEmpty(from))// && !String.IsNullOrEmpty(uniqueID))
                {
                    if (from.Count() <= 15)  //TODO: more validation for from and uniqueID
                    {
                        Session["from"] = from;
                        //Session["uniqueID"] = uniqueID;
                    }
                    else
                    {
                        Response.Redirect("http://riot.jp?Error=Wrong_FROM");
                    }
                }

                //1. Create an instance Twitter service with the Consumer Key and the Consume Secret keys.
                TwitterService service = new TwitterService(consumer_key, consumer_secret);

                // 2. Get the OAuthRequestToken based on the Consumer key and secret.
                reqToken = service.GetRequestToken();

                // TODO: Save the Token To compare with the querystring Cal

                //3. Get the Authorization URI using the request token.             
                string url = service.GetAuthorizationUri(reqToken).ToString();

                Response.Redirect(url);
                // Call back is set to CallBack_SendReqToB.aspx.

            }
        }
    }
}