namespace WebExecutor
{
    partial class ConsolePanel
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
            this.interactiveConsole = new WebExecutor.InteractiveConsole();
            this.SuspendLayout();
            // 
            // interactiveConsole
            // 
            this.interactiveConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interactiveConsole.Location = new System.Drawing.Point(0, 0);
            this.interactiveConsole.Name = "interactiveConsole";
            this.interactiveConsole.Size = new System.Drawing.Size(667, 247);
            this.interactiveConsole.TabIndex = 0;
            // 
            // ConsolePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 247);
            this.Controls.Add(this.interactiveConsole);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.HideOnClose = true;
            this.Name = "ConsolePanel";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide;
            this.ShowIcon = false;
            this.Text = "Console";
            this.ResumeLayout(false);

        }

        #endregion

        private InteractiveConsole interactiveConsole;
    }
}