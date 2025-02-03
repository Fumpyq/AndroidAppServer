using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using Org.BouncyCastle.Ocsp;
using System.Text;
using ADCHGKUser4.Controllers.Libs;
using MailKit.Net.Smtp;
using static AndroidAppServer.Controllers.HandMadeController;
using System.Web;

namespace AndroidAppServer.Libs
{
    public class MailService
    {

        public const string LogPath = "\\feedBackLogs";

        public static MimeMessage FormatFeedBackMessage(FeedBackMessage fdbck,string userLogin,string UserGuid)
        {
            var message = new MimeMessage();
       
         
            message.Subject ="[CrateMate] "+ fdbck.title;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"От {userLogin} [{UserGuid}]");
            sb.AppendLine(fdbck.descr);
            if (!string.IsNullOrEmpty(fdbck.logs))
            {
                var guide = Guid.NewGuid().ToString();
                sb.AppendLine("log tace: " + guide);
                var t = Task.Run(() =>
                {
                    try
                    {
                        if (!Directory.Exists(Log.AppDirrectory + LogPath)) Directory.CreateDirectory(Log.AppDirrectory + LogPath);
                        File.WriteAllText(Log.AppDirrectory + LogPath + "\\" + guide + ".txt", fdbck.logs);
                    }
                    catch (Exception ex) { Log.Error("Save user feedback log file", ex); }
                });
                
            }


            
            message.Body = new TextPart("plain")
            {
                Text = sb.ToString()
            };
            return message; 
        }
        public static async Task SendMail(MimeMessage message, bool sendOnAddMails = true)
        {
           await SendMail(message,DefaultReceiverEmail,sendOnAddMails);
        }
        public static async Task SendMail(MimeMessage message, MailboxAddress Receiver,bool sendOnAddMails=true,MailboxAddress Sender = null, string login= DefaultEmail, string pass = DefaultEmailPass)
        {
            if (Sender == null) Sender = DefaultSenderEmail;

            Log.Text("prepare Message On Email");
            message.From.Clear();
            message.To.Clear();
            message.From.Add(Sender);
            message.To.Add(Receiver);
         //   message.To.Add(Receiver);


            if (sendOnAddMails && false)   message.To.AddRange(AdditionalMailsForFeedBack);
            message.To.Add(new MailboxAddress("", "..."));//На хельп
            

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.mail.ru", 465, true);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(login, pass);
                    Log.Text("Sending Message On Email");
                    try
                    {
                        await client.SendAsync(message);
                        client.Disconnect(true);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex);
                    }
                    Log.Text("MessageSended");
                }
            }
            catch(Exception ex) { NotSendedMessage.Add(message); }

        }
        public enum GetMailsType
        {
            Sended,
            Received,
        }
        public static List<MimeMessage> GetAllSendedMailMessages(string login,string pass, GetMailsType type,int messageLimit)
        {
            var client = new ImapClient();


            client.Connect("imap.mail.ru", 993, true);

            try
            {
                client.Authenticate(login, pass);
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return null;
            }

            // The Inbox folder is always available on all IMAP servers...
            
            IMailFolder inbox = null;
            switch (type)
            {
                case GetMailsType.Sended:
                    try
                    {
                        inbox = client.GetFolder(SpecialFolder.Sent);
                    }

                    catch (Exception ex) { }
                    if (inbox == null)
                    {
                        var personal = client.GetFolder(client.PersonalNamespaces[0]);
                        inbox = personal.GetSubfolder("Sent Items");
                    }

                    break;
                case GetMailsType.Received:
                    inbox = client.Inbox;
                    break;
                default:
                    break;
            } 


            //if (client.Capabilities.HasFlag(ImapCapabilities.SpecialUse))
            //{
            //    inbox = client.GetFolder(SpecialFolder.Sent);
            //}
            //else
            //{
            if (inbox == null)
            {
                Log.Error("Не получилось найти выбранную папку , Возможные папки");
                Log.Json(client.PersonalNamespaces);
                return null;
            }

            inbox.Open(FolderAccess.ReadWrite);
            var res = new List<MimeMessage>();
            for(int i  = 0; i< MathF.Min(messageLimit, inbox.Count); i++)
            {
                var mess = inbox.GetMessage(i);
                Log.Text(mess.Subject+"\n"+ mess.TextBody);
                res.Add(mess);
            }
            return res;
        }

        //public static void ForwardMessage(
        //  //  string login,string pass
        //    )
        //{
        //    var client = new ImapClient();


        //    client.Connect("imap.mail.ru", 993, true);

        //    try
        //    {
        //        client.Authenticate(login, pass);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex);
        //        return ;
        //    }

        //    var inbox = client.Inbox;

        //    inbox.Open(FolderAccess.ReadWrite);

            

        //    for (int i = inbox.Count-1;  i >0; i--)
        //    {
        //        try
        //        {
        //            var msg = inbox.GetMessage(i);
        //            Log.Text(msg.From.First().Name.ToLower());
        //            if (msg.From.First().Name.ToLower().Contains("noreply@tilda.ws"))
        //            {
        //                Log.Text("RightMessage");
        //                var info = client.Inbox.Fetch(new[] { i }, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure);
        //                if (info[0].Flags.Value.HasFlag(MessageFlags.Seen))
        //                {
        //                    continue;
        //                }
        //                var bodyPart = info[0].Body;
        //                // download the 'text/plain' body part
        //                var body = (TextPart)client.Inbox.GetBodyPart(info[0].UniqueId, bodyPart);
        //                // TextPart.Text is a convenience property that decodes the content and converts the result to
        //                // a string for us
        //                var text = body.Text;

        //                var message = text;

        //                var address = string.Empty;
        //                var splt = message.Split("Фактический_адрес: ");
        //                if (splt.Length > 1) { address = splt[1].Split("<br>")[0]; goto Gt; }
        //                splt = message.Split("Юридический_адрес: ");
        //                if (splt.Length > 1) { address = splt[1].Split("<br>")[0]; goto Gt; }
        //                Log.Warning("Cant Find any address");
        //                continue;
        //            Gt:
        //                var phoneOrEmail = string.Empty;
        //                splt = message.Split("Номер_телефона: ");
        //                if (splt.Length > 1) { phoneOrEmail = splt[1].Split("<br>")[0];}                        
        //                splt = message.Split("Адрес_электронной_почты: ");
        //                if (splt.Length > 1) { phoneOrEmail +=(string.IsNullOrEmpty(phoneOrEmail)? "":" ")+ splt[1].Split("<br>")[0];}

        //                if (string.IsNullOrEmpty(phoneOrEmail)) { Log.Warning("Cant Find phoneOrEmail"); continue; }

        //                var UlName = string.Empty;  
                        
        //                splt = message.Split("Организация: ");
        //                if (splt.Length > 1) { UlName = splt[1].Split("<br>")[0];}

        //                if (string.IsNullOrEmpty(UlName)) { Log.Warning("Cant Find ULname"); continue; }


        //                var naselPunkt = string.Empty;  
                        
        //                splt = message.Split("Населённый_пункт: ");
        //                if (splt.Length > 1) { naselPunkt = splt[1].Split("<br>")[0];}

        //                if (string.IsNullOrEmpty(naselPunkt)) continue;

        //                UlName = HttpUtility.HtmlDecode(UlName);
        //                phoneOrEmail = HttpUtility.HtmlDecode(phoneOrEmail);
        //                naselPunkt = HttpUtility.HtmlDecode(naselPunkt);

        //                if (SQL.TryGetEmailFromParse(address, out var Mail, out var Tao))
        //                {
        //                    Log.Text("SQl Getted...");
        //                    FormData fd = new FormData(Tao, phoneOrEmail, UlName, naselPunkt);
                           
        //                    if (GoogleFormEmulator.SendForm(fd))
        //                    {
        //                        Log.Text("Form Sended: " + fd.ToString());
        //                        //"Населённый_пункт"
        //                        //"Юридический_адрес"
        //                        //"Фактический_адрес"
        //                        try
        //                        {
        //                            var t = SendMail(msg, Mail,true, SenderFeedBackSibEmail, DefaultEmailfeedbacksibtko, DefaultEmailfeedbacksibtkoToken);
        //                            t.Wait();
        //                            inbox.AddFlags(i, MessageFlags.Seen, true);
        //                        }
        //                        catch(Exception ex)
        //                        {
        //                            Log.Error(ex);
        //                        }
                                
        //                    }
        //                    else
        //                    {
        //                        Log.Warning("Cant Send GoogleForm");
        //                    }
        //                  //  break;
        //                }
        //            }
        //        }
        //        catch(Exception ex)
        //        {
        //            Log.Error(ex);
        //        }
        //    }

            
        //}
    }
}
