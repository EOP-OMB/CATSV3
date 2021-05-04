using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Mod.Framework.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mod.CatsV3.Email.Models
{
    public class Email
    {
        
        public string EmailSubject { get; set; }       
        public string EmailBodyMessage { get; set; }        
        public string EmailFrom { get; set; }        
        public string EmailTo { get; set; }              
        public string EmailToFullNames { get; set; }        
        public string emailHtmlTemplate { get; set; }        
        public string EmailBCC { get; set; }
        public string EmailBCCFullNames { get; set; }
        public string EmailCurrentUser { get; set; }
        public string EmailCurrentUserName { get; set; }        
        public string fileName { get; set; }        
        public string contentType { get; set; }
        public Stream[] streamCollection { get; set; }
        public List<IFormFile> EmailAttachments { get; set; }        
        public string SMTP_CLIENT { get; set; }
        public int SMTP_CLIENT_PORT { get; set; }

        private string siteUrl = ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"];

        public Email(CATSEmailAPIConfiguration value)
        {
            EmailFrom = value.EmailFrom;
            SMTP_CLIENT = value.EmailServer;
            SMTP_CLIENT_PORT = value.EmailPort;
        }

        public Email getEmailSettings(PostCATSEmailRequest postCATSEmailRequest, List<IFormFile> formFiles, CurrentBrowserUser currentBrowserUser)
        {
            if (postCATSEmailRequest.CATSNotificationType == "ArchiveEmail")
            {
                if (formFiles?.Count > 0)
                {
                    Email d = GetmailRecipients(postCATSEmailRequest, this, currentBrowserUser);
                    d.EmailSubject = "CATS Package " + currentBrowserUser.CATSID + " Closed: " + currentBrowserUser.ItemSubject;
                    d.EmailBodyMessage = "CATS Final Document(s) for \"" + currentBrowserUser.CATSID + ": " + Environment.NewLine + currentBrowserUser.ItemSubject + Environment.NewLine + "\" Closed by " + currentBrowserUser.PreferredName;
                    d.EmailAttachments = formFiles;
                    return d;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                CATSNotificationTypeEnum notificationValue = EnumHelper.GetEnumValue<CATSNotificationTypeEnum>(int.Parse(postCATSEmailRequest.CATSNotificationType));
                emailHtmlTemplate = GetEnumDescription(notificationValue);
                postCATSEmailRequest.EmailTemplate = emailHtmlTemplate;

                string MailText = GetmailText(postCATSEmailRequest, emailHtmlTemplate, currentBrowserUser);
                //Get mailbody text from templates
                string MailSubject = GetmailSubject(postCATSEmailRequest, emailHtmlTemplate, currentBrowserUser);

                //EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                //EmailBCC = string.Join(";", postCATSEmailRequest.Reviewers);
                EmailSubject = MailSubject;
                EmailBodyMessage = MailText;
                EmailCurrentUserName = currentBrowserUser.UserFullName;
                EmailCurrentUser = currentBrowserUser.Email;

                //get email attachments if any
                if (formFiles != null)
                {
                    var tlistFiltered = formFiles.Where(item => item.FileName.IndexOf(postCATSEmailRequest.ItemCATSID) != -1).ToList<IFormFile>();
                    EmailAttachments = tlistFiltered;
                }

                Email d = GetmailRecipients(postCATSEmailRequest, this, currentBrowserUser);
                return d;
            }
        }

        private static string GetmailText(PostCATSEmailRequest postCATSEmailRequest, string emailBodyTemplate, CurrentBrowserUser currentBrowserUser)
        {
            //Fetching Email Body Text from EmailTemplate File.  
            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"EmailTemplates\EmailPackageCloseTemplate.html");
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"EmailTemplates\" + emailBodyTemplate + "Template.html");
            string siteUrl = ConfigurationManager.Secrets["MOD.CatsV3.SiteUrl"];
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            switch (postCATSEmailRequest.CATSNotificationType)
            {

                case "1":
                    Console.WriteLine("Case 1 - EmailPackageLetterCreated");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[DocumentType]", postCATSEmailRequest.LetterType)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject)
                    .Replace("[Lead Offices]", postCATSEmailRequest.LeadOffice)
                    .Replace("[Copied Office]", postCATSEmailRequest.CopiedOffices)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Due for Signature By]", postCATSEmailRequest.Due_for_Signature_By);
                    break;
                case "2":
                    Console.WriteLine("Case 2- EmailPackageRejected"); 
                    MailText = MailText
                     .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                     .Replace("[Subject]", postCATSEmailRequest.ItemSubject)
                     .Replace("[Lead Offices]", postCATSEmailRequest.LeadOffice)
                     .Replace("[RejectReasons]", postCATSEmailRequest.RejectReasons)
                     .Replace("[ModifiedBy]", currentBrowserUser.UserFullName)
                     .Replace("[siteUrl]", siteUrl)
                     .Replace("[Modified]", postCATSEmailRequest.Modified);
                    break;
                case "3":
                    Console.WriteLine("Case 3 - EmailPackageCopied"); 
                    MailText = MailText
                     .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                     .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                     .Replace("[DocumentType]", postCATSEmailRequest.LetterType)
                     .Replace("[Subject]", postCATSEmailRequest.ItemSubject)
                     .Replace("[Lead Offices]", postCATSEmailRequest.LeadOffice)
                     .Replace("[Copied Office]", postCATSEmailRequest.CopiedOffices)
                    .Replace("[siteUrl]", siteUrl)
                     .Replace("[Due for Signature By]", postCATSEmailRequest.Due_for_Signature_By);
                    break;
                case "4":
                    Console.WriteLine("Case 4 - EmailPackageLaunchReview");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[ToEmailRecipients]", string.Join(", ", postCATSEmailRequest.ReviewersNames))
                    .Replace("[Originators]", string.Join(", ", postCATSEmailRequest.OriginatorsNames))
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject)
                    .Replace("[Lead Offices]", postCATSEmailRequest.LeadOffice)
                    .Replace("[CurrentReviewRound]", postCATSEmailRequest.CurrentReviewRound)
                    .Replace("[SummaryBackground]", postCATSEmailRequest.SummaryBackground)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[CurrentReviewRoundDueDate]", postCATSEmailRequest.CurrentReviewRoundDueDate);
                    break;
                case "5":
                    Console.WriteLine("Case 5 - EmailPackageAddOriginators");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[Created]", currentBrowserUser.UserFullName)
                    .Replace("[ToEmailRecipients]", string.Join(", ", postCATSEmailRequest.OriginatorsNames))
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[CurrentReviewRound]", postCATSEmailRequest.CurrentReviewRound);
                    break;
                case "6":
                    Console.WriteLine("Case 6 - EmailPackageAddReviewers");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[ToEmailRecipients]", string.Join(", ", postCATSEmailRequest.ReviewersNames))
                    .Replace("[Originators]", postCATSEmailRequest.CurrenFullName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject)
                    .Replace("[Lead Offices]", postCATSEmailRequest.LeadOffice)
                    .Replace("[CurrentReviewRound]", postCATSEmailRequest.CurrentReviewRound)
                    .Replace("[SummaryBackground]", postCATSEmailRequest.SummaryBackground)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[CurrentReviewRoundDueDate]", postCATSEmailRequest.CurrentReviewRoundDueDate);
                    break;
                case "7":
                    Console.WriteLine("Case 7 - EmailPackageAddFYIUsers");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[Created]", currentBrowserUser.UserFullName)
                    .Replace("[CurrentReviewRound]", postCATSEmailRequest.CurrentReviewRound)
                    .Replace("[CurrentFolderUrl]", postCATSEmailRequest.CurrentFolder)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[ToEmailRecipients]", string.Join(", ", postCATSEmailRequest.FYIUsersNames));
                    break;
                case "8":
                    Console.WriteLine("Case 8 - EmailPackageReviewCompleted");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[Created]", currentBrowserUser.UserFullName)
                    .Replace("[Modified By]", string.IsNullOrEmpty(currentBrowserUser.SURROGATE_ID) == true ? currentBrowserUser.UserFullName : currentBrowserUser.UserFullName + " (on behalf of : " + postCATSEmailRequest.SURROGATE_OF + ")")
                    .Replace("[CurrentReviewRound]", postCATSEmailRequest.CurrentReviewRound)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "9":
                    Console.WriteLine("Case 9 - EmailPackageReviewDraft");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[Modified By]", string.IsNullOrEmpty(currentBrowserUser.SURROGATE_ID) == true ? currentBrowserUser.UserFullName : currentBrowserUser.UserFullName + " (on behalf of : " + postCATSEmailRequest.SURROGATE_OF + ")")
                    .Replace("[Draft Reason]", postCATSEmailRequest.DraftReason)
                    .Replace("[SummaryBackground]", postCATSEmailRequest.SummaryBackground)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "10":
                    Console.WriteLine("Case 10 - EmailPackageClose");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Modified By]", string.IsNullOrEmpty(currentBrowserUser.SURROGATE_ID) == true ? currentBrowserUser.UserFullName : currentBrowserUser.UserFullName + " (on behalf of : " + postCATSEmailRequest.SURROGATE_OF + ")")
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "11":
                    Console.WriteLine("Case 11 - EmailPackageReopen");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Modified By]", string.IsNullOrEmpty(currentBrowserUser.SURROGATE_ID) == true ? currentBrowserUser.UserFullName : currentBrowserUser.UserFullName + " (on behalf of : " + postCATSEmailRequest.SURROGATE_OF + ")")
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "12":
                    Console.WriteLine("Case 12 - EmailPackageCloseAdminConsole");
                    MailText = MailText
                    .Replace("[CurrentUser]", currentBrowserUser.PreferredName)
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "13":
                    Console.WriteLine("Case 13 - EmailPackageToSupport");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[StatusMessage]", postCATSEmailRequest.CATSSupportStatusMessage)
                    .Replace("[CurrentRound]", postCATSEmailRequest.CurrentReviewRound)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[EmailRecipient]", postCATSEmailRequest.CATSSupportIntendedRecipients);
                    break;
                case "14":
                    Console.WriteLine("Case 14 - EmailPackageReviewReminder");
                    MailText = MailText
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[siteUrl]", siteUrl)
                    .Replace("[ToEmailRecipients]", string.Join(";", postCATSEmailRequest.ReviewersNames ?? Array.Empty<string>()));
                    break;
                case "15":
                    Console.WriteLine("Case 15- EmailPackageRemoveReviewer");
                    break;
                case "16":
                    Console.WriteLine("Case 16 - EmailPackageRemoveFYI");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            return MailText;
        }

        private static string GetmailSubject(PostCATSEmailRequest postCATSEmailRequest, string emailHtmlTemplate, CurrentBrowserUser currentBrowserUser)
        {
            //Fetching Email Body Text from EmailTemplate File. 
            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"EmailTemplates\EmailPackageCloseTemplate.txt");
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"EmailTemplates\" + emailHtmlTemplate + "Template.txt");
            StreamReader str = new StreamReader(FilePath);
            string MailSubject = str.ReadToEnd();
            str.Close();

            switch (postCATSEmailRequest.CATSNotificationType)
            {

                case "1":
                    Console.WriteLine("Case 1 - EmailPackageLetterCreated");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[DocumentType]", postCATSEmailRequest.LetterType)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "2":
                    Console.WriteLine("Case 2- EmailPackageRejected"); 
                    MailSubject = MailSubject
                     .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                     .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                     .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "3":
                    Console.WriteLine("Case 3 - EmailPackageCopied");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[DocumentType]", postCATSEmailRequest.LetterType)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "4":
                    Console.WriteLine("Case 4 - EmailPackageLaunchReview");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "5":
                    Console.WriteLine("Case 5 - EmailPackageAddOriginators");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "6":
                    Console.WriteLine("Case 6 - EmailPackageAddReviewers");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "7":
                    Console.WriteLine("Case 7 - EmailPackageAddFYIUsers");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "8":
                    Console.WriteLine("Case 8 - EmailPackageReviewCompleted");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "9":
                    Console.WriteLine("Case 9 - EmailPackageReviewDraft");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "10":
                    Console.WriteLine("Case 10 - EmailPackageClose");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "11":
                    Console.WriteLine("Case 11 - EmailPackageReopen");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "12":
                    Console.WriteLine("Case 12 - EmailPackageCloseAdminConsole");
                    MailSubject = MailSubject
                    .Replace("[CurrentUser]", currentBrowserUser.PreferredName)
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace("[CorrespondentName]", postCATSEmailRequest.ItemCorrespondentName)
                    .Replace("[Subject]", postCATSEmailRequest.ItemSubject);
                    break;
                case "13":
                    Console.WriteLine("Case 13 - EmailPackageToSupport");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID);
                    break;
                case "14":
                    Console.WriteLine("Case 14 - EmailPackageReviewReminder");
                    MailSubject = MailSubject
                    .Replace("[CATSID]", postCATSEmailRequest.ItemCATSID)
                    .Replace(" [Originator]", currentBrowserUser.UserFullName);
                    break;
                case "15":
                    Console.WriteLine("Case 15- EmailPackageRemoveReviewer");
                    break;
                case "16":
                    Console.WriteLine("Case 16 - EmailPackageRemoveFYI");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            return RemoveSpecialCharacters(MailSubject);
        }

        private static Email GetmailRecipients(PostCATSEmailRequest postCATSEmailRequest, Email email, CurrentBrowserUser currentBrowserUser)
        {


            switch (postCATSEmailRequest.CATSNotificationType)
            {
                case "1":
                    Console.WriteLine("Case 1 - EmailPackageLetterCreated");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.LeadOfficeUsers ?? Array.Empty<string>());
                    email.EmailBCC = string.Join(";", postCATSEmailRequest.CopiedUsers ?? Array.Empty<string>());
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.LeadOfficeUsersFullNames ?? Array.Empty<string>());
                    email.EmailBCCFullNames = string.Join(";", postCATSEmailRequest.CopiedUsersFullNames);
                    break;
                case "2":
                    Console.WriteLine("Case 2- EmailPackageRejected");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.CorrespondentUnitUsers);
                    email.EmailBCC = email.EmailCurrentUser;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.CorrespondentUnitUsersFullNames);
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName);
                    break;
                case "3":
                    Console.WriteLine("Case 3 - EmailPackageCopied");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.CopiedUsers);
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.CopiedUsersFullNames);
                    email.EmailBCC = email.EmailCurrentUser;
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName);
                    break;
                case "4":
                    Console.WriteLine("Case 4 - EmailPackageLaunchReview");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Reviewers);
                    email.EmailBCC = string.Join(";", postCATSEmailRequest.Originators) + (postCATSEmailRequest.SurrogatesUsers.Length > 0 ? ";" + string.Join(";", postCATSEmailRequest.SurrogatesUsers) : "");//email.EmailCurrentUser;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.ReviewersNames);
                    email.EmailBCCFullNames = string.Join(";", postCATSEmailRequest.OriginatorsFullNames) + (postCATSEmailRequest.SurrogatesUsersNames.Length > 0 ? ";" + string.Join(";", postCATSEmailRequest.SurrogatesUsersNames) : "");
                    break;
                case "5":
                    Console.WriteLine("Case 5 - EmailPackageAddOriginators");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = email.EmailCurrentUser;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.OriginatorsFullNames);
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName);
                    break;
                case "6":
                    Console.WriteLine("Case 6 - EmailPackageAddReviewers");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Reviewers);
                    email.EmailBCC = email.EmailCurrentUser + (postCATSEmailRequest.SurrogatesUsers.Length > 0 ? ";" + string.Join(";", postCATSEmailRequest.SurrogatesUsers) : "");
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.ReviewersNames);
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName) + (postCATSEmailRequest.SurrogatesUsersNames.Length > 0 ? ";" + string.Join(";", postCATSEmailRequest.SurrogatesUsersNames) : "");
                    break;
                case "7":
                    Console.WriteLine("Case 7 - EmailPackageAddFYIUsers");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.FYIUsers);
                    email.EmailBCC = email.EmailCurrentUser;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.FYIUsersNames);
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName);
                    break;
                case "8":
                    Console.WriteLine("Case 8 - EmailPackageReviewCompleted");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = postCATSEmailRequest.CurrentUserEmail;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.OriginatorsFullNames);
                    email.EmailBCCFullNames = postCATSEmailRequest.CurrenFullName;
                    break;
                case "9":
                    Console.WriteLine("Case 9 - EmailPackageReviewDraft");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = postCATSEmailRequest.CurrentUserEmail;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.OriginatorsFullNames);
                    email.EmailBCCFullNames = postCATSEmailRequest.CurrenFullName;
                    break;
                case "10":
                    Console.WriteLine("Case 10 - EmailPackageClose");
                    email.EmailTo = email.EmailCurrentUser + ";" + string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = email.EmailCurrentUser + ";" + string.Join(";", postCATSEmailRequest.CorrespondentUnitUsers);
                    email.EmailToFullNames = postCATSEmailRequest.CurrenFullName + ";" + string.Join(";", postCATSEmailRequest.OriginatorsFullNames);
                    email.EmailBCCFullNames = postCATSEmailRequest.CurrenFullName + ";" + string.Join(";", postCATSEmailRequest.CorrespondentUnitUsersFullNames);
                    break;
                case "11":
                    Console.WriteLine("Case 11 - EmailPackageReopen");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = email.EmailCurrentUser + ";" + string.Join(";", postCATSEmailRequest.CorrespondentUnitUsers);
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.LeadOfficeUsersFullNames);
                    email.EmailBCCFullNames = string.Join(";", postCATSEmailRequest.CopiedUsersFullNames);
                    break;
                case "12":
                    Console.WriteLine("Case 12 - EmailPackageCloseAdminConsole");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Originators);
                    email.EmailBCC = string.Join(";", postCATSEmailRequest.ReviewersIncludedDLsMembers) + ";" + string.Join(";", postCATSEmailRequest.CorrespondentUnitUsers);
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.LeadOfficeUsersFullNames);
                    email.EmailBCCFullNames = string.Join(";", postCATSEmailRequest.CopiedUsersFullNames);
                    break;
                case "13":
                    Console.WriteLine("Case 13 - EmailPackageToSupport");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.CATSSupportTeam);
                    break;
                case "14":
                    Console.WriteLine("Case 14 - EmailPackageReviewReminder");
                    email.EmailTo = string.Join(";", postCATSEmailRequest.Reviewers);
                    email.EmailBCC = email.EmailCurrentUser;
                    email.EmailToFullNames = string.Join(";", postCATSEmailRequest.ReviewersNames);
                    email.EmailBCCFullNames = string.Join(";", email.EmailCurrentUserName);
                    break;
                case "15":
                    Console.WriteLine("Case 15- EmailPackageRemoveReviewer");
                    break;
                case "16":
                    Console.WriteLine("Case 16 - EmailPackageRemoveFYI");
                    break;
                case "ArchiveEmail":
                    Console.WriteLine("ArchiveEmail");
                    string cATSArchiveEmailService = ConfigurationManager.Secrets["MOD.CatsV3.CATSArchiveEmailService"];
                    email.EmailTo = cATSArchiveEmailService;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }

            return email;
        }

        public static string[] GetmailEventTypeAndSource(PostCATSEmailRequest postCATSEmailRequest)
        {
            string Source = "";
            string EventType = "";

            switch (postCATSEmailRequest.CATSNotificationType)
            {
                case "1":
                    Console.WriteLine("Case 1 - EmailPackageLetterCreated");
                    Source = "Correspondence Dashboard";
                    EventType = "Letter package Created";
                    break;
                case "2":
                    Console.WriteLine("Case 2- EmailPackageRejected");
                    Source = "Correspondence Dashboard";
                    EventType = "Letter Package Returned";
                    break;
                case "3":
                    Console.WriteLine("Case 3 - EmailPackageCopied");
                    Source = "Correspondence Dashboard";
                    EventType = "Letter Package Returned";
                    break;
                case "4":
                    Console.WriteLine("Case 4 - EmailPackageLaunchReview");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Originator Dashboard" : postCATSEmailRequest.Source;
                    EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Review Start" : postCATSEmailRequest.EventType;
                    break;
                case "5":
                    Console.WriteLine("Case 5 - EmailPackageAddOriginators");
                    Source = "Originator Dashboard";
                    EventType = "Add Originator";
                    break;
                case "6":
                    Console.WriteLine("Case 6 - EmailPackageAddReviewers");
                    Source = "Originator Dashboard";
                    EventType = "Add Reviewer";
                    break;
                case "7":
                    Console.WriteLine("Case 7 - EmailPackageAddFYIUsers");
                    Source = "Originator Dashboard";
                    EventType = "Add FYUsers";
                    break;
                case "8":
                    Console.WriteLine("Case 8 - EmailPackageReviewCompleted");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Review Dashboard" : postCATSEmailRequest.Source;
                    EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Review Completed" : postCATSEmailRequest.EventType; ;
                    break;
                case "9":
                    Console.WriteLine("Case 9 - EmailPackageReviewDraft");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Review Dashboard" : postCATSEmailRequest.Source;
                    //EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Draft Sent" : postCATSEmailRequest.EventType;
                    EventType = "Draft Sent";
                    break;
                case "10":
                    Console.WriteLine("Case 10 - EmailPackageClose");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Review Dashboard" : postCATSEmailRequest.Source;
                    //EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Closed" : postCATSEmailRequest.EventType;
                    EventType = "Closed";
                    break;
                case "11":
                    Console.WriteLine("Case 11 - EmailPackageReopen");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Admin Console" : postCATSEmailRequest.Source;
                    EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Reopen" : postCATSEmailRequest.EventType;
                    EventType = "Reopen By Correspondence Team";
                    break;
                case "12":
                    Console.WriteLine("Case 11 - EmailPackageCloseAdminConsole");
                    Source = string.IsNullOrEmpty(postCATSEmailRequest.Source) ? "Admin Console" : postCATSEmailRequest.Source;
                    EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Closed" : postCATSEmailRequest.EventType;
                    EventType = "Closed By Admin";
                    break;
                case "13":
                    Console.WriteLine("Case 12 - EmailPackageToSupport");
                    Source = "CATS Support";
                    EventType = "Report Errors";
                    break;
                case "14":
                    Console.WriteLine("Case 13 - EmailPackageRemoveOriginator");
                    break;
                case "15":
                    Console.WriteLine("Case 14- EmailPackageRemoveReviewer");
                    break;
                case "16":
                    Console.WriteLine("Case 15 - EmailPackageRemoveFYI");
                    break;
                default:
                    Console.WriteLine("Default case");
                    Source = "Admin Console";
                    EventType = string.IsNullOrEmpty(postCATSEmailRequest.EventType) ? "Closed" : postCATSEmailRequest.EventType;
                    break;
            }

            return new string[] { Source, EventType };
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^0-9A-Za-z ,:-]", "");
        }

        public FormCollection CreateTempFormData(PostCATSEmailRequest postCATSEmailRequest, IFormCollection formdata, CurrentBrowserUser currentBrowserUser)
        {
            var formFiles = new FormFileCollection();
            foreach (var frm in formdata.Files)
            {
                formFiles.Add(frm);
            }


            var formCol = new FormCollection(new Dictionary<string, StringValues>
            {
                { "CurrentBrowserUser", JsonConvert.SerializeObject(currentBrowserUser) },
                { "PostCATSEmailRequest", JsonConvert.SerializeObject(postCATSEmailRequest)}
            }, formFiles);

            //var formCollection = new FormCollection(new Dictionary<string, StringValues>(), formFiles);

            return formCol;
        }
    }
}
