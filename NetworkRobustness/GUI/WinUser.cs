using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation.Lib;
using System.Windows.Forms;
using System.Threading;


namespace NetSimulation
{
    public class WinUser : User
    {
        MainForm mainForm = Program.mainform;
        public WinUser()
        {
            User.One = this;
        }
        public override void BeginWait(string reason, int maximumStep, int minimumStep=0)
        {
            mainForm.pbMain.Minimum = minimumStep;
            mainForm.pbMain.Maximum = maximumStep;
            mainForm.labprogress.Text = reason;
        }
        
        public override void EndWait(string reason)
        {
            mainForm.labprogress.Text = "";
        }
        public override void ShowWaitIndicator(int atStep, int totalStep)
        {
            mainForm.pbMain.Step = atStep;
        }
        public override void SendCalculationResult(string Result)
        {
 
        }
        private delegate void MessageToUserDelegate(String message);
        public override void MessageToUser(string strMessage)
        {
            if (mainForm.txtmsgboard.InvokeRequired)
            {
                mainForm.Invoke(new MessageToUserDelegate(MessageToUser), new object[] { strMessage });
                return;
            }

            mainForm.txtmsgboard.AppendText("\n\n"+strMessage+"\n");
        }

        public override void SendErrorToUser(Exception ex)
        {
            MessageBox.Show(ex.Message, "An error happens!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public override YesNoQuestion AskUserYesNoQuestion(string strMessage)
        {
            return MessageBox.Show(strMessage, "Ask a question", MessageBoxButtons.YesNo)==DialogResult.Yes?
                YesNoQuestion.Yes:YesNoQuestion.No;
            
        }
        public override YesNoCancelQuestion AskUserYesNoCancelQuestion(string strMessage)
        {
            DialogResult dlgr= MessageBox.Show(strMessage, "Ask a question", MessageBoxButtons.YesNoCancel);
            if (dlgr == DialogResult.Yes)
                return YesNoCancelQuestion.Yes;
            else if (dlgr == DialogResult.No)
                return YesNoCancelQuestion.No;
            else
                return YesNoCancelQuestion.Cancel;
        }
        public override YesNoQuestion AskUserAnValue(string Comment, string Prompt, System.Type type, object DefaultValue, ref object Return)
        {
            TextBoxForm tbf = new TextBoxForm();
            tbf.labComment.Text = Comment;
            tbf.txtPrompt.Text = Prompt;
            tbf.txtEdit.Text = Convert.ToString(Uti.CheckNull(DefaultValue,""));
            if (tbf.ShowDialog() == DialogResult.OK)
            {
                Return = Convert.ChangeType(tbf.txtEdit.Text, type);
                return YesNoQuestion.Yes;
            }
            return YesNoQuestion.No;
        }
        public override void Clear()
        {
 
        }
        public override void PressAnyKey()
        {
            
        }
        public override bool LockWritingMessage()
        {
            return false;
        }
    }

}