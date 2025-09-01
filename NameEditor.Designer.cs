using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class NameEditor : Form
    {

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
            // NameEditor
            // 
            AutoScaleDimensions = new SizeF(10.0f, 25.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "NameEditor";
            Text = "NameEditor";
            Load += new EventHandler(NameManagment);
            ResumeLayout(false);
        }

    }
}