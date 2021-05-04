using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Email.Interface;
using Mod.CatsV3.Email.POCO;
using Mod.CATSV3.Logs;
using Mod.Framework.Configuration;
using Mod.Framework.User.Entities;
using Mod.Framework.User.Repositories;

namespace Mod.CatsV3.Email.Models
{
    public class EmailHelper : IMessage
    {
        private readonly IOptions<CATSEmailAPIConfiguration> config;
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceProvider;
        CurrentBrowserUser _currentUser;
        ICatsAppService _catsAppService;
        PostCATSEmailRequest postCATSEmailRequest;
        List<EmployeeDto> employees; 
        Email email;
        EmailLogs emailLogs;
        int catsNotificationType;
        List<IFormFile> formFiles;


        public EmailHelper(IOptions<CATSEmailAPIConfiguration> config, ICatsAppService catsAppService,  int catsNotificationType, CurrentBrowserUser currentUser, Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceProvider = null, List<IFormFile> formFiles = null)
        {
            this.email = new Email(config.Value); ;
            this.config = config;
            this.catsNotificationType = catsNotificationType;
            this.serviceProvider = serviceProvider;
            this.emailLogs = new EmailLogs(this.serviceProvider);
            this.formFiles = formFiles;
            _catsAppService = catsAppService;
            _currentUser = currentUser;
            employees = _catsAppService.getEmployeeAppService().GetAll().ToList();

        }

        public Email getEmail()
        {
            return email;
        }

        public object SendEMail()
        {
            throw new NotImplementedException();
        }

        public string SendEMailWithAttachement()
        {
            string isMessageSent = "true";

            EmailNotificationLogDto emailLog = new EmailNotificationLogDto();

            Stream[] streams = new Stream[email.EmailAttachments != null ? email.EmailAttachments.Count : 0];

            SmtpClient client = new SmtpClient(email.SMTP_CLIENT);
            client.Port = email.SMTP_CLIENT_PORT;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(senderAddress, netPassword);
            client.EnableSsl = false;
            //client.Credentials = credentials;
            try
            {
                MailAddressCollection TO_addressList;
                MailAddressCollection CC_addressList;
                MailMessage mail;
                BuildToAddressCollection(out TO_addressList, out CC_addressList, out mail);

                //Uncomment to attach from a local file
                //netAttachment = new System.Net.Mail.Attachment(@"C:\Users\JDKazadi\Documents\Email.txt"); 

                //Add attachements if any
                if (email.EmailAttachments != null && email.EmailAttachments.Count > 0)
                {
                    int index = 0;
                    foreach (IFormFile formfile in email.EmailAttachments)
                    {
                        email.fileName = formfile.FileName;
                        email.contentType = formfile.ContentType;

                        var stream = formfile.OpenReadStream();
                        mail.Attachments.Add(new Attachment(stream, email.fileName, email.contentType));
                        streams[index] = stream; ;
                        index++;
                    }
                }

                //set email log
                emailLog.CATSID = postCATSEmailRequest.ItemCATSID;
                emailLog.Source = postCATSEmailRequest.CATSNotificationType;
                emailLog.Category = "0";
                emailLog.EventType = "Email Sent...at " + DateTime.Now.ToString(); 
                emailLog.CurrentRound = !string.IsNullOrEmpty(postCATSEmailRequest.CurrentReviewRound) ? postCATSEmailRequest.CurrentReviewRound : "NONE";
                emailLog.IsError = false;
                emailLog.EmailSubject = mail.Subject;
                emailLog.EmailOrigin = mail.From.Address;
                emailLog.EmailOriginNames = mail.From.DisplayName;
                emailLog.EmailRecipientNames = email.EmailToFullNames;//string.Join(";", mail.To.Select(to => to.DisplayName).ToArray());
                emailLog.EmailRecipients = string.Join(";", mail.To.Select(to => to.Address).ToArray());
                emailLog.EmailCCs = string.Join(";", mail.CC.Select(to => to.Address).ToArray());
                emailLog.EmailCCsNames = email.EmailBCCFullNames;

                emailLog.EmailTemplate = postCATSEmailRequest.EmailTemplate;
                emailLog.EmailMessage = mail.Body;
                emailLog.CreatedBy = postCATSEmailRequest.CurrenFullName;
                emailLog.ModifiedBy = postCATSEmailRequest.CurrenFullName;
                emailLog.IsCurrentRound = true;

                using (var message = mail)
                {
                    //message.To.Add(TO_addressList.ToString());
                    //message.Bcc.Add(CC_addressList.ToString());
                    //client.Timeout = 100;
                    // Send email to smtp
                    client.Send(message);
                }

                isMessageSent = "Email Sent successfully";

                emailLog.Status = "Sent";

            }
            catch (SmtpFailedRecipientException ex)
            {
                SmtpStatusCode statusCode = ex.StatusCode;
                isMessageSent = "Recipient error on " + ex.FailedRecipient + " => " + ex.Message;
                emailLog.IsError = true;
                emailLog.Category = "3";
                emailLog.EventType = "Error: " + ex.Source + " at " + DateTime.Now.ToString(); ;
                emailLog.ErrorMessage = isMessageSent;
                emailLog.StackTrace = emailLog.StackTrace;
                emailLog.Status = "failed";
            }
            catch (SmtpException ex)
            {
                SmtpStatusCode statusCode = ex.StatusCode;
                isMessageSent = "SMTP error " + ex.InnerException + " => " + ex.Message;
                emailLog.IsError = true;
                emailLog.Category = "3";
                emailLog.EventType = "Error: " + ex.Source + " at " + DateTime.Now.ToString(); ;
                emailLog.ErrorMessage = isMessageSent;
                emailLog.StackTrace = ex.StackTrace;
                emailLog.Status = "failed";
            }
            catch (Exception ex)
            {
                isMessageSent = ex.ToString();//System.Security.Principal.WindowsIdentity.GetCurrent().Name + " ==> " + ex.Message;
                emailLog.IsError = true;
                emailLog.Category = "3";
                emailLog.EventType = "Error: " + ex.Source + " at " + DateTime.Now.ToString();
                emailLog.ErrorMessage = isMessageSent;
                emailLog.StackTrace = emailLog.StackTrace;
                emailLog.Status = "failed";
            }
            finally
            {
                foreach (Stream stream in streams)
                {
                    stream?.Dispose();
                }
                //create email status log
                emailLogs.logTransactions(null, null, null, emailLog);
            }

            return isMessageSent;
        }

