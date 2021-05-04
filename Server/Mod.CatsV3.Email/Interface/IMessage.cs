using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Mod.CatsV3.Email.Interface
{
    public interface IMessage
    {
        object SendEMail();

        string SendEMailWithAttachement();

        //Task<string> SendEMailWithAttachement();
    }
}
