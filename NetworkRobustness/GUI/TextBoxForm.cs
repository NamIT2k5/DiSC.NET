using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NetSimulation
{
	
	/// <summary>
	/// Summary description for TextBoxForm.
	/// </summary>
	public class TextBoxForm : Form
	{
		private System.Windows.Forms.Button bntOK;
		public TextBoxEnter txtEdit;
		public System.Windows.Forms.Label txtPrompt;
		private System.Windows.Forms.Button bntCancel;
        public Label labComment;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TextBoxForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.bntOK = new System.Windows.Forms.Button();
            this.txtPrompt = new System.Windows.Forms.Label();
            this.bntCancel = new System.Windows.Forms.Button();
            this.txtEdit = new NetSimulation.TextBoxEnter();
            this.labComment = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bntOK
            // 
            this.bntOK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bntOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bntOK.Location = new System.Drawing.Point(192, 107);
            this.bntOK.Name = "bntOK";
            this.bntOK.Size = new System.Drawing.Size(75, 23);
            this.bntOK.TabIndex = 1;
            this.bntOK.Text = "&OK";
            this.bntOK.Click += new System.EventHandler(this.bntOK_Click);
            // 
            // txtPrompt
            // 
            this.txtPrompt.Location = new System.Drawing.Point(8, 55);
            this.txtPrompt.Name = "txtPrompt";
            this.txtPrompt.Size = new System.Drawing.Size(344, 20);
            this.txtPrompt.TabIndex = 2;
            this.txtPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bntCancel
            // 
            this.bntCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bntCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bntCancel.Location = new System.Drawing.Point(280, 107);
            this.bntCancel.Name = "bntCancel";
            this.bntCancel.Size = new System.Drawing.Size(75, 23);
            this.bntCancel.TabIndex = 2;
            this.bntCancel.Text = "&Cancel";
            // 
            // txtEdit
            // 
            this.txtEdit.Location = new System.Drawing.Point(8, 75);
            this.txtEdit.MaxLength = 64;
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.Size = new System.Drawing.Size(344, 20);
            this.txtEdit.TabIndex = 0;
            // 
            // labComment
            // 
            this.labComment.Location = new System.Drawing.Point(4, 8);
            this.labComment.Name = "labComment";
            this.labComment.Size = new System.Drawing.Size(344, 35);
            this.labComment.TabIndex = 2;
            this.labComment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TextBoxForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.bntCancel;
            this.ClientSize = new System.Drawing.Size(360, 136);
            this.Controls.Add(this.labComment);
            this.Controls.Add(this.txtPrompt);
            this.Controls.Add(this.txtEdit);
            this.Controls.Add(this.bntOK);
            this.Controls.Add(this.bntCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TextBoxForm";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.TextBoxForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void TextBoxForm_Load(object sender, System.EventArgs e)
		{
			txtEdit.OnKeyDownEnter=new TextBoxEnter.KEY_DOWN_ENTER(bntOK_Click);
//			txtEdit.Select();
		}

		private void bntOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult=DialogResult.OK;
		}

		
	}
	public class TextBoxEnter: TextBox
	{
		public delegate void KEY_DOWN_ENTER(object sender, System.EventArgs e);
		public KEY_DOWN_ENTER OnKeyDownEnter=null;
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(e.KeyCode==Keys.Enter)
			{
				if(OnKeyDownEnter!=null)
				{
					OnKeyDownEnter(null,null);
				}
			}
			base.OnKeyDown (e);
		}
	}
}
