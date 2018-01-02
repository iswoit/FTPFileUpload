using System;
using System.Collections.Generic;
using System.Text;

namespace FtpFileUpLoad
{
    public class FTP
    {
        private string _server;
        private string _acc;
        private string _pwd;

        public FTP(string server, string acc, string pwd)
        {
            _server = server.Trim();
            if (_server.EndsWith(@"/"))
                _server = _server.Substring(0, _server.Length - 1);

            _acc = acc.Trim();
            _pwd = pwd.Trim();
        }

        public string Server
        {
            get { return _server; }
        }

        public string Acc
        {
            get { return _acc; }
        }

        public string Pwd
        {
            get { return _pwd; }
        }
    }
}
