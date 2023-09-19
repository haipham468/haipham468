namespace DataCheck
{
    partial class DataCheckMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtScriptFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseScript = new System.Windows.Forms.Button();
            this.btnBrowseData = new System.Windows.Forms.Button();
            this.txtDataFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnProcess = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Script Folder";
            //this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtScriptFolder
            // 
            this.txtScriptFolder.Location = new System.Drawing.Point(86, 19);
            this.txtScriptFolder.Name = "txtScriptFolder";
            this.txtScriptFolder.Size = new System.Drawing.Size(260, 20);
            this.txtScriptFolder.TabIndex = 1;
            //this.txtScriptFolder.TextChanged += new System.EventHandler(this.txtScriptFolder_TextChanged);
            // 
            // btnBrowseScript
            // 
            this.btnBrowseScript.Location = new System.Drawing.Point(359, 19);
            this.btnBrowseScript.Name = "btnBrowseScript";
            this.btnBrowseScript.Size = new System.Drawing.Size(67, 21);
            this.btnBrowseScript.TabIndex = 2;
            this.btnBrowseScript.Text = "Browse";
            this.btnBrowseScript.UseVisualStyleBackColor = true;
            this.btnBrowseScript.Click += new System.EventHandler(this.btnBrowseScript_Click);
            // 
            // btnBrowseData
            // 
            this.btnBrowseData.Location = new System.Drawing.Point(359, 49);
            this.btnBrowseData.Name = "btnBrowseData";
            this.btnBrowseData.Size = new System.Drawing.Size(67, 21);
            this.btnBrowseData.TabIndex = 5;
            this.btnBrowseData.Text = "Browse";
            this.btnBrowseData.UseVisualStyleBackColor = true;
            this.btnBrowseData.Click += new System.EventHandler(this.btnBrowseData_Click);
            // 
            // txtDataFile
            // 
            this.txtDataFile.Location = new System.Drawing.Point(86, 49);
            this.txtDataFile.Name = "txtDataFile";
            this.txtDataFile.Size = new System.Drawing.Size(260, 20);
            this.txtDataFile.TabIndex = 4;
            //this.txtDataFile.TextChanged += new System.EventHandler(this.txtDataFile_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Data File";
            //this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(162, 140);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(87, 27);
            this.btnProcess.TabIndex = 8;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // DataCheckMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 187);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnBrowseData);
            this.Controls.Add(this.txtDataFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseScript);
            this.Controls.Add(this.txtScriptFolder);
            this.Controls.Add(this.label1);
            this.Name = "DataCheckMain";
            this.Text = "Data Check";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScriptFolder;
        private System.Windows.Forms.Button btnBrowseScript;
        private System.Windows.Forms.Button btnBrowseData;
        private System.Windows.Forms.TextBox txtDataFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

