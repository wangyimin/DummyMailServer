using System;
using System.Collections.Generic;

namespace DummyMailServer
{
    public class Commands
    {
        private readonly Dictionary<string, string> commands = new Dictionary<string, string>()
        {
            { "EHLO", "250-AUTH LOGIN\r\n250-AUTH=LOGIN\r\n250 OK\r\n"},
            { "AUTH LOGIN", "334 VXNlcm5hbWU6\r\n"}, { "USER", "334 UGFzc3dvcmQ6\r\n"}, { "PASS", "235 AUTH OK\r\n"},
            { "MAIL FROM", "250 MAIL FROM\r\n"}, { "RCPT TO", "250 RCPT TO\r\n"},
            { "DATA", "354 DATA START\r\n"}, { ".", "250 OK\r\n" },
            { "QUIT", "250 BYE\r\n"}
        };

        private string prev = String.Empty;

        public (bool, string, string) TryExecute(string data)
        {
            bool r = false; 
            string resp = String.Empty;

            if (prev.Equals("AUTH LOGIN"))
                (r, prev, resp) = (true, "USER", commands["USER"]);
            else if (prev.Equals("USER"))
                (r, prev, resp) = (true, "PASS", commands["PASS"]);
            else if (prev.Equals("DATA") && data.EndsWith("\r\n.\r\n"))
                (r, prev, resp) = (true, ".", commands["."]);
            else
                foreach (var k in commands.Keys)
                    if (data.StartsWith(k))
                    {
                        (r, prev, resp) = (true, k, commands[k]);
                        break;
                    }

            return r ? (r, prev, resp) : (false, String.Empty, String.Empty);
        }
    }
}
