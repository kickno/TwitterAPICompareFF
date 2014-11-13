using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace test
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string urlAddress = "http://www.apple.com/contact/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                string data = readStream.ReadToEnd();
                ListData(data);

                response.Close();
                readStream.Close();
            }

        }

        private void ListData(string data)
        {

            //const string MatchPhonePattern = @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}";
            const string MatchPhonePattern = @"\(?\d{3}\)?\p{Pd}?\s*\d{3}\p{Pd}?\s*\p{Pd}?\d{4}";
            const string DottedPhonePattern = @"\b\d{3}[.]?\d{3}[.]?\d{4}\b";
            Regex rx = new Regex(MatchPhonePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            Regex rxdot = new Regex(DottedPhonePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            //Regex rxdot800 = new Regex(DottedPhonePattern800, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            MatchCollection matches = rx.Matches(data);
            MatchCollection matchesdot = rxdot.Matches(data);
            //MatchCollection matchesdot800 = rxdot800.Matches(data);
            // Report the number of matches found.
            int noOfMatches = matches.Count;
            int noOfMatchesdot = matchesdot.Count;
            //int noOfMatches800 = matchesdot800.Count;

            // Report on each match.
            string tempPhoneNumbers = "<ul>";
            foreach (Match match in matches)
            {   
                
                tempPhoneNumbers += String.Concat("<li>" + match.Value.ToString()+ "</li>");

            }
            foreach (Match match in matchesdot)
            {

                tempPhoneNumbers += String.Concat("<li>" + match.Value.ToString() + "</li>");

            }

            tempPhoneNumbers += "</ul>";
            divcontent.InnerHtml = tempPhoneNumbers;
          // divcontent.InnerText= data;
        }
    }
}