using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text;
using System.DirectoryServices;

namespace ProjectPortal
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void LogIn(object sender, EventArgs e)
        {
           string domainName = string.Empty;
            string adPath = string.Empty;
            string userName = Username.Text.Trim().ToUpper();
            string strError = string.Empty;
            LdapAuthentication adAuth = new LdapAuthentication(adPath);
            foreach (string k in ConfigurationSettings.AppSettings.Keys)
            {
                domainName = k.Contains("DirectoryDomain") ? ConfigurationSettings.AppSettings[k] : domainName;
                adPath = k.Contains("DirectoryPath") ? ConfigurationSettings.AppSettings[k] : adPath;
                if (!String.IsNullOrEmpty(domainName) && !String.IsNullOrEmpty(adPath))
                {
                    adAuth = new LdapAuthentication(adPath);
                }
            }
            try
            {
                if (true == adAuth.IsAuthenticated(domainName, Username.Text, Password.Text))
                {
                    //string groups = adAuth.GetGroups();
                    string groups = " ";
                    //Create the ticket, and add the groups.
                    bool isCookiePersistent = false;
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                              Username.Text, DateTime.Now, DateTime.Now.AddMinutes(60), isCookiePersistent, groups);

                    //Encrypt the ticket.
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                    //Create a cookie, and then add the encrypted ticket to the cookie as data.
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                    if (true == isCookiePersistent)
                        authCookie.Expires = authTicket.Expiration;

                    //Add the cookie to the outgoing cookies collection.
                    Response.Cookies.Add(authCookie);

                    //You can redirect now.
                    Session["Username"] = Username;
                    Session["Password"] = Password;
                    Response.Redirect(FormsAuthentication.GetRedirectUrl(Username.Text, false));
                }
                else
                {
                    errorLabel.Text = "Authentication did not succeed. Check user name and password.";
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = "Error authenticating. " + ex.Message;
            }
        }

        public bool AuthenticateUser(string domain, string username, string password, string LdapPath, out string Errmsg)
        {
            Errmsg = "";
            //string domainAndUsername = domain + @"\" + username;
            //DirectoryEntry entry = new DirectoryEntry(LdapPath, domainAndUsername, password);
            DirectoryEntry entry = new DirectoryEntry("LDAP://jnuldap.state.ak.us/ou=people,o=state.ak.us");
             entry.Username = "uid=" + username + ",ou=people,o=state.ak.us";
             entry.Password = password;
             entry.AuthenticationType = System.DirectoryServices.AuthenticationTypes.FastBind;
            try
            {
                // Bind to the native AdsObject to force authentication.
                Object obj = entry.NativeObject;
                DirectorySearcher searcher = new DirectorySearcher(entry);
                //search.Filter = "(SAMAccountName=" + username + ")";
                searcher.PropertiesToLoad.Add("cn");
                searcher.SearchScope = SearchScope.Subtree;
                searcher.Asynchronous = true;
                searcher.Filter = "(uid=" + username + ")";
                SearchResult result = searcher.FindOne();
                if (null == result)
                {
                    return false;
                }
                // Update the new path to the user in the directory
                LdapPath = result.Path;
                string _filterAttribute = (String)result.Properties["cn"][0];
                string name = _filterAttribute;
            }
            catch (Exception ex)
            {
                Errmsg = ex.Message;
                return false;
                throw new Exception("Error authenticating user." + ex.Message);
            }
            return true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Username.Text = string.Empty;
            Password.Text = string.Empty;
        }
    }
}