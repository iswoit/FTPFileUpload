using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.ComponentModel;

namespace FtpFileUpLoad
{
    public class UploadFile
    {
        private string _source;
        private string _dest;
        private string _rename;
        private bool _isOK = false;

        private bool _isRunning = false;
        private float _percent = 0.0f;

        public UploadFile(FTP ftp, string source, string dest, string rename)
        {
            _source = Util.Filename_Date_Convert(source.Trim());
            _dest = Util.Filename_Date_Convert(dest.Trim());
            _rename = Util.Filename_Date_Convert(rename.Trim());

            // ftp.server与dest合并形成路径
            if (!_dest.StartsWith(@"/"))
                _dest = @"/" + _dest;
            if (!_dest.EndsWith(@"/"))
                _dest = _dest + @"/";

            _dest = string.Format("{0}{1}", ftp.Server, _dest);

            // 接下来附加文件名
            if (string.IsNullOrEmpty(_rename))
                _dest = string.Format("{0}{1}", _dest, Path.GetFileName(_source));
            else
                _dest = string.Format("{0}{1}", _dest, _rename);

            _isOK = false;
        }



        /// <summary>  
        /// 上传文件  
        /// </summary>  
        /// <param name="localFile">要上传到FTP服务器的本地文件</param>  
        /// <param name="ftpPath">FTP地址</param>  
        public void UpLoadFile(string ftpUserID, string ftpPassword, BackgroundWorker bgWorker)
        {

            FileInfo fileInf = new FileInfo(this.Source);
            FtpWebRequest reqFTP;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(this.Dest);// 根据uri创建FtpWebRequest对象   
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);// ftp用户名和密码  
            reqFTP.KeepAlive = false;// 默认为true，连接不会被关闭 // 在一个命令之后被执行  
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;// 指定执行什么命令  
            reqFTP.UseBinary = true;// 指定数据传输类型  
            reqFTP.ContentLength = fileInf.Length;// 上传文件时通知服务器文件的大小  
            int buffLength = (int)fileInf.Length/100;// 缓冲大小设置为2kb  
            if (buffLength <= 0)
                buffLength = 1;
            byte[] buff = new byte[buffLength];
            int contentLen;     // 每次读取的大小
            int contentLenAccumulate = 0;   // 累计读取大小，用于计算百分比

            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件  
            FileStream fs = null;
            Stream strm = null;
            try
            {
                fs = fileInf.OpenRead();                // 读文件流
                strm = reqFTP.GetRequestStream();       // 写FTP流
                contentLen = fs.Read(buff, 0, buffLength);// 每次读文件流的2kb  
                
                

                while (contentLen != 0)// 流内容没有结束  
                {
                    // 把内容从file stream 写入 upload stream  
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);

                    contentLenAccumulate += contentLen;
                    _percent = (float)contentLenAccumulate / fileInf.Length;
                    bgWorker.ReportProgress(1);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                // 关闭两个流
                if (strm != null)
                    strm.Close();

                if (fs != null)
                    fs.Close();
            }
        }


        /// <summary>
        /// 源路径是否可访问
        /// </summary>
        /// <returns></returns>
        public bool IsSourcePathAvailable()
        {
            if (File.Exists(this._source))
                return true;
            else
                return false;
        }

        public string Source
        {
            get { return _source; }
        }

        public string Dest
        {
            get { return _dest; }
        }

        public bool IsOK
        {
            get { return _isOK; }
            set { _isOK = value; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public float Percent
        {
            get { return _percent; }
            set { _percent = value; }
        }
    }
}