        public string setCorrespondenceEmailRequestAsync(CorrespondenceDto dto, string CurrentUserUPN )
        {
            try {

                string ret = "";

                //get the previous log: NOT NEEDED AS THE CHECK IS BEING DONE FROM THE CLIENT SIDE
                // getPreviousEmailLog(dto.CATSID, dto.CurrentReview);

                //Get lead Office Users
                var cut = _catsAppService.getLeadOfficeAppService().GetBy(x => x.Name.ToUpper() == "CORRESPONDENCE").FirstOrDefault();
                //var currentLeadUsers = _catsAppService.getCorrespondenceAppService().Get(dto.Id).LeadOfficeUsersIds; // not sending to existing users copied already

                string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));


                postCATSEmailRequest = new PostCATSEmailRequest();

                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.LetterType = dto.LetterTypeName;
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.LeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.CopiedOffices = dto.CopiedOfficeName.Replace(";", ", ");
                postCATSEmailRequest.Due_for_Signature_By = dto.DueforSignatureByDate.Value.ToString("MM/dd/yyyy");
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.RejectReasons = dto.RejectionReason;
                postCATSEmailRequest.RejectLeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.Modified = dto.ModifiedTime.ToString("MM/dd/yyyy");
                postCATSEmailRequest.ModifiedBy = dto.ModifiedBy;

                //get Correspondence Unit Team
                var cutUPNs = cut.LeadOfficeMembers.Select(c => c.UserUPN).ToList();                
                List<string> cutFullNames = SetUsersFullNamesFromEmployees(cutUPNs);
                List<string> cutUPNsEmails = SetUsersEmailsFromEmployees(cutUPNs); //SetUsersEmailsFromAD(cutUPNs);

                postCATSEmailRequest.CorrespondentUnitUsers = cutUPNsEmails.Count > 0 ? cutUPNsEmails.ToArray() : new string[] { _currentUser.Email };
                postCATSEmailRequest.CorrespondentUnitUsersFullNames = cutFullNames.ToArray();

                if (dto.Rejected != true)
                {

                    //get Originators
                    if (dto.Collaboration != null)
                    {
                        //HtmlDocument doc = new HtmlDocument();
                        //doc.LoadHtml(dto.Originators);
                        //HtmlNodeCollection col = doc.DocumentNode.SelectNodes("//span");
                        //var innerTexts = col != null ? col.Select(x => x.InnerText).ToList() : new List<string>();

                        // var originators = innerTexts.Count == 0 && dto.Originators.IndexOf("<div") != -1 && !string.IsNullOrEmpty(dto.Originators) ? dto.Originators.Split(";").ToList() : innerTexts;

                        var originatorsUPNs = dto.Collaboration.Originators.Select(x => x.OriginatorUpn).ToList();//offices.Where(office => dto.LeadOfficeName.ToUpper().Trim() == office.Name.Trim().ToUpper()).FirstOrDefault().LeadOfficeMembers.Where(x => originators.Contains(x.UserFullName)).Select(x => x.UserUPN).ToList();

                        List<string> originatorsFullNames = SetUsersFullNamesFromEmployees(originatorsUPNs);

                        //get users email
                        List<string> originatorsEmails = SetUsersEmailsFromEmployees(originatorsUPNs);//SetUsersEmailsFromAD(originatorsUPNs);


                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsFullNames = originatorsFullNames.ToArray();
                    }
                    else
                    {
                        postCATSEmailRequest.Originators = cutUPNsEmails.ToArray();
                    }

                    //get office users
                    if (!string.IsNullOrEmpty(dto.LeadOfficeName))
                    {
                        //get users fullnames
                        List<string> officeUsersFullNames = employees.Where(e => dto.LeadOfficeUsersIds.ToLower().Contains(e.Upn.ToLower()) ).Select(e => e.DisplayName).ToList();

                        //get users email
                        List<string> officeEmails = employees.Where(e => dto.LeadOfficeUsersIds.ToLower().Contains(e.Upn.ToLower()) ).Select(e => e.EmailAddress).ToList();

                        postCATSEmailRequest.LeadOfficeUsers = officeEmails.ToArray();
                        postCATSEmailRequest.LeadOfficeUsersFullNames = officeUsersFullNames.ToArray();
                    }
                    else
                    {
                        postCATSEmailRequest.LeadOfficeUsers = cutUPNsEmails.ToArray();
                    }
                }

