using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.DirectoryServices;

namespace ProjectPortal
{
  public class LdapAuthentication
  {
    private string _path;
    private string _filterAttribute;

    public LdapAuthentication(string path)
    {
      _path = path;
    }

    public bool IsAuthenticated(string domain, string username, string password)
    {
      //string domainAndUsername = domain + @"\" + username;
      //DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);
      DirectoryEntry entry = new DirectoryEntry("LDAP://server/ou=people,o=state.ak.us");
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

        //Update the new path to the user in the directory.
        _path = result.Path;
        _filterAttribute = (string)result.Properties["cn"][0];
      }
      catch (Exception ex)
      {
        throw new Exception("Error authenticating user. " + ex.Message);
      }

      return true;
    }

    public string GetGroups()
    {
      DirectorySearcher search = new DirectorySearcher(_path);
      search.Filter = "(cn=" + _filterAttribute + ")";
      search.PropertiesToLoad.Add("memberOf");
      StringBuilder groupNames = new StringBuilder();

      try
      {
        SearchResult result = search.FindOne();
        int propertyCount = result.Properties["memberOf"].Count;
        string dn;
        int equalsIndex, commaIndex;

        for(int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
        {
          dn = (string)result.Properties["memberOf"][propertyCounter];
       equalsIndex = dn.IndexOf("=", 1);
          commaIndex = dn.IndexOf(",", 1);
          if(-1 == equalsIndex)
          {
            return null;
          }
          groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
          groupNames.Append("|");
        }
      }
    catch(Exception ex)
    {
      throw new Exception("Error obtaining group names. " + ex.Message);
    }
    return groupNames.ToString();
  }
}
}