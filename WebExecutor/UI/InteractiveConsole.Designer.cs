namespace WebExecutor
{
    partial class InteractiveConsole
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.logEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.SuspendLayout();
            // 
            // logEditor
            // 
            this.logEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logEditor.IsReadOnly = false;
            this.logEditor.Location = new System.Drawing.Point(3, 3);
            this.logEditor.Name = "logEditor";
            this.logEditor.ShowLineNumbers = false;
            this.logEditor.ShowVRuler = false;
            this.logEditor.Size = new System.Drawing.Size(622, 204);
            this.logEditor.TabIndex = 0;
            // 
            // InteractiveConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.logEditor);
            this.Name = "InteractiveConsole";
            this.Size = new System.Drawing.Size(628, 210);
            this.Load += new System.EventHandler(this.InteractiveConsole_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl logEditor;
    }
}
