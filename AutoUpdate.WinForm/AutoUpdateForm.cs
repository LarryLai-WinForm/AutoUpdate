using System;
using System.Windows.Forms;

namespace AutoUpdate.WinForm
{
    using modules;

    public partial class AutoUpdateForm : Form
    {
        private void MsgAdd(string msg)
        {
            backgroundWorker_Update.ReportProgress(0, msg);
        }
        private void ProcessResult(AutoUpdate.Result res)
        {
            switch (res)
            {
                case AutoUpdate.Result.Newest:
                    Close();
                    return;

                case AutoUpdate.Result.Success:
                    break;

                case AutoUpdate.Result.Fail:
                default:
                    MessageBox.Show(this,"Automatic update failed, please contact the program developer");
                    button_Close.Enabled = true;
                    return;
            }
        }

        FTP Ftp { set; get; }
        public AutoUpdateForm(FTP fTP)
        {
            InitializeComponent();

            Ftp = fTP;
        }

        private void AutoUpdateForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            backgroundWorker_Update.RunWorkerAsync();
        }

        private void BackgroundWorker_Update_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            AutoUpdate autoUpdate = new AutoUpdate(Ftp);
            autoUpdate.MsgAddEvent_Func += MsgAdd;
            autoUpdate.ResultEvent_Func += ProcessResult;
            autoUpdate.Run();
        }

        private void BackgroundWorker_Update_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            textBox_MSG.Text += e.UserState.ToString();
        }

        private void Button_Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TextBox_MSG_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.ScrollBars = ScrollBars.Vertical;
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }
    }

}

#region Example AutoUpdateForm

//new AutoUpdateForm(new FTP()
//{
//    UrlString = "",
//    UserName = "",
//    Password = "",
//    Path = "",
//}).ShowDialog();

#endregion
