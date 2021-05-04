using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Mod.CatsV3.Email.Models;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;
using Mod.CATSV3.Logs;
using System.Threading.Tasks;
using System.Net.Http;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Configuration;

namespace Mod.CatsV3.Email.Interface
{
    public class CatsEmailAppService : AppService, ICatsEmailAppService
    {
        ICatsAppService catsAppService;
        IServiceScopeFactory serviceProvider;
        IOptions<CATSEmailAPIConfiguration> config;
        EmailLogs emailLogs;
        public CatsEmailAppService(IObjectMapper objectMapper, ILogger<ICatsAppService> logger, ICatsAppService catsAppService, IModSession session, IServiceScopeFactory serviceProvider, IOptions<CATSEmailAPIConfiguration> config) : base(objectMapper, null, session)
        {
            this.catsAppService = catsAppService;
            this.serviceProvider = serviceProvider;
            this.config = config;
            this.emailLogs = new EmailLogs(serviceProvider);
        }

        public async Task SenArchivedEmailToMailbox(HttpContext HttpContext)
        {

            CorrespondenceDto dto = new CorrespondenceDto();
            try
            {

                string host = HttpContext.Request.Scheme.Contains("localhost") ? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host : ConfigurationManager.Secrets["MOD.CatsV3.SiteAPIUrl"];

                HttpClient client = new HttpClient();
                var request1 = HttpContext.CreateProxyHttpRequest(new Uri(host + "/api/correspondence/archiveEmailAttachment"));
                var response = await client.SendAsync(request1, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);
                //await HttpContext.CopyProxyHttpResponse(response);


                //using (var newRequest = new HttpRequestMessage(new HttpMethod(request.Method), request.Scheme + "://" + request.Host + "/api/correspondence/archiveEmailAttachment"))
                //{
                //    // Add headers, etc
                //    newRequest.Headers.Accept.Clear();
                //    var multiContent = new MultipartFormDataContent();
                //    var files = request.Form.Files;
                //    if (files != null)
                //    {
                //        files.ToList().ForEach(file =>
                //        {
                //            var fileStreamContent = new StreamContent(file.OpenReadStream());
                //            multiContent.Add(fileStreamContent, "final", file.FileName);
                //        });
                //    }
                //    var correspondence = request.Form.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value;
                //    dto = JsonConvert.DeserializeObject<CorrespondenceDto>(correspondence);
                //    multiContent.Add(new StringContent(correspondence), "correspondence");

                //    newRequest.Content = multiContent;// TODO: how to get content from HttpContext

                //    using (var serviceResponse = await new HttpClient().SendAsync(newRequest, System.Threading.CancellationToken.None))
                //    {
                //        // handle response
                //    }
                //}
            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
            }

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            //client.DefaultRequestHeaders.Accept.Clear();
            //var multiContent = new MultipartFormDataContent();


            //var file = formdata.Files;
            //if (file != null)
            //{
            //    formdata.Files.ToList().ForEach(file =>
            //    {
            //        var fileStreamContent = new StreamContent(file.OpenReadStream());
            //        multiContent.Add(fileStreamContent, "final", file.FileName);
            //    });
            //}

            //multiContent.Add(new StringContent(formdata.Select(x => x).FirstOrDefault(x => x.Key == "correspondence").Value), "correspondence");
            //try
            //{
            //    HttpResponseMessage response = await client.PostAsync("api/correspondence/archiveEmailAttachment", multiContent,  System.Threading.CancellationToken.None);
            //}
            //catch(Exception ex)
            //{

            //}
        }

