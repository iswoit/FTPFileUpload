using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace FtpFileUpLoad
{
    public partial class FrmMain : Form
    {
        Manager _manager;

        public FrmMain()
        {
            InitializeComponent();

            try
            {
                _manager = new Manager();
                LvFileInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void LvFileInit()
        {
            lvFile.Items.Clear();
            for (int i = 0; i < _manager.UploadFiles.Count; i++)
            {
                ListViewItem lvi = new ListViewItem((i + 1).ToString());
                lvi.SubItems.Add(_manager.UploadFiles[i].Source);
                lvi.SubItems.Add(_manager.UploadFiles[i].Dest);
                lvi.SubItems.Add(_manager.UploadFiles[i].IsOK ? "√" : "×");
                lvi.Tag = _manager.UploadFiles[i];

                lvFile.Items.Add(lvi);
            }
        }

        private void LvFileUpdate()
        {
            // 进度列表
            try
            {
                for (int i = 0; i < _manager.UploadFiles.Count; i++)
                {
                    UploadFile tmpFile = (UploadFile)lvFile.Items[i].Tag;   // 配置对象

                    string strStatus = string.Empty;
                    if (tmpFile.IsRunning)
                    {
                        strStatus = string.Format("{0:0}%", tmpFile.Percent * 100);
                    }
                    else
                    {
                        if (tmpFile.IsOK)
                            strStatus = "√";
                        else
                            strStatus = "×";
                    }

                    lvFile.Items[i].SubItems[3].Text = strStatus;             // 状态

                }
            }
            catch (Exception)
            {
                // ui异常过滤
            }

        }


        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (!bwUpload.IsBusy)
            {
                bwUpload.RunWorkerAsync();
                btnUpload.Text = "上传中...";
            }
            else
            {
                bwUpload.CancelAsync();
                btnUpload.Text = "上传";
            }
        }


        private void bwUpload_DoWork(object sender, DoWorkEventArgs e)
        {
            /* 0.所有重置为未完成
             * 1.源文件是否存在
             * 2.上传
             */

            try
            {

                BackgroundWorker bgWorker = sender as BackgroundWorker;
                _manager.ResetStatus();
                bgWorker.ReportProgress(1);

                int idx = 0;
                foreach (UploadFile tmpUploadFile in _manager.UploadFiles)
                {
                    idx++;

                    if (true == bgWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }


                    // 1.源文件存在
                    bool isSourcePathAvailable = tmpUploadFile.IsSourcePathAvailable();
                    if (!isSourcePathAvailable)
                    {
                        tmpUploadFile.IsOK = false;
                        UserState us = new UserState(true, string.Format("第{0}行任务出错: 源文件不存在", idx));
                        bgWorker.ReportProgress(1, us);

                        continue;
                    }


                    // 2.上传
                    try
                    {
                        tmpUploadFile.IsRunning = true;
                        bgWorker.ReportProgress(1);

                        tmpUploadFile.UpLoadFile(_manager.Ftp.Acc, _manager.Ftp.Pwd, bgWorker);

                        tmpUploadFile.IsRunning = false;
                        tmpUploadFile.IsOK = true;
                        UserState us = new UserState(true, string.Format("第{0}行{1}上传完成", idx, tmpUploadFile.Source));
                        bgWorker.ReportProgress(1, us);

                    }
                    catch (Exception ex)
                    {
                        tmpUploadFile.IsOK = false;
                        UserState us = new UserState(true, string.Format("第{0}行{1}任务出错: {2}", idx, tmpUploadFile.Source, ex.Message));
                        bgWorker.ReportProgress(1, us);
                    }
                }



                e.Result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void bwUpload_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                UserState us = (UserState)e.UserState;
                if (us.HasError)
                    Print_Error_Message(us.ErrorMsg);
            }


            try
            {
                LvFileUpdate();
            }
            catch (Exception ex)
            {
                Print_Error_Message(ex.Message);
            }
        }

        private void bwUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)    // 未处理的异常，需要弹框
            {
                Print_Error_Message(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Print_Error_Message("任务被手工取消");
            }
            else
            {
            }

            if (_manager.IsAllOK)
            {
                Print_Error_Message("任务完成");
            }
            else
            {
                Print_Error_Message("任务失败");
            }


            btnUpload.Text = "上传";
        }

        private void Print_Error_Message(string message)
        {
            tbLog.Text = string.Format("{0}:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message) + System.Environment.NewLine + tbLog.Text;

            // 写到TXT

            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "log.txt"), FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("{0}:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));//开始写入值
                    }
                }
            }
            catch (Exception ex)
            { }

        }

    }
}
