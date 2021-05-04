using System;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Mod.ExchangeDistribution
{
    public class Class1
    {
        private void GetCurrentUserMembership()
        {
            //Outlook.AddressEntry currentUser =
            //    Application.Session.CurrentUser.AddressEntry;
            //if (currentUser.Type == "EX")
            //{
            //    Outlook.ExchangeUser exchUser =
            //        currentUser.GetExchangeUser();
            //    if (exchUser != null)
            //    {
            //        Outlook.AddressEntries addrEntries =
            //            exchUser.GetMemberOfList();
            //        if (addrEntries != null)
            //        {
            //            foreach (Outlook.AddressEntry addrEntry
            //                in addrEntries)
            //            {
            //                //Debug.WriteLine(addrEntry.Name);
            //            }
            //        }
            //    }
            //}

            Outlook.Application Application = new Outlook.Application();
            Outlook.Accounts accounts = Application.Session.Accounts;
            foreach (Outlook.Account account in accounts)
            {
                Console.WriteLine(account.DisplayName);
            }
        }
        private void GetDistributionListMembers()
        {
            //Outlook.SelectNamesDialog snd =
            //    Application.Session.GetSelectNamesDialog();
            //Outlook.AddressLists addrLists =
            //    Application.Session.AddressLists;
            //foreach (Outlook.AddressList addrList in addrLists)
            //{
            //    if (addrList.Name == "All Groups")
            //    {
            //        snd.InitialAddressList = addrList;
            //        break;
            //    }
            //}
            //snd.NumberOfRecipientSelectors =
            //    Outlook.OlRecipientSelectors.olShowTo;
            //snd.ToLabel = "D/L";
            //snd.ShowOnlyInitialAddressList = true;
            //snd.AllowMultipleSelection = false;
            //snd.Display();
            //if (snd.Recipients.Count > 0)
            //{
            //    Outlook.AddressEntry addrEntry =
            //        snd.Recipients[1].AddressEntry;
            //    if (addrEntry.AddressEntryUserType ==
            //        Outlook.OlAddressEntryUserType.
            //        olExchangeDistributionListAddressEntry)
            //    {
            //        Outlook.ExchangeDistributionList exchDL =
            //            addrEntry.GetExchangeDistributionList();
            //        Outlook.AddressEntries addrEntries =
            //            exchDL.GetExchangeDistributionListMembers();
            //        if (addrEntries != null)
            //            foreach (Outlook.AddressEntry exchDLMember
            //                in addrEntries)
            //            {
            //                Debug.WriteLine(exchDLMember.Name);
            //            }
            //    }
            //}
        }

    }
}
