using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FtpFileUpLoad
{
    public class Manager
    {
        private List<UploadFile> _uploadFiles = new List<UploadFile>();
        private FTP ftp = null;

        public Manager()
        {
            _uploadFiles = new List<UploadFile>();

            // 判断配置文件是否存在，不存在抛出异常
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "cfg.xml")))
                throw new Exception("未能找到配置文件cfg.xml，请重新配置该文件后重启程序!");

            // 读取文件
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;     //忽略文档里面的注释
            using (XmlReader reader = XmlReader.Create(@"cfg.xml", settings))
            {
                doc.Load(reader);

                // 根节点检查
                XmlNode rootNode = doc.SelectSingleNode("config");   // 根节点
                if (rootNode == null)
                    throw new Exception("无法找到配置文件的根节点<config>，请检查配置文件格式是否正确!");


                // 找ftp节点
                XmlNode nodeFtp = rootNode.SelectSingleNode("//ftp");
                if (nodeFtp != null)
                {
                    string server = string.Empty;
                    string acc = string.Empty;
                    string pwd = string.Empty;

                    foreach (XmlNode tmpNode in nodeFtp.ChildNodes)
                    {
                        switch (tmpNode.Name.Trim().ToLower())
                        {
                            case "server":
                                server = tmpNode.InnerText;
                                break;
                            case "acc":
                                acc = tmpNode.InnerText;
                                break;
                            case "pwd":
                                pwd = tmpNode.InnerText;
                                break;
                        }
                    }


                    if (string.IsNullOrEmpty(server))
                        throw new Exception("节点<server>丢失，无法指定FTP服务器，请检查!");
                    if (string.IsNullOrEmpty(acc))
                        throw new Exception("节点<acc>丢失，无法确定FTP账号，请检查!");
                    if (string.IsNullOrEmpty(pwd))
                        throw new Exception("节点<pwd>丢失，无法确定FTP密码，请检查!");

                    ftp = new FTP(server, acc, pwd);
                }
                else
                {
                    throw new Exception("无法找到配置文件的节点<ftp>，请检查配置文件格式是否正确!");
                }


                // 找文件节点
                XmlNode nodeFiles = rootNode.SelectSingleNode("//files");
                if (nodeFiles != null)
                {
                    foreach (XmlNode tmpNode in nodeFiles.ChildNodes)
                    {
                        if (tmpNode.Name.Trim().ToLower() == "file")
                        {
                            string source = string.Empty;
                            string dest = string.Empty;
                            string rename = string.Empty;

                            foreach (XmlNode tmpChildNode in tmpNode.ChildNodes)
                            {
                                switch (tmpChildNode.Name.Trim().ToLower())
                                {
                                    case "source":
                                        source = tmpChildNode.InnerText.Trim();
                                        break;
                                    case "dest":
                                        dest = tmpChildNode.InnerText.Trim();
                                        break;
                                    case "rename":
                                        rename = tmpChildNode.InnerText.Trim();
                                        break;
                                }
                            }


                            if (string.IsNullOrEmpty(source))
                                throw new Exception("节点<file>的子节点<source>丢失请检查!");
                            if (string.IsNullOrEmpty(dest))
                                throw new Exception("节点<file>的子节点<dest>丢失请检查!");


                            UploadFile uploadFile = new UploadFile(ftp, source, dest, rename);
                            _uploadFiles.Add(uploadFile);
                        }
                    }
                }
            }
        }

        public List<UploadFile> UploadFiles
        {
            get { return _uploadFiles; }
        }

        public FTP Ftp
        {
            get { return ftp; }
        }


        public void ResetStatus()
        {
            foreach (UploadFile tmpFile in _uploadFiles)
                tmpFile.IsOK = false;
        }

        public bool IsAllOK
        {
            get {
                foreach(UploadFile tmpFile in _uploadFiles)
                {
                    if (tmpFile.IsOK == false)
                        return false;
                }
                return true;
            }
        }
    }
}
