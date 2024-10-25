using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWHEmail
{
    public class EmailData
    {
        [SMTPCommand("MAIL")]
        public string Sender { get; set; }

        [SMTPCommand("RCPT")]
        public string Recipient { get; set; }

        [SMTPCommand("SUBJECT")]
        public string Subject { get; set; }

        [SMTPCommand("BODY")]
        public string Body { get; set; }

        [SMTPCommand("QUIT")]
        public string Quit { get; set; }


    }
}
