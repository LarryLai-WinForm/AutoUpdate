using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoUpdate.Form
{

    abstract public partial class AutoUpdateForm : System.Windows.Forms.Form
    {
        public static void Show<AutoUpdateForm_T>() where AutoUpdateForm_T : AutoUpdateForm, new()
        {
            new AutoUpdateForm_T().ShowDialog();
        }
        protected abstract AutoUpdateClass.Info Info { get; }
        protected virtual AutoUpdateClass.Info.Directory_Info Directory_Info { get; }

        private void msgAdd(string msg)
        {
            backgroundWorker_Update.ReportProgress(0, msg);
        }
        private void result(AutoUpdateClass.Result res)
        {
            switch (res)
            {
                case AutoUpdateClass.Result.Newest:
                    Close();
                    return;

                case AutoUpdateClass.Result.Success:
                    break;

                case AutoUpdateClass.Result.Fail:
                default:
                    MessageBox.Show("Automatic update failed, please contact the program developer");
                    button_Close.Enabled = true;
                    return;
            }
        }


        public AutoUpdateForm()
        {
            InitializeComponent();
        }

        private void AutoUpdateForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            backgroundWorker_Update.RunWorkerAsync();
        }

        private void backgroundWorker_Update_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            AutoUpdateClass autoUpdate = new AutoUpdateClass(Info);
            autoUpdate.msgAddEvent += msgAdd;
            autoUpdate.resultEvent += result;

            autoUpdate.Run();
        }

        private void backgroundWorker_Update_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            textBox_MSG.Text += e.UserState.ToString();
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox_MSG_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.ScrollBars = ScrollBars.Vertical;
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }
    }

    #region Example AutoUpdateForm

    public class Example_AutoUpdateForm : AutoUpdateForm
    {
        sealed protected override AutoUpdateClass.Info Info
        {
            get
            {
                AutoUpdateClass.Info info = new AutoUpdateClass.Info
                {
                    ftpInfo = new AutoUpdateClass.Info.FtpInfo()
                    {
                        UrlString = "ftp://xxx.xxx.xxx.xxx/",
                        UserName = "user name",
                        Password = "password"
                    },

                    directory_Info = new AutoUpdateClass.Info.Directory_Info()
                    {
                        Directories = new Dictionary<string, AutoUpdateClass.Info.Directory_Info>()
                        {
                            {
                                ".\\directory\\",new AutoUpdateClass.Info.Directory_Info()
                                {
                                    Directories = new Dictionary<string, AutoUpdateClass.Info.Directory_Info>()
                                    {
                                        {
                                            ".\\directory\\123\\",new AutoUpdateClass.Info.Directory_Info()
                                            {
                                                Directories = new Dictionary<string, AutoUpdateClass.Info.Directory_Info>(){ },
                                                Files = new List<string>()
                                                {
                                                    ".\\directory\\123\\" + "filename1",
                                                    ".\\directory\\123\\" + "filename2"
                                                }
                                            }
                                        },
                                    },

                                    Files = new List<string>()
                                    {
                                        ".\\directory\\" + "filename1",
                                        ".\\directory\\" + "filename2",
                                    }
                                }
                            },

                        },

                        Files = new List<string>()
                        {
                            "filename1",
                            "filename2",
                            "filename3",
                        },
                    },

            };

                return info;
            }
        }
    }

    /// Call AutoUpdateForm.Show<Example_AutoUpdateForm>() at the beginning of Form_Load;

    #endregion

}
