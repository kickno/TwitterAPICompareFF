using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using TweetSharp;

namespace test
{
    public partial class CalledBack_SendReqToB : System.Web.UI.Page
    {
        #region Private Members
        //private string uniqueID;
        private string oauth_token;
        private string oauth_verifier;
        TwitterService service;
        OAuthAccessToken accToken = new OAuthAccessToken();
        private string consumer_key = ConfigurationManager.AppSettings["consumer_key"];
        private string consumer_secret = ConfigurationManager.AppSettings["consumer_secret"];
        List<TwitterUser> listFL;
        List<TwitterUser> listFR;
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {

            string userFrom;
            userA.Visible = true;
            userB.Visible = true;
            #region Twitter Service Authentication

            service = new TwitterService(consumer_key, consumer_secret);
            oauth_token = Request.QueryString["oauth_token"];
            oauth_verifier = Request.QueryString["oauth_verifier"];

            // Check query string null -> if Landing here without oauth and varifire, go back to top
            if (String.IsNullOrEmpty(oauth_token) || String.IsNullOrEmpty(oauth_verifier))
                Response.Redirect("http://kickno.riot.jp?Error=No_Token");

            // TODO: Match the oauth token saved in TweetSharpTest (1st page)
            OAuthRequestToken req = new OAuthRequestToken { Token = oauth_token };

            accToken = service.GetAccessToken(req, oauth_verifier);
            service.AuthenticateWith(accToken.Token, accToken.TokenSecret);

            #endregion

            if (service.Response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (string.IsNullOrEmpty(Session["from"] as string) /*&& Session["uniqueID"] == null*/) //This is RequestING user.
                {
                    userB.Visible = false;
                }
                else
                {   //This is RequestED user.
                    ListAndSave(false);
                    userFrom = Session["from"].ToString();
                    //resUniqueID = Session["uniqueID"].ToString();
                    Session.Clear();
                    if (divError.InnerText != "")
                    {
                        return;
                    }
                    if (userFrom == accToken.ScreenName)
                    {
                        Response.Redirect("http://kickno.riot.jp/?Error=The_Same_Account");
                    }
                    else
                    {
                        userA.Visible = false;
                        XmlDocument docRequester = new XmlDocument();
                        docRequester = FindRequesterData(userFrom/*, resUniqueID*/);
                        if (docRequester.InnerXml != "")
                        {
                            CreateChart(docRequester, listFL, listFR);
                            lblrequester.InnerText = String.Concat("You have Common Factors with ", userFrom);
                        }
                        else
                        {
                            divError.InnerText = "Could Not find the requester's information";
                            userA.Visible = false;
                        }
                    }

                }

            }
            else
            {
                divError.InnerText = String.Concat("Authentication failed :", service.Response.Error.Message);
            }

        }
        private void ListAndSave(bool requesting)
        {
            divError.InnerText = "";

            //If the File found, and it is within 15min, use that, instead of doing the following.
            string path = HostingEnvironment.MapPath(String.Concat("~/data/", accToken.ScreenName, ".xml"));
            bool hasFreshFile = File.Exists(path) && File.GetLastWriteTime(path).AddMinutes(15) > DateTime.Now;

            if (!hasFreshFile)
            {
                ListFollowersOptions followerOp = new ListFollowersOptions { UserId = accToken.UserId, Cursor = -1 };
                var listFollower = service.ListFollowers(followerOp);

                if (listFollower != null)
                {
                    listFL = GetAllData(listFollower, followerOp);
                }
                else
                {
                    divError.InnerText = String.Concat("Error from Follower: ", service.Response.Error.Message);
                    userA.Visible = false;
                }

                ListFriendsOptions friendsOp = new ListFriendsOptions { UserId = accToken.UserId, Cursor = -1 };

                var listFriends = service.ListFriends(friendsOp);
                if (listFriends != null)
                {
                    listFR = GetAllData(listFriends, friendsOp);
                }
                else
                {
                    divError.InnerText = String.Concat("Error from Friend: ", service.Response.Error.Message);
                    userA.Visible = false;
                }

                if (divError.InnerText == "")
                {
                    SaveToXML(listFL, listFR);
                }
                else
                {
                    userA.Visible = false;
                    
                }
            }
            else //If File existed, and file creation time + 15 > Now (have new file)  Requesting && hasFreshFile.
            {
                if (!requesting)
                {
                    //Read xml and feed to list FL and list FR. Don't save.
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(path);
                    XmlNodeList FL = xdoc.SelectNodes("User/Follower").Item(0).ChildNodes;
                    XmlNodeList FR = xdoc.SelectNodes("User/Friend").Item(0).ChildNodes;
                    listFL = new List<TwitterUser>();
                    listFR = new List<TwitterUser>();
                    foreach (XmlNode f in FL)
                    {
                        listFL.Add(new TwitterUser { ScreenName = f.InnerText });
                    }
                    foreach (XmlNode f in FR)
                    {
                        listFR.Add(new TwitterUser { ScreenName = f.InnerText });
                    }
                }

            }

        }
        //create userB and saved userA
        private void CreateChart(XmlDocument docRequester, List<TwitterUser> listFL, List<TwitterUser> listFR)
        {
            List<string> matchedFL = new List<string>();
            List<string> matchedFR = new List<string>();
            XmlNodeList requesterFL = docRequester.SelectNodes("User/Follower").Item(0).ChildNodes;
            XmlNodeList requesterFR = docRequester.SelectNodes("User/Friend").Item(0).ChildNodes;

            // Combine the two. the both are doing the same thing.
            if (requesterFL != null)
            {
                foreach (XmlNode reqfl in requesterFL)
                {
                    foreach (TwitterUser myfl in listFL)
                    {
                        if (myfl.ScreenName == reqfl.InnerText)
                            matchedFL.Add(reqfl.InnerText);
                    }
                }

            }
            if (requesterFR != null)
            {
                foreach (XmlNode reqfr in requesterFR)
                {
                    foreach (TwitterUser myfr in listFR)
                    {
                        if (myfr.ScreenName == reqfr.InnerText)
                            matchedFR.Add(reqfr.InnerText);
                    }
                }
            }
            //TODO: This can be enhanced to location match, retweet mache etc. Though not enough time.
            lblflstat.InnerText = String.Concat(matchedFL.Count.ToString(), " out of ", listFL.Count.ToString());
            lblfrstat.InnerText = String.Concat(matchedFR.Count.ToString(), " out of ", listFR.Count.ToString());
            ShowMatched(matchedFL, matchedFR);
        }

