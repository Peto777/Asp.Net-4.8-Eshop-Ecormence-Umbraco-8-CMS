using dufeksoft.lib.Mail;
using eshoppgsoftweb.lib.Controllers;
using System.Collections.Generic;
using System.Net.Mail;

namespace eshoppgsoftweb.lib.Util
{
    public class EshoppgsoftwebMailer
    {
        /// <summary>
        /// Send an email using HTML template
        /// </summary>
        /// <param name="mailSubject">Mail subject</param>
        /// <param name="mailBody">Mail body</param>
        /// <param name="sendTo">Send to email address</param>
        public static void SendMailTemplate(string mailSubject, string mailBody, string sendTo, Attachment attachement = null)
        {
            List<TextTemplateParam> paramList = new List<TextTemplateParam>();
            paramList.Add(new TextTemplateParam("EMAIL_SUBJ", mailSubject));
            paramList.Add(new TextTemplateParam("EMAIL_MSG", mailBody));
            paramList.Add(new TextTemplateParam("EMAIL_TO", sendTo));
            paramList.Add(new TextTemplateParam("WEB_ROOT", new _BaseControllerUtil().SiteRootUrl));

            MailProvider mailProvider = new MailProvider(null);
            mailProvider.SendMail(mailSubject, TextTemplate.GetTemplateText("EmailOne_Sk", paramList), sendTo, null, attachement);
        }

        /// <summary>
        /// Send an email using HTML template
        /// </summary>
        /// <param name="mailSubject">Mail subject</param>
        /// <param name="mailBody">Mail body</param>
        /// <param name="sendTo">Send to email address</param>
        public static void SendMailTemplateWithoutBcc(string mailSubject, string mailBody, string sendTo, Attachment attachement = null)
        {
            List<TextTemplateParam> paramList = new List<TextTemplateParam>();
            paramList.Add(new TextTemplateParam("EMAIL_SUBJ", mailSubject));
            paramList.Add(new TextTemplateParam("EMAIL_MSG", mailBody));
            paramList.Add(new TextTemplateParam("EMAIL_TO", sendTo));
            paramList.Add(new TextTemplateParam("WEB_ROOT", new _BaseControllerUtil().SiteRootUrl));

            MailProvider mailProvider = new MailProvider(null);
            mailProvider.UseSendToBcc = false;
            mailProvider.SendToBcc = null;
            mailProvider.SendMail(mailSubject, TextTemplate.GetTemplateText("EmailOne_Sk", paramList), sendTo, null, attachement);
        }
    }
}
