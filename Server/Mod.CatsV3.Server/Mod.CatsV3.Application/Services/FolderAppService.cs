using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Domain.Entities;
using Mod.CatsV3.Domain.Interfaces;
using Mod.CatsV3.Domain.Services;
using Mod.Framework.Application;
using Mod.Framework.Application.ObjectMapping;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.Application.Services
{
    public class FolderAppService : CrudAppServiceCATS<FolderDto, Folder>, IFolderAppService
    {
        public FolderAppService(IFolderRepository repository, IObjectMapper objectMapper, ILogger<IAppService> logger, IModSession session) : base(repository, objectMapper, logger, session)
        {
        }

        public override FolderDto Update(FolderDto dto)
        {
            var entity = Repository.Get(dto.Id);
            //update 
            if (entity.Documents != null)
            {
                foreach (var doc in entity.Documents)
                {
                    //update collaboration
                    PropertyInfo[] properties = doc.GetType().GetProperties();

                    foreach (PropertyInfo pi in properties)
                    {
                        var propName = pi.Name;
                        var entityValue = pi.GetValue(doc, null);
                        var updatedDto = dto.Documents.Where(x => x.Id == doc.Id).FirstOrDefault();
                        if (updatedDto != null)
                        {
                            PropertyInfo myPropInfo = updatedDto.GetType().GetProperty(propName);

                            if (pi != null && myPropInfo != null && (myPropInfo.PropertyType == typeof(string) ||
                                myPropInfo.PropertyType == typeof(string) ||
                                myPropInfo.PropertyType == typeof(DateTime) ||
                                myPropInfo.PropertyType == typeof(DateTime?) ||
                                myPropInfo.PropertyType == typeof(bool) ||
                                myPropInfo.PropertyType == typeof(bool?) ||
                                myPropInfo.PropertyType == typeof(int)))
                            {
                                var dtoValue = myPropInfo.GetValue(updatedDto, null);
                                if (dtoValue != null)
                                {
                                    if (dtoValue != entityValue && dtoValue != null && pi.CanWrite == true)
                                    {
                                        doc.GetType().GetProperty(propName).SetValue(doc, dtoValue, null);
                                    }
                                }
                            }

                        }
                    }
                }

                //add
                dto.Documents.ToList().ForEach(doc => {
                    if (doc.Id == 0)
                    {
                        Document tempDoc = new Document();
                        tempDoc = ObjectMapper.Map<Document>(doc);
                        entity.Documents.Add(tempDoc);
                    }
                });

                entity = Repository.Update(entity);

                var newDto = MapToDto(entity);

                return newDto;
            }
            else
            {
                return dto;
            }
        }
    }
}
