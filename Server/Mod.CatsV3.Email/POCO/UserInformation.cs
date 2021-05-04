using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mod.CatsV3.Email.POCO
{

    public class UserInformation
    {
        //public ADUser AdUserInfo { get; set; }

        //public UserInformation(ADUser AdUserInfo)
        //{
        //    this.AdUserInfo = AdUserInfo;
        //}

        //public static UserInformation LoggedInUser()
        //{
        //    //var id = (ClaimsIdentity)11;// HttpContext.Current.User.Identity as ClaimsIdentity;
        //    //var upn = id.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn").FirstOrDefault().Value;
        //    UserInformation loggedInApprover = new UserInformation(new ADUser("upn", "Connections.LdapDomain", "Connections.LdapPath"));
        //    return loggedInApprover;
        //}

    }

    //public class ADUser
    //{

    //    public Guid? Guid { get; set; }
    //    public string GivenName { get; set; }

    //    public string MiddleName { get; set; }

    //    public string Surname { get; set; }
    //    public string DisplayName { get; set; }
    //    public string OfficeTelephone { get; set; }
    //    public string OfficeEmail { get; set; }
    //    public string OrgAddress { get; set; }
    //    public string UserPrincipalName { get; set; }
    //    public string SamAccountName { get; set; }
    //    public string UserEmailForSending { get; set; }
    //    public string Title { get; set; }
    //    public ADUser Manager { get; set; }
    //    public ADUser Assistant { get; set; }
    //    public string ManagerDistinguishedName { get; set; }
    //    public bool HasManager { get; set; }
    //    public string ManagerEmailForSending { get; set; }
    //    public List<string> Groups { get; set; }

    //    public ADUser()
    //    {
    //    }
    //    public ADUser(string UPN, string Domain, string Path)
    //    {
    //        if (UPN != null)
    //        {
    //            //DirectorySearcher search = ADHelper.CreateDirectorySearcher(ADHelper.CreateDirectoryEntry());
    //            //search.Filter = ("UserPrincipalName=" + UPN);
    //            //SearchResult result = search.FindOne();

    //            //if (result != null)
    //            //{
    //            //    Load(result, Domain, Path);

    //            //}
    //        }


    //    }

    //    private void Load(SearchResult result, string Domain, string Path)
    //    {

    //        var propertyNames = result.Properties.PropertyNames.Cast<String>().ToList<String>();

    //        //this.Guid = userPrincipal.Guid;
    //        if (propertyNames.Contains("telephonenumber"))
    //        {
    //            OfficeTelephone = result.Properties["telephoneNumber"][0].ToString();
    //        }


    //        if (propertyNames.Contains("streetaddress"))
    //        {
    //            OrgAddress = result.Properties["streetaddress"][0].ToString();
    //        }

    //        if (propertyNames.Contains("extensionattribute3"))
    //        {
    //            OfficeEmail = result.Properties["extensionattribute3"][0].ToString();
    //        }

    //        if (propertyNames.Contains("title"))
    //        {
    //            Title = result.Properties["title"][0].ToString();
    //        }
    //        if (propertyNames.Contains("userprincipalname"))
    //        {
    //            UserPrincipalName = result.Properties["userprincipalname"][0].ToString();
    //        }
    //        if (propertyNames.Contains("samaccountname"))
    //        {
    //            SamAccountName = result.Properties["samaccountname"][0].ToString();
    //        }
    //        if (propertyNames.Contains("sn"))
    //        {
    //            Surname = result.Properties["sn"][0].ToString();
    //        }
    //        if (propertyNames.Contains("givenname"))
    //        {
    //            GivenName = result.Properties["givenname"][0].ToString();
    //        }
    //        if (propertyNames.Contains("displayname"))
    //        {
    //            DisplayName = result.Properties["displayname"][0].ToString();
    //        }
    //        if (propertyNames.Contains("middlename"))
    //        {
    //            MiddleName = result.Properties["middlename"][0].ToString();
    //        }
    //        if (propertyNames.Contains("extensionattribute3"))
    //        {
    //            UserEmailForSending = result.Properties["extensionattribute3"][0].ToString();
    //        }


    //    }


    //}
}
