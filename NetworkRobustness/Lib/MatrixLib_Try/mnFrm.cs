//------------------------------------------------------------------------
//
// Author      : Anas Abidi
// Date        : 18 Dec 2004
// Version     : 2.0
// Description : Matrix Operations Library
//
//------------------------------------------------------------------------	
//
//   Matrix Library v2.0, this Library contains class Matrix which 
// provides many static methods for making various matrix operations on
// objects derived from the class or on arrays defined as double of any
// dimension. The cMathLib v2.0 class is an enhancement to a previous 
// submission to planet-source-code. The previous code was written in
// VB.Net, due to popularity it received I decided to submit this newer
// version. This version is written in C#.net, and contains a lot of 
// enhancements to the previous code plus newer methods for matrix 
// manipulations. 
//   Some Methods included in this library are: add, subtract, 
// multiply, Transpose, Determinant, Inverse, LU decomposition, Eigen
// Values and Vectors, Pseudoinverse,  Singular Value Decomposition ,
// SolveLinear Equations, Rank, Dot Product, Cross Product etc... 

//   Also The '+','-','*' operators are overloaded to work with the 
// objects derived from the matrix class. The code used in this class is
// highly optimized for speed. Errors are handled as exceptions. 
// The whole class is documented and I also provided a help file describing
// the class. If you compile the class as a ‘dll’ it can be easily used in 
// other .Net languages easily. 
//   A lot of hard work was put into the developing of this class, the 
// reason for sharing such a work for free, is my belief in sharing
// good work and free source code. As an appreciation of my efforts I
// would really appreciate your votes and my reference when you use this
// class, so I share more of my work with you in future.

// The following shows a demonstration on how to use the library.
// In this example I just added the cMathLib class and then defined it 
// with a variable in the program
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using MatrixLibrary;