        private void ShowMatched(List<string> matchedFL, List<string> matchedFR)
        {
            dlFollower.DataSource = matchedFL;
            dlFollower.DataBind();
            dlFriend.DataSource = matchedFR;
            dlFriend.DataBind();
            lblflstat.Visible = true;
            lblfrstat.Visible = true;
            //TODO: When XML files are disposed? 
        }
        /// <summary>
        /// Look for a {userFrom}.xml file under App_Data, and load to xml then return. 
        /// </summary>
        /// <param name="userFrom"></param>
        /// <returns></returns>
        private XmlDocument FindRequesterData(string userFrom/*, string resUniqueID*/)
        {
            XmlDocument foundxml = new XmlDocument();
            string path = HostingEnvironment.MapPath(String.Concat("~/data/", userFrom, ".xml"));
            if (File.Exists(path))
            {
                foundxml.Load(path);
                //XmlNode xnd = foundxml.SelectSingleNode("User");
                //if (xnd.Attributes["ID"].Value.ToString() != resUniqueID)
                //{
                //    // This is not what you want to return.  TODO: Return some message
                //    foundxml = null;
                //}
            }
            return foundxml;
        }
        //private string FindRequesterID()
        //{
        //    string retstr = "";
        //    XmlDocument foundxml = new XmlDocument();
        //    string path = HostingEnvironment.MapPath(String.Concat("~/data/", accToken.ScreenName, ".xml"));
        //    if (File.Exists(path))
        //    {
        //        foundxml.Load(path);
        //        //If the uniqueID
        //        XmlNode xnd = foundxml.SelectSingleNode("User");
        //        retstr = xnd.Attributes["ID"].Value.ToString();
        //    }
        //    return retstr;
        //}