                email.EmailCurrentUser = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                if (!string.IsNullOrEmpty(email.EmailTo))
                {
                    ret = SendEMailWithAttachement();
                }
                else if (!string.IsNullOrEmpty(email.EmailBCC))
                {
                    email.EmailTo = _currentUser.Email;
                    ret = SendEMailWithAttachement();
                }
                return ret;

            }
            catch(Exception ex)
            {
                //return ex.Message;
                throw;
            }
            
        }

        public string setCorrespondenceRejectedAsyn(CorrespondenceDto dto, string currentUserUPN)
        {
            var correspondence = _catsAppService.getLeadOfficeAppService().GetAll("correspondence").FirstOrDefault();

            try
            {
                string ret = "";
                string upn = currentUserUPN.Substring(currentUserUPN.LastIndexOf("|") + 1);

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.LeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.RejectLeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString(); 
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                postCATSEmailRequest.ModifiedBy = _currentUser.PreferredName;
                postCATSEmailRequest.RejectReasons = dto.RejectionReason;
                postCATSEmailRequest.Modified = DateTime.Now.ToString();


                //get users email
                List<string> cutEmails = SetUsersEmailsFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                List<string> cutFullNames = SetUsersFullNamesFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());

                if (cutEmails.Count > 0)
                {

                    postCATSEmailRequest.CorrespondentUnitUsers = cutEmails.ToArray();
                    postCATSEmailRequest.CorrespondentUnitUsersFullNames = cutFullNames.ToArray();
                    email.EmailCurrentUser = _currentUser.Email;

                    email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                    if (!string.IsNullOrEmpty(email.EmailTo))
                    {
                        ret = SendEMailWithAttachement();
                    }
                    else if (!string.IsNullOrEmpty(email.EmailBCC))
                    {
                        email.EmailTo = _currentUser.Email;
                        ret = SendEMailWithAttachement();
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }
        }

        public string setReviewPackageCloseFormCorrespondence(CorrespondenceDto dto, string currentUserUPN)
        {
            var correspondence = _catsAppService.getLeadOfficeAppService().GetAll("correspondence").FirstOrDefault();

            try
            {
                string ret = "";
                string upn = currentUserUPN.Substring(currentUserUPN.LastIndexOf("|") + 1);

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString(); 
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;

                if (dto.Collaboration != null)
                {
                    //get users email
                    List<string> originatorsEmails = SetUsersEmailsFromEmployees(dto.Collaboration.Originators.Select(u => u.OriginatorUpn).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                    List<string> originatorsFullNames = SetUsersFullNamesFromEmployees(dto.Collaboration.Originators.Select(u => u.OriginatorUpn).ToList());

                    if (originatorsEmails.Count > 0)
                    {
                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsFullNames = originatorsFullNames.ToArray();
                    }
                    else
                    {
                        postCATSEmailRequest.Originators = Array.Empty<string>();
                        postCATSEmailRequest.OriginatorsFullNames = Array.Empty<string>();
                    }
                }

                //get users email
                List<string> cutEmails = SetUsersEmailsFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                List<string> cutFullNames = SetUsersFullNamesFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());

                if (cutEmails.Count > 0)
                {

                    postCATSEmailRequest.CorrespondentUnitUsers = cutEmails.ToArray();
                    postCATSEmailRequest.CorrespondentUnitUsersFullNames = cutFullNames.ToArray();
                    email.EmailCurrentUser = _currentUser.Email;

                    email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                    if (!string.IsNullOrEmpty(email.EmailTo))
                    {
                        ret = SendEMailWithAttachement();
                    }
                    else if (!string.IsNullOrEmpty(email.EmailBCC))
                    {
                        email.EmailTo = _currentUser.Email;
                        ret = SendEMailWithAttachement();
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }
        }

        public string setCopiedOfficeEmailRequestAsync(CorrespondenceDto dto, string CurrentUserUPN)
        {
            try
            {

                string ret = "";

                //get the previous log: NOT NEEDED AS THE CHECK IS BEING DONE FROM THE CLIENT SIDE
                // getPreviousEmailLog(dto.CATSID, dto.CurrentReview);

                //Get lead Office Users
                var cut = _catsAppService.getLeadOfficeAppService().GetBy(x => x.Name.ToUpper() == "CORRESPONDENCE").FirstOrDefault();
                //var currentCopiedUsers = _catsAppService.getCorrespondenceAppService().Get(dto.Id).CopiedUsersIds; // not sending to existing users copied already

                string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                postCATSEmailRequest = new PostCATSEmailRequest();

                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.LetterType = dto.LetterTypeName;
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.LeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.CopiedOffices = dto.CopiedOfficeName.Replace(";", ", ");
                postCATSEmailRequest.Due_for_Signature_By = dto.DueforSignatureByDate.Value.ToString("MM/dd/yyyy");
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.RejectReasons = dto.RejectionReason;
                postCATSEmailRequest.RejectLeadOffice = dto.LeadOfficeName;
                postCATSEmailRequest.Modified = dto.ModifiedTime.ToString("MM/dd/yyyy");
                postCATSEmailRequest.ModifiedBy = dto.ModifiedBy;

                //get Correspondence Unit Team

                var cutUPNs = cut.LeadOfficeMembers.Select(c => c.UserUPN).ToList();
                List<string> cutFullNames = SetUsersFullNamesFromEmployees(cutUPNs);
                List<string> cutUPNsEmails = SetUsersEmailsFromEmployees(cutUPNs); //SetUsersEmailsFromAD(cutUPNs);

                postCATSEmailRequest.CorrespondentUnitUsers = cutUPNsEmails.Count > 0 ? cutUPNsEmails.ToArray() : new string[] { _currentUser.Email };
                postCATSEmailRequest.CorrespondentUnitUsersFullNames = cutFullNames.ToArray();


                //get copiedoffice users
                if (!string.IsNullOrEmpty(dto.CopiedOfficeName))
                {

                    //get users fullnames
                    List<string> copiedOfficeUsersFullNames = employees.Where(e => dto.CopiedUsersIds.ToLower().Contains(e.Upn.ToLower()) ).Select(e => e.DisplayName).ToList();

                    //get users email
                    List<string> copiedOfficeEmails = employees.Where(e => dto.CopiedUsersIds.ToLower().Contains(e.Upn.ToLower()) ).Select(e => e.EmailAddress).ToList();

                    postCATSEmailRequest.CopiedUsers = copiedOfficeEmails.ToArray();
                    postCATSEmailRequest.CopiedUsersFullNames = copiedOfficeUsersFullNames.ToArray();



                    email.EmailCurrentUser = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                    email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                    if (!string.IsNullOrEmpty(email.EmailTo))
                    {
                        ret = SendEMailWithAttachement();
                    }
                    else if (!string.IsNullOrEmpty(email.EmailBCC))
                    {
                        email.EmailTo = _currentUser.Email;
                        ret = SendEMailWithAttachement();
                    }
                    return ret;
                }
                else
                {
                    postCATSEmailRequest.CopiedUsersFullNames = postCATSEmailRequest.CopiedUsers;
                    postCATSEmailRequest.CopiedUsers = cutUPNsEmails.ToArray();

                    return "Ok";
                }

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }

        }

        public string setReviewCompletedDraft(CorrespondenceDto dto, OriginatorDto[] originators, string CurrentUserUPN)
        {

            try
            {
                string ret = "";
                string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;
                postCATSEmailRequest.CurrenFullName = _currentUser.UserFullName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                //get Originators
                if (originators.Count() > 0)
                {
                    //get users email
                    List<string> originatorsEmails = SetUsersEmailsFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                    List<string> originatorsFullNames = SetUsersFullNamesFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());

                    if (originatorsEmails.Count > 0)
                    {

                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsFullNames = originatorsFullNames.ToArray();
                        email.EmailCurrentUser = _currentUser.Email;

                        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                        if (!string.IsNullOrEmpty(email.EmailTo))
                        {
                            ret = SendEMailWithAttachement();
                        }
                        else if (!string.IsNullOrEmpty(email.EmailBCC))
                        {
                            email.EmailTo = _currentUser.Email;
                            ret = SendEMailWithAttachement();
                        }
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }
        }

        public string setReviewPackageClose(CorrespondenceDto dto,OriginatorDto[] originators, string currentUserUPN)
        {
            var correspondence = _catsAppService.getLeadOfficeAppService().GetAll("correspondence").FirstOrDefault();

            try
            {
                string ret = "";
                string upn = currentUserUPN.Substring(currentUserUPN.LastIndexOf("|") + 1);

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;

                if (originators.Count() > 0)
                {
                    //get users email
                    List<string> originatorsEmails = SetUsersEmailsFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                    List<string> originatorsFullNames = SetUsersFullNamesFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());

                    if (originatorsEmails.Count > 0)
                    {
                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsFullNames = originatorsFullNames.ToArray();
                    }
                    else
                    {
                        postCATSEmailRequest.Originators = Array.Empty<string>();
                        postCATSEmailRequest.OriginatorsFullNames = Array.Empty<string>();
                    }
                }

                //get users email
                List<string> cutEmails = SetUsersEmailsFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                List<string> cutFullNames = SetUsersFullNamesFromEmployees(correspondence.LeadOfficeMembers.Select(u => u.UserUPN).ToList());

                if (cutEmails.Count > 0)
                {

                    postCATSEmailRequest.CorrespondentUnitUsers = cutEmails.ToArray();
                    postCATSEmailRequest.CorrespondentUnitUsersFullNames = cutFullNames.ToArray();
                    email.EmailCurrentUser = _currentUser.Email;

                    email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                    if (!string.IsNullOrEmpty(email.EmailTo))
                    {
                        ret = SendEMailWithAttachement();
                    }
                    else if (!string.IsNullOrEmpty(email.EmailBCC))
                    {
                        email.EmailTo = _currentUser.Email;
                        ret = SendEMailWithAttachement();
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }
        }

        public string sendFinalAttachedArchivedEmail(CorrespondenceDto dto, string currentUserUPN)
        {
            _currentUser.ItemSubject = dto.LetterSubject;
            _currentUser.CATSID = dto.CATSID;
            postCATSEmailRequest = new PostCATSEmailRequest();
            postCATSEmailRequest.EmailTemplate = "";
            postCATSEmailRequest.ItemCATSID = dto.CATSID;
            postCATSEmailRequest.CATSNotificationType = "ArchiveEmail";
            postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;
            postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;
            postCATSEmailRequest.ItemSubject = dto.LetterSubject;
            postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
            postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
            postCATSEmailRequest.EmailTemplate = "ArchiveEmail";

            email.EmailCurrentUser = _currentUser.Email;
            email = email.getEmailSettings(postCATSEmailRequest, formFiles, _currentUser);

            string result = "";

            if (email != null)
            {
                if (!string.IsNullOrEmpty(email.EmailTo))
                {
                    result = SendEMailWithAttachement();
                }
            }

            return result;
        }

        public string setReviewReminder(CorrespondenceDto dto, ReviewerDto[] reviewers, string CurrentUserUPN)
        {

            try
            {
                string ret = "";
                string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                //get Originators
                if (reviewers.Count() > 0)
                {
                    //get users email
                    List<string> reviewersEmails = SetUsersEmailsFromEmployees(reviewers.Select(u => u.ReviewerUPN).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                    List<string> reviewersFullNames = SetUsersFullNamesFromEmployees(reviewers.Select(u => u.ReviewerUPN).ToList());

                    if (reviewersEmails.Count > 0)
                    {

                        postCATSEmailRequest.Reviewers = reviewersEmails.ToArray();
                        postCATSEmailRequest.ReviewersNames = reviewersFullNames.ToArray();
                        email.EmailCurrentUser = _currentUser.Email;

                        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                        if (!string.IsNullOrEmpty(email.EmailTo))
                        {
                            ret = SendEMailWithAttachement();
                        }
                        else if (!string.IsNullOrEmpty(email.EmailBCC))
                        {
                            email.EmailTo = _currentUser.Email;
                            ret = SendEMailWithAttachement();
                        }
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw new ArgumentException("Email Exception occured: ",  ex); 
            }
        }

        public string setOriginatorEmailRequest(CorrespondenceDto dto, OriginatorDto[] originators, string CurrentUserUPN)
        {
            try
            {
                string ret = "";
                string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                //get Originators
                if (originators.Count() > 0)
                {
                    //get users email
                    List<string> originatorsEmails = SetUsersEmailsFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());//SetUsersEmailsFromAD(originatorsUPNs);
                    List<string> originatorsFullNames = SetUsersFullNamesFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());

                    if (originatorsEmails.Count > 0)
                    {

                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsNames = originatorsFullNames.ToArray();
                        postCATSEmailRequest.OriginatorsFullNames = originatorsFullNames.ToArray();
                        email.EmailCurrentUser = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                        if (!string.IsNullOrEmpty(email.EmailTo))
                        {
                            ret = SendEMailWithAttachement();
                        }
                        else if (!string.IsNullOrEmpty(email.EmailBCC))
                        {
                            email.EmailTo = _currentUser.Email;
                            ret = SendEMailWithAttachement();
                        }
                    }
                }
               
                return ret;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }

        }

        public string setReviewerEmailRequest(CorrespondenceDto dto, CollaborationDto collaboration, OriginatorDto[] originators, ReviewerDto[] reviewers, string CurrentUserUPN)
        {
            try
            {
                string ret = "";
               string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                var surrogates = _catsAppService.getSurrogateReviewerAppService().GetAll(reviewers.Select(r => r.ReviewerUPN).ToArray(), false).ToArray();

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                postCATSEmailRequest.Modified = dto.ModifiedTime.ToString("MM/dd/yyyy");
                postCATSEmailRequest.ModifiedBy = dto.ModifiedBy;
                
                //get Originators
                if (originators.Count() > 0)
                {
                    List<string> originatorsEmails = SetUsersEmailsFromEmployees(originators.Select(u => u.OriginatorUpn).ToList());

                    if (originatorsEmails.Count > 0)
                    {

                        postCATSEmailRequest.Originators = originatorsEmails.ToArray();
                        postCATSEmailRequest.OriginatorsNames = originators.Select(u => u.OriginatorName).ToArray();
                    }
                }

                //set reviewers
                if (reviewers.Count() > 0)
                {
                    var DLs = reviewers.Where(u => u.ReviewerUPN.Contains("DL-") == true).Select(g => g.ReviewerUPN).ToArray();
                    List<string> dlsMembers = new List<string>();
                    List<string> dlsMembersNames = new List<string>();                    
                    //get DLS members emails
                    List<string> dlsEmails = new List<string>();

                    if (DLs.Count() > 0)
                    {
                        
                        var res = _catsAppService.getDLGroupAppService().GetAll(DLs).ToList().Select(g => g.DLGroupMembers).ToList();
                        res.ForEach(g => {
                            g.ToList().ForEach(x => {
                                dlsMembers.Add(x.UserUPN);
                            });
                        });
                        dlsEmails = SetUsersEmailsFromEmployees(dlsMembers);
                        dlsMembersNames = DLs.ToList();
                        SetUsersFullNamesFromEmployees(dlsMembers).ForEach(x => { dlsMembersNames.Add(x); });
                    }

                    //get users email
                    List<string> reviewersEmails = SetUsersEmailsFromEmployees(reviewers.Where(u => !u.ReviewerUPN.Contains("DL-")).Select(u => u.ReviewerUPN).ToList()); 
                    dlsEmails.ForEach(x => reviewersEmails.Add(x));
                    List<string> reviewersFullNames = SetUsersFullNamesFromEmployees(reviewers.Where(u => !u.ReviewerUPN.Contains("DL-")).Select(u => u.ReviewerUPN).ToList());
                    dlsMembersNames.ForEach(x => { if (!reviewersFullNames.Contains(x)) reviewersFullNames.Add(x); });

                    List<string> surrogateEmails = SetUsersEmailsFromEmployees(surrogates.Select(s => s.SurrogateUPN).ToList());
                    List<string> surrogateNames = SetUsersFullNamesFromEmployees(surrogates.Select(s => s.SurrogateUPN).ToList());

                    if (reviewersEmails.Count > 0)
                    {

                        postCATSEmailRequest.Reviewers = reviewersEmails.ToArray();
                        postCATSEmailRequest.ReviewersNames = reviewersFullNames.ToArray();
                        postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                        postCATSEmailRequest.LeadOffice = dto.LeadOfficeName;
                        postCATSEmailRequest.SummaryBackground = collaboration.SummaryMaterialBackground;
                        if (surrogateEmails.Count > 0)
                        {
                            postCATSEmailRequest.SurrogatesUsers = surrogateEmails.ToArray();
                            postCATSEmailRequest.SurrogatesUsersNames = surrogateNames.ToArray();
                        }
                        if (collaboration.CurrentRoundEndDate != null)
                        {
                            postCATSEmailRequest.CurrentReviewRoundDueDate = ((DateTime)collaboration.CurrentRoundEndDate).ToString("MM/dd/yyyy");
                        }


                        email.EmailCurrentUser = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                        if (!string.IsNullOrEmpty(email.EmailTo))
                        {
                            ret = SendEMailWithAttachement();
                        }
                        else if (!string.IsNullOrEmpty(email.EmailBCC))
                        {
                            email.EmailTo = _currentUser.Email;
                            ret = SendEMailWithAttachement();
                        }
                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }

        }

        public string setFYIUserEmailRequest(CorrespondenceDto dto, FYIUserDto[] fYIUsers, string CurrentUserUPN)
        {
            try
            {
                string ret = "";
               string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
                //UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

                //SET THE PACKAGE SHAREPOINT FOLDER PHYSICAL PATH
                string relativeFileUrl = Path.Combine(ConfigurationManager.Secrets["MOD.CatsV3.SPSiteUrl"], ConfigurationManager.Secrets["MOD.CatsV3.DocumentLibraryDEV"],  dto.CATSID).Replace("\\", "/");

                postCATSEmailRequest = new PostCATSEmailRequest();
                postCATSEmailRequest.EmailTemplate = "";
                postCATSEmailRequest.CATSNotificationType = catsNotificationType.ToString();
                postCATSEmailRequest.CurrentUserEmail = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                postCATSEmailRequest.CurrenFullName = _currentUser.PreferredName;//LoggedUser.AdUserInfo.DisplayName;
                postCATSEmailRequest.ItemCATSID = dto.CATSID;
                postCATSEmailRequest.ItemSubject = dto.LetterSubject;
                postCATSEmailRequest.CurrentReviewRound = dto.CurrentReview;
                postCATSEmailRequest.ItemCorrespondentName = dto.CorrespondentName;
                postCATSEmailRequest.Modified = dto.ModifiedTime.ToString("MM/dd/yyyy");
                postCATSEmailRequest.ModifiedBy = dto.ModifiedBy;
                postCATSEmailRequest.CurrentFolder = relativeFileUrl;

                //get Originators
                if (fYIUsers.Count() > 0)
                {
                    var DLs = fYIUsers.Where(u => u.FYIUpn.Contains("DL-") == true).Select(g => g.FYIUpn).ToArray();
                    List<string> dlsMembers = new List<string>();
                    
                    //get DLS members emails
                    List<string> dlsEmails = new List<string>();
                    List<string> dlsMembersNames = new List<string>();

                    if (DLs.Count() > 0)
                    {

                        var res = _catsAppService.getDLGroupAppService().GetAll(DLs).ToList().Select(g => g.DLGroupMembers).ToList();
                        res.ForEach(g => {
                            g.ToList().ForEach(x => {
                                dlsMembers.Add(x.UserUPN);
                            });
                        });
                        dlsEmails = SetUsersEmailsFromEmployees(dlsMembers);
                        dlsMembersNames = DLs.ToList();
                        SetUsersFullNamesFromEmployees(dlsMembers).ForEach(x => { dlsMembersNames.Add(x); });
                    }

                    //get users email
                    List<string> fyiusersEmails = SetUsersEmailsFromEmployees(fYIUsers.Where(u => !u.FYIUpn.Contains("DL-")).Select(u => u.FYIUpn).ToList());
                    dlsEmails.ForEach(x => fyiusersEmails.Add(x));
                    List<string> fyiusersFullNames = SetUsersFullNamesFromEmployees(fYIUsers.Where(u => !u.FYIUpn.Contains("DL-")).Select(u => u.FYIUpn).ToList());
                    dlsMembersNames.ForEach(x => { if (!fyiusersFullNames.Contains(x)) fyiusersFullNames.Add(x); });

                    fyiusersEmails.Concat(dlsEmails).Distinct().ToList();
                    if (fyiusersEmails.Count > 0)
                    {
                        postCATSEmailRequest.FYIUsers = fyiusersEmails.ToArray();
                        postCATSEmailRequest.FYIUsersNames = fyiusersFullNames.ToArray();
                        email.EmailCurrentUser = _currentUser.Email;//LoggedUser.AdUserInfo.OfficeEmail;
                        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

                        if (!string.IsNullOrEmpty(email.EmailTo))
                        {
                            ret = SendEMailWithAttachement();
                        }
                        else if (!string.IsNullOrEmpty(email.EmailBCC))
                        {
                            email.EmailTo = _currentUser.Email;
                            ret = SendEMailWithAttachement();
                        }
                    }
                    
                }

                return null;

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
                throw;
            }

        }

        //public string setSurrogateEmailRequest(SurrogateOriginator dto, string CurrentUserUPN)

        //{
        //    try
        //    {
        //        string ret = "";
        //        //Get lead Office Users
        //        var offices = serviceLeadOffice.GetAll();

        //        string upn = CurrentUserUPN.Substring(CurrentUserUPN.LastIndexOf("|") + 1);
        //        UserInformation LoggedUser = new UserInformation(new ADUser(upn, config.Value.LdapDomain, config.Value.LdapPath));

        //        CurrentBrowserUser currentBrowserUser = new CurrentBrowserUser()
        //        {
        //            UserUpn = upn,
        //            Email = LoggedUser.AdUserInfo.OfficeEmail,
        //            UserFullName = LoggedUser.AdUserInfo.DisplayName
        //        };

        //        postCATSEmailRequest = new PostCATSEmailRequest();

        //        postCATSEmailRequest.SurrogatesUsers = new string[] { dto.Surrogate };
        //        postCATSEmailRequest.SURROGATE_OF = dto.CATSUser;
        //        postCATSEmailRequest.CurrentUserEmail = LoggedUser.AdUserInfo.OfficeEmail;
        //        postCATSEmailRequest.CurrenFullName = LoggedUser.AdUserInfo.DisplayName;
        //        postCATSEmailRequest.Modified = dto.ModifiedTime.ToString("MM/dd/yyyy");
        //        postCATSEmailRequest.ModifiedBy = dto.ModifiedBy;
        //        postCATSEmailRequest.CATSNotificationType = "999";
        //        //get office users


        //        email.EmailCurrentUser = LoggedUser.AdUserInfo.OfficeEmail;
        //        email = email.getEmailSettings(postCATSEmailRequest, null, _currentUser);

        //        return SendEMailWithAttachement();
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //        //throw;
        //    }

        //}

        private List<string> SetUsersEmailsFromEmployees(List<string> usersUPNs)
        {
            List<string> officeUsersEmails = new List<string>();

            foreach (var upn in usersUPNs)
            {
                if (upn.IndexOf("DL-") != -1)
                {
                    officeUsersEmails.Add(upn + "@spmx.omb.gov");
                    officeUsersEmails.Add(upn.ToLower() + "@omb.gov");
                }
                else
                {
                    var email = employees.Where(e => upn.Trim().ToLower().Contains(e.Upn.Trim().ToLower())).Select(e => e.EmailAddress).FirstOrDefault();
                    if (email != null)
                    {
                        officeUsersEmails.Add(email);
                    }                    
                }
            }

            return officeUsersEmails.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        private List<string> SetUsersFullNamesFromEmployees(List<string> usersUPNs)
        {
            List<string> officeUsersEmails = new List<string>();

            foreach (var upn in usersUPNs)
            {
                var name = employees.Where(e => upn.Trim().ToLower().Contains(e.Upn.Trim().ToLower())).Select(e => e.DisplayName).FirstOrDefault();
                if (name != null)
                {
                    officeUsersEmails.Add(name);
                }
            }

            return officeUsersEmails.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        private List<string> SetUsersEmailsFromAD(List<string> officeUsersUPNs)
        {
            List<string> officeUsersEmails = new List<string>();

            //foreach (var userLogin in officeUsersUPNs)
            //{
            //    string upn1 = userLogin.Substring(userLogin.LastIndexOf("|") + 1);
            //    UserInformation LoggedUser1 = new UserInformation(new ADUser(upn1, config.Value.LdapDomain, config.Value.LdapPath));
            //    if (LoggedUser1.AdUserInfo.OfficeEmail != null)
            //    {
            //        officeUsersEmails.Add(LoggedUser1.AdUserInfo.OfficeEmail);
            //    }
            //    else if (userLogin.IndexOf("DL-") != -1)
            //    {
            //        officeUsersEmails.Add(upn1 + "@spmx.omb.gov");
            //        officeUsersEmails.Add(upn1.ToLower() + "@omb.gov");
            //    }
            //}

            return officeUsersEmails;
        }

        private void BuildToAddressCollection(out MailAddressCollection TO_addressList, out MailAddressCollection CC_addressList, out MailMessage mail)
        {
            //The Destination email Addresses
            TO_addressList = new MailAddressCollection();
            CC_addressList = new MailAddressCollection();

            //Prepare the Destination email Addresses list
            if (email.EmailTo != null)
            {
                foreach (var curr_address in email.EmailTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (curr_address.Trim() != "")
                    {
                        MailAddress mytoAddress = new MailAddress(curr_address);
                        TO_addressList.Add(mytoAddress);
                    }
                }
            }

            //Prepare the Destination CC email Addresses list
            if (email.EmailBCC != null)
            {
                foreach (var curr_address in email.EmailBCC.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (curr_address.Trim() != "")
                    {
                        MailAddress myccAddress = new MailAddress(curr_address);
                        CC_addressList.Add(myccAddress);
                    }
                }
            }

            MailAddress fromAddress = new MailAddress(email.EmailFrom.Trim());
            mail = new MailMessage()
            {
                From = fromAddress,
                Subject = email.EmailSubject,
                Body = email.EmailBodyMessage,
                IsBodyHtml = true
            };

            foreach (var address in TO_addressList)
            {
                mail.To.Add(address.Address);
            }

            foreach (var address in CC_addressList)
            {
                mail.CC.Add(address.Address);
                //mail.Bcc.Add(address.Address);
            }
        }

        private IEnumerable<string> GetRecipientsUpn(List<EmailNotificationLogDto> latestEmailLog, Boolean IsRecipients)
        {
            if (catsNotificationType < 3)
            {
                if (!IsRecipients)
                {
                    return employees.Where(e => latestEmailLog.Any(log => log.EmailCCs.Contains(e.EmailAddress))).Select(e => e.Upn);
                }
                else
                {
                    return employees.Where(e => latestEmailLog.Any(log => log.EmailRecipients.Contains(e.EmailAddress))).Select(e => e.Upn);
                }
            }
            else
            {
                if (!IsRecipients)
                {
                    return employees.Where(e => latestEmailLog.FirstOrDefault().EmailCCs.ToUpper().IndexOf(e.EmailAddress.ToUpper()) != -1).Select(e => e.Upn);
                }
                else
                {
                    return employees.Where(e => latestEmailLog.FirstOrDefault().EmailRecipients.ToUpper().IndexOf(e.EmailAddress.ToUpper()) != -1).Select(e => e.Upn);
                }
            }




        }
    }
    public static class EnumHelper
    {
        public static T GetEnumValue<T>(string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }
            T val;
            return Enum.TryParse<T>(str, true, out val) ? val : default(T);
        }

        public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }

            return (T)Enum.ToObject(enumType, intValue);
        }
    }
}
