﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    partial class BackupRestore : Form
    {
        public BackupRestore()
        {
            InitializeComponent();
        }

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // BackupRestore
            // 
            AutoScaleDimensions = new SizeF(10f, 25f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "BackupRestore";
            Text = "BackupRestore";
            Load += new EventHandler(BackupRestore_Load);
            ResumeLayout(false);
        }

        private void BackupRestore_Load(object sender, EventArgs e)
        {

        }
    }
}