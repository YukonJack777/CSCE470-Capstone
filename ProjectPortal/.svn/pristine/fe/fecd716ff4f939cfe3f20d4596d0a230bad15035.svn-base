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

namespace ProjectPortal.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void LogIn(object sender, EventArgs e)
        {
           string dominName = string.Empty;
            string adPath = string.Empty;
            string userName = Username.Text.Trim().ToUpper();
            string strError = string.Empty;
            try
            {
                foreach (string key in ConfigurationSettings.AppSettings.Keys)
                {
                    dominName = key.Contains("DirectoryDomain") ? ConfigurationSettings.AppSettings[key] : dominName;
                    adPath = key.Contains("DirectoryPath") ? ConfigurationSettings.AppSettings[key] : adPath;
                    if (!String.IsNullOrEmpty(dominName) && !String.IsNullOrEmpty(adPath))
                    {
                        if (true == AuthenticateUser(dominName, userName, Password.Text, adPath, out strError))
                        {
                            Session["Username"] = Username.Text;
                            Session["Password"] = Password.Text;
                            Response.Redirect("~/default.aspx");// Authenticated user redirects to default.aspx
                        }
                        dominName = string.Empty;
                        adPath = string.Empty;
                        if (String.IsNullOrEmpty(strError)) break;
                    }

                }
                if (!string.IsNullOrEmpty(strError))
                {
                    lblError.Text = "Invalid user name or Password!";
                }
            }
            catch
            {

            }
            finally
            {

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