namespace Matrix_Lib
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class mnFrm : System.Windows.Forms.Form
	{
		// define a matrix A with dimensions (4x4)
		Matrix A = new Matrix(4,4);

		// define a matrix B with dimensions (4x4)
		Matrix B = new Matrix(4, 4);

		// define a vector V1
		Matrix V1 = new Matrix(3,1);

		// define a vector V2
		Matrix V2 = new Matrix(3,1);

		#region windows public methods
		public System.Windows.Forms.GroupBox FrmSelect;
		public System.Windows.Forms.Label Label12;
		public System.Windows.Forms.RadioButton Option3;
		public System.Windows.Forms.RadioButton Option7;
		public System.Windows.Forms.RadioButton Option6;
		public System.Windows.Forms.RadioButton Option5;
		public System.Windows.Forms.RadioButton Option4;
		public System.Windows.Forms.RadioButton Option2;
		public System.Windows.Forms.RadioButton Option1;
		public System.Windows.Forms.Label Label11;
		public System.Windows.Forms.Label Label10;
		public System.Windows.Forms.Label Label9;
		public System.Windows.Forms.Label Label8;
		public System.Windows.Forms.Label Label7;
		public System.Windows.Forms.Label Label6;
		public System.Windows.Forms.Label Label5;
		public System.Windows.Forms.Label Label4;
		public System.Windows.Forms.Label Label3;
		public System.Windows.Forms.Label Label2;
		public System.Windows.Forms.Label Label1;
		public System.Windows.Forms.Button CalButton;
		public System.Windows.Forms.RichTextBox txtDisplay;
		public System.Windows.Forms.RichTextBox txtSolution;
		#endregion
		public System.Windows.Forms.RadioButton Option13;
		public System.Windows.Forms.RadioButton Option12;
		public System.Windows.Forms.RadioButton Option11;
		public System.Windows.Forms.RadioButton Option10;
		public System.Windows.Forms.RadioButton Option9;
		public System.Windows.Forms.RadioButton Option8;
		public System.Windows.Forms.Label label13;
		public System.Windows.Forms.RadioButton Option14;
		public System.Windows.Forms.Label label14;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public mnFrm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.FrmSelect = new System.Windows.Forms.GroupBox();
			this.CalButton = new System.Windows.Forms.Button();
			this.Label12 = new System.Windows.Forms.Label();
			this.Option3 = new System.Windows.Forms.RadioButton();
			this.Option13 = new System.Windows.Forms.RadioButton();
			this.Option12 = new System.Windows.Forms.RadioButton();
			this.Option11 = new System.Windows.Forms.RadioButton();
			this.Option10 = new System.Windows.Forms.RadioButton();
			this.Option9 = new System.Windows.Forms.RadioButton();
			this.Option7 = new System.Windows.Forms.RadioButton();
			this.Option6 = new System.Windows.Forms.RadioButton();
			this.Option5 = new System.Windows.Forms.RadioButton();
			this.Option4 = new System.Windows.Forms.RadioButton();
			this.Option2 = new System.Windows.Forms.RadioButton();
			this.Option1 = new System.Windows.Forms.RadioButton();
			this.Label11 = new System.Windows.Forms.Label();
			this.Label10 = new System.Windows.Forms.Label();
			this.Label9 = new System.Windows.Forms.Label();
			this.Label8 = new System.Windows.Forms.Label();
			this.Label7 = new System.Windows.Forms.Label();
			this.Label6 = new System.Windows.Forms.Label();
			this.Label5 = new System.Windows.Forms.Label();
			this.Label4 = new System.Windows.Forms.Label();
			this.Label3 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.txtDisplay = new System.Windows.Forms.RichTextBox();
			this.txtSolution = new System.Windows.Forms.RichTextBox();
			this.Option8 = new System.Windows.Forms.RadioButton();
			this.label13 = new System.Windows.Forms.Label();
			this.Option14 = new System.Windows.Forms.RadioButton();
			this.label14 = new System.Windows.Forms.Label();
			this.FrmSelect.SuspendLayout();
			this.SuspendLayout();
			// 
			// FrmSelect
			// 
			this.FrmSelect.BackColor = System.Drawing.SystemColors.Control;
			this.FrmSelect.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.Option14,
																					this.label14,
																					this.Option8,
																					this.label13,
																					this.CalButton,
																					this.Label12,
																					this.Option3,
																					this.Option13,
																					this.Option12,
																					this.Option11,
																					this.Option10,
																					this.Option9,
																					this.Option7,
																					this.Option6,
																					this.Option5,
																					this.Option4,
																					this.Option2,
																					this.Option1,
																					this.Label11,
																					this.Label10,
																					this.Label9,
																					this.Label8,
																					this.Label7,
																					this.Label6,
																					this.Label5,
																					this.Label4,
																					this.Label3,
																					this.Label2,
																					this.Label1});
			this.FrmSelect.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FrmSelect.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FrmSelect.Location = new System.Drawing.Point(408, 0);
			this.FrmSelect.Name = "FrmSelect";
			this.FrmSelect.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FrmSelect.Size = new System.Drawing.Size(105, 456);
			this.FrmSelect.TabIndex = 2;
			this.FrmSelect.TabStop = false;
			// 
			// CalButton
			// 
			this.CalButton.BackColor = System.Drawing.SystemColors.Control;
			this.CalButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CalButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.CalButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CalButton.Location = new System.Drawing.Point(8, 392);
			this.CalButton.Name = "CalButton";
			this.CalButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CalButton.Size = new System.Drawing.Size(89, 48);
			this.CalButton.TabIndex = 38;
			this.CalButton.Text = "Calculate";
			this.CalButton.Click += new System.EventHandler(this.CalButton_Click);
			// 
			// Label12
			// 
			this.Label12.BackColor = System.Drawing.SystemColors.Control;
			this.Label12.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label12.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label12.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label12.Location = new System.Drawing.Point(24, 80);
			this.Label12.Name = "Label12";
			this.Label12.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label12.Size = new System.Drawing.Size(56, 25);
			this.Label12.TabIndex = 37;
			this.Label12.Text = " A x B";
			// 
			// Option3
			// 
			this.Option3.BackColor = System.Drawing.SystemColors.Control;
			this.Option3.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option3.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option3.Location = new System.Drawing.Point(8, 80);
			this.Option3.Name = "Option3";
			this.Option3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option3.Size = new System.Drawing.Size(17, 17);
			this.Option3.TabIndex = 36;
			this.Option3.Text = "Option1";
			// 
			// Option13
			// 
			this.Option13.BackColor = System.Drawing.SystemColors.Control;
			this.Option13.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option13.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option13.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option13.Location = new System.Drawing.Point(8, 320);
			this.Option13.Name = "Option13";
			this.Option13.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option13.Size = new System.Drawing.Size(17, 17);
			this.Option13.TabIndex = 35;
			this.Option13.Text = "Option1";
			// 
			// Option12
			// 
			this.Option12.BackColor = System.Drawing.SystemColors.Control;
			this.Option12.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option12.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option12.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option12.Location = new System.Drawing.Point(8, 296);
			this.Option12.Name = "Option12";
			this.Option12.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option12.Size = new System.Drawing.Size(17, 17);
			this.Option12.TabIndex = 34;
			this.Option12.Text = "Option1";
			// 
			// Option11
			// 
			this.Option11.BackColor = System.Drawing.SystemColors.Control;
			this.Option11.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option11.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option11.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option11.Location = new System.Drawing.Point(8, 272);
			this.Option11.Name = "Option11";
			this.Option11.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option11.Size = new System.Drawing.Size(17, 17);
			this.Option11.TabIndex = 33;
			this.Option11.Text = "Option1";
			// 
			// Option10
			// 
			this.Option10.BackColor = System.Drawing.SystemColors.Control;
			this.Option10.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option10.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option10.Location = new System.Drawing.Point(8, 248);
			this.Option10.Name = "Option10";
			this.Option10.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option10.Size = new System.Drawing.Size(17, 17);
			this.Option10.TabIndex = 32;
			this.Option10.Text = "Option1";
			// 
			// Option9
			// 
			this.Option9.BackColor = System.Drawing.SystemColors.Control;
			this.Option9.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option9.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option9.Location = new System.Drawing.Point(8, 224);
			this.Option9.Name = "Option9";
			this.Option9.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option9.Size = new System.Drawing.Size(17, 17);
			this.Option9.TabIndex = 31;
			this.Option9.Text = "Option1";
			// 
			// Option7
			// 
			this.Option7.BackColor = System.Drawing.SystemColors.Control;
			this.Option7.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option7.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option7.Location = new System.Drawing.Point(8, 176);
			this.Option7.Name = "Option7";
			this.Option7.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option7.Size = new System.Drawing.Size(17, 17);
			this.Option7.TabIndex = 30;
			this.Option7.Text = "Option1";
			// 
			// Option6
			// 
			this.Option6.BackColor = System.Drawing.SystemColors.Control;
			this.Option6.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option6.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option6.Location = new System.Drawing.Point(8, 152);
			this.Option6.Name = "Option6";
			this.Option6.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option6.Size = new System.Drawing.Size(17, 17);
			this.Option6.TabIndex = 29;
			this.Option6.Text = "Option1";
			// 
			// Option5
			// 
			this.Option5.BackColor = System.Drawing.SystemColors.Control;
			this.Option5.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option5.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option5.Location = new System.Drawing.Point(8, 128);
			this.Option5.Name = "Option5";
			this.Option5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option5.Size = new System.Drawing.Size(17, 17);
			this.Option5.TabIndex = 28;
			this.Option5.Text = "Option1";
			// 
			// Option4
			// 
			this.Option4.BackColor = System.Drawing.SystemColors.Control;
			this.Option4.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option4.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option4.Location = new System.Drawing.Point(8, 104);
			this.Option4.Name = "Option4";
			this.Option4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option4.Size = new System.Drawing.Size(17, 17);
			this.Option4.TabIndex = 27;
			this.Option4.Text = "Option1";
			// 
			// Option2
			// 
			this.Option2.BackColor = System.Drawing.SystemColors.Control;
			this.Option2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option2.Location = new System.Drawing.Point(8, 56);
			this.Option2.Name = "Option2";
			this.Option2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option2.Size = new System.Drawing.Size(17, 17);
			this.Option2.TabIndex = 26;
			this.Option2.Text = "Option1";
			// 
			// Option1
			// 
			this.Option1.BackColor = System.Drawing.SystemColors.Control;
			this.Option1.Checked = true;
			this.Option1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option1.Location = new System.Drawing.Point(8, 32);
			this.Option1.Name = "Option1";
			this.Option1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option1.Size = new System.Drawing.Size(17, 17);
			this.Option1.TabIndex = 25;
			this.Option1.TabStop = true;
			this.Option1.Text = "Option1";
			// 
			// Label11
			// 
			this.Label11.BackColor = System.Drawing.SystemColors.Control;
			this.Label11.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label11.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label11.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label11.Location = new System.Drawing.Point(24, 104);
			this.Label11.Name = "Label11";
			this.Label11.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label11.Size = new System.Drawing.Size(56, 25);
			this.Label11.TabIndex = 24;
			this.Label11.Text = " Det (A)";
			// 
			// Label10
			// 
			this.Label10.BackColor = System.Drawing.SystemColors.Control;
			this.Label10.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label10.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label10.Location = new System.Drawing.Point(24, 320);
			this.Label10.Name = "Label10";
			this.Label10.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label10.Size = new System.Drawing.Size(57, 25);
			this.Label10.TabIndex = 23;
			this.Label10.Text = " A x Tr (B) + Inv(A)";
			// 
			// Label9
			// 
			this.Label9.BackColor = System.Drawing.SystemColors.Control;
			this.Label9.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label9.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label9.Location = new System.Drawing.Point(24, 296);
			this.Label9.Name = "Label9";
			this.Label9.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label9.Size = new System.Drawing.Size(64, 25);
			this.Label9.TabIndex = 22;
			this.Label9.Text = " A x Inv (A)";
			// 
			// Label8
			// 
			this.Label8.BackColor = System.Drawing.SystemColors.Control;
			this.Label8.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label8.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label8.Location = new System.Drawing.Point(24, 272);
			this.Label8.Name = "Label8";
			this.Label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label8.Size = new System.Drawing.Size(57, 25);
			this.Label8.TabIndex = 21;
			this.Label8.Text = " V2 / 3 ";
			// 
			// Label7
			// 
			this.Label7.BackColor = System.Drawing.SystemColors.Control;
			this.Label7.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label7.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label7.Location = new System.Drawing.Point(24, 248);
			this.Label7.Name = "Label7";
			this.Label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label7.Size = new System.Drawing.Size(57, 25);
			this.Label7.TabIndex = 20;
			this.Label7.Text = " 5 A";
			// 
			// Label6
			// 
			this.Label6.BackColor = System.Drawing.SystemColors.Control;
			this.Label6.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label6.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label6.Location = new System.Drawing.Point(24, 224);
			this.Label6.Name = "Label6";
			this.Label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label6.Size = new System.Drawing.Size(57, 25);
			this.Label6.TabIndex = 19;
			this.Label6.Text = " | V1 |";
			// 
			// Label5
			// 
			this.Label5.BackColor = System.Drawing.SystemColors.Control;
			this.Label5.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label5.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label5.Location = new System.Drawing.Point(24, 176);
			this.Label5.Name = "Label5";
			this.Label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label5.Size = new System.Drawing.Size(57, 25);
			this.Label5.TabIndex = 18;
			this.Label5.Text = " V1 x V2";
			// 
			// Label4
			// 
			this.Label4.BackColor = System.Drawing.SystemColors.Control;
			this.Label4.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label4.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label4.Location = new System.Drawing.Point(24, 152);
			this.Label4.Name = "Label4";
			this.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label4.Size = new System.Drawing.Size(72, 25);
			this.Label4.TabIndex = 17;
			this.Label4.Text = "Transpose(B)";
			// 
			// Label3
			// 
			this.Label3.BackColor = System.Drawing.SystemColors.Control;
			this.Label3.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label3.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label3.Location = new System.Drawing.Point(24, 128);
			this.Label3.Name = "Label3";
			this.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label3.Size = new System.Drawing.Size(57, 25);
			this.Label3.TabIndex = 16;
			this.Label3.Text = "Inverse(A)";
			// 
			// Label2
			// 
			this.Label2.BackColor = System.Drawing.SystemColors.Control;
			this.Label2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label2.Location = new System.Drawing.Point(24, 56);
			this.Label2.Name = "Label2";
			this.Label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label2.Size = new System.Drawing.Size(33, 25);
			this.Label2.TabIndex = 15;
			this.Label2.Text = " A - B";
			// 
			// Label1
			// 
			this.Label1.BackColor = System.Drawing.SystemColors.Control;
			this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
			this.Label1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Label1.Location = new System.Drawing.Point(24, 32);
			this.Label1.Name = "Label1";
			this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label1.Size = new System.Drawing.Size(40, 25);
			this.Label1.TabIndex = 14;
			this.Label1.Text = " A + B";
			// 
			// txtDisplay
			// 
			this.txtDisplay.BackColor = System.Drawing.SystemColors.Window;
			this.txtDisplay.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtDisplay.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtDisplay.ForeColor = System.Drawing.SystemColors.WindowText;
			this.txtDisplay.Location = new System.Drawing.Point(8, 8);
			this.txtDisplay.MaxLength = 0;
			this.txtDisplay.Name = "txtDisplay";
			this.txtDisplay.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.txtDisplay.Size = new System.Drawing.Size(393, 328);
			this.txtDisplay.TabIndex = 11;
			this.txtDisplay.Text = "";
			this.txtDisplay.WordWrap = false;
			// 
			// txtSolution
			// 
			this.txtSolution.BackColor = System.Drawing.SystemColors.Window;
			this.txtSolution.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtSolution.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtSolution.ForeColor = System.Drawing.SystemColors.WindowText;
			this.txtSolution.Location = new System.Drawing.Point(8, 336);
			this.txtSolution.MaxLength = 0;
			this.txtSolution.Name = "txtSolution";
			this.txtSolution.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.txtSolution.Size = new System.Drawing.Size(393, 120);
			this.txtSolution.TabIndex = 12;
			this.txtSolution.Text = "";
			this.txtSolution.WordWrap = false;
			// 
			// Option8
			// 
			this.Option8.BackColor = System.Drawing.SystemColors.Control;
			this.Option8.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option8.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option8.Location = new System.Drawing.Point(8, 200);
			this.Option8.Name = "Option8";
			this.Option8.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option8.Size = new System.Drawing.Size(17, 17);
			this.Option8.TabIndex = 40;
			this.Option8.Text = "Option1";
			// 
			// label13
			// 
			this.label13.BackColor = System.Drawing.SystemColors.Control;
			this.label13.Cursor = System.Windows.Forms.Cursors.Default;
			this.label13.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label13.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label13.Location = new System.Drawing.Point(24, 200);
			this.label13.Name = "label13";
			this.label13.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label13.Size = new System.Drawing.Size(57, 25);
			this.label13.TabIndex = 39;
			this.label13.Text = " V1 . V2";
			// 
			// Option14
			// 
			this.Option14.BackColor = System.Drawing.SystemColors.Control;
			this.Option14.Cursor = System.Windows.Forms.Cursors.Default;
			this.Option14.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Option14.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Option14.Location = new System.Drawing.Point(8, 352);
			this.Option14.Name = "Option14";
			this.Option14.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Option14.Size = new System.Drawing.Size(17, 17);
			this.Option14.TabIndex = 42;
			this.Option14.Text = "Option1";
			// 
			// label14
			// 
			this.label14.BackColor = System.Drawing.SystemColors.Control;
			this.label14.Cursor = System.Windows.Forms.Cursors.Default;
			this.label14.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label14.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label14.Location = new System.Drawing.Point(24, 352);
			this.label14.Name = "label14";
			this.label14.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label14.Size = new System.Drawing.Size(72, 25);
			this.label14.TabIndex = 41;
			this.label14.Text = " A x Pinv (A)  x A";
			// 
			// mnFrm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(520, 456);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.txtSolution,
																		  this.txtDisplay,
																		  this.FrmSelect});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "mnFrm";
			this.Text = "Matrix_Lib";
			this.Load += new System.EventHandler(this.mnFrm_Load);
			this.FrmSelect.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new mnFrm());
		}

		private void mnFrm_Load(object sender, System.EventArgs e)
		{

			//Assign some values to matrix A and B
			//notice the use of indexers
			A[0, 0] = 1 ;  A[0, 1] = 2 ;  A[0, 2] = 3 ; A[0, 3] = 4;
			A[1, 0] = 5 ;  A[1, 1] = 6 ;  A[1, 2] = 7 ; A[1, 3] = 8;
			A[2, 0] = 9 ;  A[2, 1] = 10;  A[2, 2] = 1 ; A[2, 3] = 12;
			A[3, 0] = 13 ; A[3, 1] = -14; A[3, 2] = 15 ; A[3, 3] = 16;

			Random rnd = new Random();
			for (int i = 0; i<B.NoRows;i++)
				for (int j = 0; j < B.NoCols;j++)
					B[i, j] = 2 * rnd.NextDouble();

			//Assign some values to vectors V1 and V2
			V1[0, 0] = 1 ; V1[1, 0] = 2 ; V1[2, 0] = 3;
			V2[0, 0] = 4 ; V2[1, 0] = 5 ; V2[2, 0] = 6;

			//Print Matrices And Vectors
			txtDisplay.Text = "Matix A = " + "\n";
			txtDisplay.Text += Matrix.PrintMat(A) + "\n\n";

			txtDisplay.Text += "Matix B = " + "\n";
			txtDisplay.Text += Matrix.PrintMat(B)+ "\n\n";

			txtDisplay.Text += "Vector V1 = " + "\n";
			txtDisplay.Text += Matrix.PrintMat(V1) + "\n\n";

			txtDisplay.Text += "Vector V2 = " + "\n";
			txtDisplay.Text += Matrix.PrintMat(V2)+ "\n";
		}


		private void CalButton_Click(object sender, System.EventArgs e)
		{
			//Assign a dynamic matrix C
			Matrix C;
		
			double Determinant;
			double Magnitude;

			//Addition Case
			if (Option1.Checked)
			{
				// C = Addition of A and B
				// notice the use of operator overloading
				C =  A+B; 
			
				// Or if you dont want to use operator overloading
				// C = Matrix.Add(A,B);

				txtSolution.Text = "Answer> A + B = \n\n";
				// Print C
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Subtraction Case
			else if (Option2.Checked)
			{
				// C = Subtraction of A from B
				// notice the use of operator overloading
				C = A-B;

				// Or if you dont want to use operator overloading
				// C = Matrix.Subtract(A,B);

				txtSolution.Text = "Answer> A - B = \n\n";
				//Print C
				txtSolution.Text +=  Matrix.PrintMat(C);
			}
				//Multiplication Case
			else if (Option3.Checked)
			{
				// C = Multiplication of A with B
				// notice the use of operator overloading
				C = A*B;

				// Or if you dont want to use operator overloading
				//C = Matrix.Multiply(A,B);

				txtSolution.Text = "Answer> A x B = \n\n";
				//Print C
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Determinant Case of Matrix A
			else if (Option4.Checked)
			{
				Determinant = Matrix.Det(A);

				txtSolution.Text = "Answer> Determinant of A = " + Determinant;
			}
				//Inverse Case of Matrix B
			else if (Option5.Checked)
			{            
				C = Matrix.Inverse(A);

				txtSolution.Text = "Answer> Inverse of A = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Transpose Case of Matrix B
			else if (Option6.Checked)
			{            
				C = Matrix.Transpose(B);

				txtSolution.Text = "Answer> Transpose of B = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Mutiply Vectors V1 and V2 Case
			else if (Option7.Checked)
			{
				C = Matrix.CrossProduct(V1, V2);

				//OR using operator overloading
				//C = V1*V2
				txtSolution.Text = "Answer> V1 x V2 = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Dot Vectors V1 and V2 Case
			else if (Option8.Checked)
			{
				double dotproduct = Matrix.DotProduct(V1, V2);

				txtSolution.Text = "Answer> V1 . V2 = " + dotproduct.ToString();
			}

				//Magnitude of Vector V1 Case
			else if (Option9.Checked)
			{
				Magnitude = Matrix.VectorMagnitude(V1);

				txtSolution.Text = "Answer> |V1| =" + Magnitude;
			}
				//Scalar Multiply 5*A Case
			else if (Option10.Checked)
			{
				// Notice the use of operator overloading
				C = 5*A;

				// Or if you dont want to use operator overloading
				// C = Matrix.ScalarMultiply(5,A);

				txtSolution.Text = "Answer> 5A = \n\n";
				//Print C
				txtSolution.Text += Matrix.PrintMat(C);
			}
				//Scalar Divide V2/3 Case
			else if (Option11.Checked)
			{
				// Notice the use of operator overloading
				C = V2/3;
			
				// Or if you dont want to use operator overloading
				// C = Matrix.ScalarDivide(3,V2);

				txtSolution.Text = "Answer> V2 / 3 = \n\n" ;
				txtSolution.Text += Matrix.PrintMat(C);
			}

			else if (Option12.Checked)
			{
				// Case Axinv(A)
				C =  A * Matrix.Inverse(A);
				//or 
				//C = Matrix.Multiply(A, Matrix.Inverse(A));

				Console.Write(A.ToString());
				Console.Write(C.ToString());

				// Equality test
                if (C==new Matrix(Matrix.Identity(4))) MessageBox.Show("A x Inverse(A) = Identity(4), left side and right side equal!!");

				txtSolution.Text = "Answer> A x Inverse(A) = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
			else if (Option13.Checked)
			{
				//Case AxTranspose(B)+inv(A)
				C = A * Matrix.Transpose(B)+Matrix.Inverse(A);

				// OR if not using operator overloading
				//C = Matrix.Add(Matrix.Multiply(A, Matrix.Transpose(B)), Matrix.Inverse(A));
				txtSolution.Text = "Answer> A x Transpose(B)+Inverse(A) = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
			else if (Option14.Checked)
			{
				//Case AxPinv(A)xA

				C = A * Matrix.PINV(A)*A;

				// OR if not using operator overloading
				//C = Matrix.Multiply(A,Matrix.Multiply(Matrix.PINV(A),A));

				txtSolution.Text = "Answer> A x Pinv(A) x A = \n\n";
				txtSolution.Text += Matrix.PrintMat(C);
			}
		}
	}
}