        #region Private Methods, GetAlldata and SaveToXML
        // TODO: Combine the two overloaded GetAllData function to one. It's just FrinesOption and FollowerOption diff.
        private List<TwitterUser> GetAllData(TwitterCursorList<TwitterUser> sender, ListFollowersOptions options)
        {
            int countCalls = 0;
            List<TwitterUser> Luser = new List<TwitterUser>();
            if (sender == null)
            {
                divError.InnerText = String.Concat("Error: Friend ", service.Response.Error.Message);
                userA.Visible = false;
            }
            else
            {
                while (sender.NextCursor != null)
                {
                    // if the API call did not succeed
                    if (sender == null)
                    {
                        divError.InnerText = service.Response.Error.Message;
                        userA.Visible = false;
                    }
                    else
                    {
                        foreach (TwitterUser user in sender)
                        {
                            Luser.Add(user);
                        }
                    }
                    //If more cursor, then go to next page
                    if (sender.NextCursor != 0 && sender.NextCursor != null)
                    {
                        options.Cursor = sender.NextCursor;
                        sender = service.ListFollowers(options);
                        if (sender == null)
                            break;
                        countCalls++;
                    }
                    // otherwise, we're done!
                    else
                        break;
                }
            }
            return Luser;

        }
        private List<TwitterUser> GetAllData(TwitterCursorList<TwitterUser> sender, ListFriendsOptions options)
        {
            int countCalls = 0;
            List<TwitterUser> Luser = new List<TwitterUser>();
            if (sender == null)
            {
                divError.InnerText = String.Concat("Error: Friend ", service.Response.Error.Message);
                userA.Visible = false;
            }
            else
            {
                while (sender.NextCursor != null)
                {
                    // if the API call did not succeed
                    if (sender == null)
                    {
                        divError.InnerText = service.Response.Error.Message;
                        userA.Visible = false;
                    }
                    else
                    {
                        foreach (TwitterUser user in sender)
                        {
                            Luser.Add(user);
                        }
                    }
                    //If more cursor, then go to next page
                    if (sender.NextCursor != 0 && sender.NextCursor != null)
                    {
                        options.Cursor = sender.NextCursor;
                        sender = service.ListFriends(options);
                        if (sender == null)
                            break;
                        countCalls++;
                    }
                    else
                        break;
                }
            }

            return Luser;

        }
        private void SaveToXML(List<TwitterUser> usersFL, List<TwitterUser> usersFR)
        {
            //create xml file
            XmlDocument xdoc = new XmlDocument();
            XmlElement el = (XmlElement)xdoc.AppendChild(xdoc.CreateElement("User"));
            // el.SetAttribute("ID", uniqueID);

            //TODO: Combine the both foreach Follower and Friends
            XmlElement followerElement = (XmlElement)el.AppendChild(xdoc.CreateElement("Follower"));
            foreach (TwitterUser user in usersFL)
            {
                XmlNode node = xdoc.CreateNode(XmlNodeType.Element, "user", "");
                node.InnerText = user.ScreenName;
                followerElement.AppendChild(node);
            }

            XmlElement friendElement = (XmlElement)el.AppendChild(xdoc.CreateElement("Friend"));
            foreach (TwitterUser user in usersFR)
            {
                XmlNode node = xdoc.CreateNode(XmlNodeType.Element, "user", "");
                node.InnerText = user.ScreenName;
                friendElement.AppendChild(node);
            }

            xdoc.AppendChild(el);
            //TODO: Check to see if the file already exist
            xdoc.Save(HostingEnvironment.MapPath(String.Concat("~/data/", accToken.ScreenName, ".xml")));
        }

        #endregion

        protected void btnSend_Click(object sender, EventArgs e)
        {
            divError.InnerText = "";
            divMessage.InnerHtml = "";
            if (accToken.ScreenName != "")
            {
                var options = new GetUserProfileForOptions { ScreenName = tbxUserA.Text.Trim().Replace("@", "") };

                var recipient = service.GetUserProfileFor(options);
                if (recipient != null)
                {
                    GetFriendshipInfoOptions op = new GetFriendshipInfoOptions
                    {
                        SourceScreenName = accToken.ScreenName,
                        TargetScreenName = recipient.ScreenName
                    };

                    TwitterFriendship friendship = service.GetFriendshipInfo(op);
                    if (service.Response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (friendship.Relationship.Source.CanDirectMessage.HasValue &&
                           friendship.Relationship.Source.CanDirectMessage.Value)
                        {
                            //uniqueID = DateTime.Now.Ticks.ToString() + '_' + accToken.ScreenName.ToString();

                            // Listed up Followers and Save to XML
                            ListAndSave(true);

                            StringBuilder str = new StringBuilder(accToken.ScreenName);
                            //TODO: URL cannot be sent via Direct Mail. Worked around. Find better wayt to communicate between Requeser and Requested user.
                            str.Append(" would like to play Twitter App with you. GoTo: http: //kickno.riot.jp?");
                            //str.Append(uniqueID);
                            str.Append("from=");
                            str.Append(accToken.ScreenName);

                            var tweet = service.SendDirectMessage(new SendDirectMessageOptions
                            {
                                ScreenName = options.ScreenName,
                                Text = str.ToString()
                            });
                            if (service.Response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                divMessage.InnerHtml = String.Concat("You have sent Direct Message to ", recipient.ScreenName.ToString(), "<br/><q>", str.ToString().Replace("http: //", "http://"), "</q>");
                            }
                            else
                            {
                                divError.InnerText = String.Concat("Message failed to sent :", service.Response.StatusDescription);
                            }
                        }
                        else
                        {
                            divError.InnerText = String.Concat("You are not allowed to send message this account");
                        }
                    }
                    else
                    {
                        divError.InnerText = String.Concat("Message failed to sent :");
                    }
                }
                else
                {
                    divError.InnerText = String.Concat("Could not find user: ", tbxUserA.Text.Trim());
                }

            }
            else
            {
                Response.Redirect("http://kickno.riot.jp/?Error=No_Token");
            }


        }
    }
}