        public void sendEmailnotificationAsync(CorrespondenceDto dto, string CurrentUserUPN, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null)
        {
            try
            {
                //send email notification in its own thread
                if (dto.CatsNotificationId == 2) // rejected
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        string emailSent = emailHelper.setCorrespondenceRejectedAsyn(dto, CurrentUserUPN);
                    }
                }
                else if (dto.CatsNotificationId == 10) // closing
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        string emailSent = emailHelper.setReviewPackageCloseFormCorrespondence(dto, CurrentUserUPN);
                    }
                }
                else
                {
                    //Lead Users emails
                    if (string.IsNullOrWhiteSpace(dto.LeadOfficeUsersIds) == false && dto.CatsNotificationId != 10)
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                            EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                            try
                            {
                                string emailSent = emailHelper.setCorrespondenceEmailRequestAsync(dto, CurrentUserUPN);
                            }
                            catch (Exception ex)
                            {
                                emailLogs.logTransactions(null, dto, ex);
                            }
                        }
                    }
                    //Copied Office Users emails
                    if (string.IsNullOrWhiteSpace(dto.CopiedUsersIds) == false && dto.CatsNotificationId != 10)
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();
                            EmailHelper emailHelper = new EmailHelper(config, anotherService, 3, currentBrowserUser, serviceProvider);
                            try
                            {
                                string emailSent = emailHelper.setCopiedOfficeEmailRequestAsync(dto, CurrentUserUPN);
                            }
                            catch (Exception ex)
                            {
                                emailLogs.logTransactions(null, dto, ex);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                emailLogs.logTransactions(null, dto, ex);
            }
        }

        public void sendEmailnotificationAsync(CorrespondenceDto dto, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null)
        {
            EmailHelper emailHelper = new EmailHelper(config, catsAppService, dto.CatsNotificationId, currentBrowserUser, serviceProvider, formFiles);
            string emailSent = emailHelper.sendFinalAttachedArchivedEmail(dto, currentBrowserUser.UserUpn);
        }

        public void sendEmailnotificationAsync(CorrespondenceDto dto, CollaborationDto collaboration, OriginatorDto[] originators, ReviewerDto[] reviewers, FYIUserDto[] fyiusers, CurrentBrowserUser currentBrowserUser, List<IFormFile> formFiles = null)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                try
                {

                    var anotherService = scope.ServiceProvider.GetRequiredService<ICatsAppService>();

                    string emailSent = "";
                    dto.CatsNotificationId = dto.CatsNotificationId;

                    if (dto.CatsNotificationId == 4)
                    {
                        // Launch new Round
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setReviewerEmailRequest(dto, collaboration, originators, reviewers, currentBrowserUser.UserUpn);
                    }
                    else if (dto.CatsNotificationId == 6)
                    {
                        // Add Reviewer
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setReviewerEmailRequest(dto, collaboration, originators, reviewers, currentBrowserUser.UserUpn);
                    }
                    else if (dto.CatsNotificationId == 5)
                    {
                        // Add Orginator
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setOriginatorEmailRequest(dto, originators, currentBrowserUser.UserUpn);
                    }
                    else if (dto.CatsNotificationId == 10)
                    {
                        // Reviewer Closed
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setReviewPackageClose(dto, originators, currentBrowserUser.UserUpn);
                    }
                    else if (dto.CatsNotificationId == 8 || dto.CatsNotificationId == 9)
                    {
                        // Reviewer completed/clear/draft: 9 -> draft; 8 -> completed
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setReviewCompletedDraft(dto, originators, currentBrowserUser.UserUpn);
                    }
                    else if (dto.CatsNotificationId == 14)
                    {
                        // sending a ping reminder to an assigned reviewer
                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setReviewReminder(dto, reviewers, currentBrowserUser.UserUpn);
                    }

                    // add FYIUser
                    if (dto.CatsNotificationId == 7 || ((dto.CatsNotificationId == 4 || dto.CatsNotificationId == 5 || dto.CatsNotificationId == 6) && fyiusers.Length > 0))
                    {
                        dto.CatsNotificationId = 7;
                        dto.CatsNotificationId = 7;

                        EmailHelper emailHelper = new EmailHelper(config, anotherService, dto.CatsNotificationId, currentBrowserUser, serviceProvider);
                        emailSent = emailHelper.setFYIUserEmailRequest(dto, fyiusers, currentBrowserUser.UserUpn);
                    }
                    // return emailSent;
                }
                catch (Exception ex)
                {
                    emailLogs.logTransactions(null, dto, ex);
                }
            }
        }
    }
}
