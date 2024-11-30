using System.Net;
using System.Net.Mail;

namespace CatholicHouseholdBook.Common.Helpers
{
    public class XMail
    {
        public enum SendResultEnum : int
        {
            Failed = 0,
            MailboxBusy = 1,
            Successful = 2,
            MailboxUnavailable = 3
        }
        public static SendResultEnum Send(String to, String subject, String body)
        {
            String cc = "";
            String bcc = "";
            String attachments = "";
            return Send(to, cc, bcc, subject, body, attachments);
        }
        public static SendResultEnum Send(String to, String cc, String bcc, String subject, String body, String attachments)
        {
            string mailAccount = "admin@riolish.vn";
            string mailPassword = "P@ssword1234567";
            var statusSent = SendResultEnum.Failed;

            bool flag = false;
            try
            {
                var senderEmail = new MailAddress(mailAccount, "Ruviteks");
                var receiverEmail = new MailAddress(to, "Receiver");

                //=== Công cụ kiểm tra Email: https://www.gmass.co/smtp-test ===//
                using (var smtp = new SmtpClient
                {
                    Host = "mail93163.maychuemail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(mailAccount, mailPassword)
                })
                {
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var message = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                        Priority = MailPriority.High
                    })
                    {
                        smtp.Send(message);
                    }
                    statusSent = SendResultEnum.Successful;
                    flag = true;
                }
            }
            catch (SmtpException ex)
            {
                SmtpStatusCode status = ex.StatusCode;
                if (status == SmtpStatusCode.MailboxBusy)
                {
                    //=== Hộp thư đang bận thì phải chờ, chứ gửi ép thì Mail Server khóa luôn cả đám ===//
                    System.Threading.Thread.Sleep(5000);
                    //smtp.Send(message);
                    statusSent = SendResultEnum.MailboxBusy;
                }
                else if (status == SmtpStatusCode.MailboxUnavailable)
                {
                    statusSent = SendResultEnum.MailboxUnavailable;
                }
                else
                {
                    //Console.WriteLine("Failed to deliver message to {0}",
                    //    ex.InnerExceptions[i].FailedRecipient);
                }
            }
            catch (Exception ex)
            {
                //var message = MessageBox.Show(e.ToString());
            }
            return statusSent;
        }
        public static string TokenCode(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var TokenCode = new String(stringChars);
            return TokenCode;
        }

        public static string GetEmailHtmlBody(string activationCode = "RIO-VN9")
        {
            return string.Format(
                @"<div style={0} display:flex; justify-content:center;width:100%{0}>
                    <div style={0}text-align:justify; width: 700px; border: solid 1px #007BFF; border-radius: 3px;{0}>
                        <h1 style={0}text-align: center; color: white; background-color: #007BFF; font-family: Georgia{0}>Thank you for using Riolish</h1>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype'; margin:10px;{0}>
                            <span style={0}font-weight:bold;{0}>Bước 1: </span>
                            Tải ứng dụng Riolish về điện thoại và đăng ký 01 tài khoản chính thức
                        </div>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype';margin:10px;{0}>
                            <span style={0}font-weight:bold;{0}>Bước 2: </span>
                            Xác nhận địa chỉ Email với Mã kích hoạt được gửi qua email đăng ký
                        </div>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype'; margin:10px;{0}>
                            <span style={0}font-weight:bold;{0}>Bước 3: </span>
                            Click vào biểu tượng chú vẹt (góc phải trên cùng của ứng dụng), chọn tiếp biểu tượng hình người và chọn tiếp tính năng Đăng ký thời hạn sử dụng
                        </div>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype'; margin:10px;{0}>
                            <span style={0}font-weight:bold;{0}>Bước 4: </span>
                            Nhập mã số bên dưới để kích hoạt 12 tháng sử dụng và bấm Áp dụng
                        </div>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype'; margin:10px;{0}>
                            (Không thực hiện chức năng thanh toán)
                        </div>
                        <div style={0}color: #007BFF; font-family: 'Palatino Linotype'; font-weight: 'bold'; margin:10px;{0}>
                            Mã kích hoạt của bạn là:
                        </div>
                        <h1 style={0}text-align: center; color: white; background-color: #007BFF; font-family: Georgia{0}>{1}</h1>
                        <div style={0}text-align: center; color: #007BFF; font-family: 'Palatino Linotype'; font-weight: bold;{0}>
                            Học Tiếng Anh với 45 phút mỗi ngày
                        </div>
                    </div>
                </div>", "\"", activationCode);
        }

        
    }
}
