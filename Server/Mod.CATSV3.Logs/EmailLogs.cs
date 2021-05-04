using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mod.CATSV3.Logs
{
    public class EmailLogs
    {
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceProvider;
        public EmailLogs(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory serviceProvider = null)
        {
            this.serviceProvider = serviceProvider;
        }
        public void logTransactions(Task t, CorrespondenceDto dto, Exception ex = null, EmailNotificationLogDto emailLog = null)
        {
            
            using (var scope = this.serviceProvider.CreateScope())
            {
                if (emailLog == null)
                {
                    emailLog = new EmailNotificationLogDto();
                    //set email log
                    emailLog.CATSID = dto?.CATSID;
                    emailLog.Source = dto?.CatsNotificationId.ToString();
                    emailLog.Category = "0";
                    emailLog.CurrentRound = string.IsNullOrEmpty(dto?.CurrentReview) ? "NONE" : dto?.CurrentReview;
                    emailLog.IsError = true;
                    emailLog.EmailSubject = dto?.LetterSubject;
                    emailLog.CreatedBy = "";
                    emailLog.ModifiedBy = "";
                    emailLog.IsCurrentRound = true;

                    if (ex != null)
                    {
                        emailLog.IsError = true;
                        emailLog.Category = "3";
                        emailLog.Status = "Failed, but " + emailLog.CATSID + " Email Initiated...at " + DateTime.Now.ToString();
                        emailLog.EventType = Environment.UserName + " --> Failed, but " + emailLog.CATSID + " Email Initiated...at " + DateTime.Now.ToString(); ;
                        emailLog.EmailMessage = ex.Message + " -- " + ex.StackTrace;
                    }
                    else if (t.IsFaulted)
                    {
                        emailLog.IsError = true;
                        emailLog.Category = "3";
                        emailLog.Status = "Failed, but " + emailLog.CATSID + " Email Initiated...at " + DateTime.Now.ToString();
                        emailLog.EventType = Environment.UserName + " --> Failed, but " + emailLog.CATSID + " Email Initiated...at " + DateTime.Now.ToString();
                        emailLog.EmailMessage = t.Exception.Message;
                    }
                    else if (t.IsCompleted)
                    {
                        emailLog.IsError = false;
                        emailLog.Status = "Initiated";
                       emailLog.EventType = "Email initiated at " + DateTime.Now.ToString();
                    }
                    else
                    {
                        return;
                    }
                }

                var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                anotherService.getEmailNotificationLogAppService().Create(emailLog);
            }
        }
    }
}
