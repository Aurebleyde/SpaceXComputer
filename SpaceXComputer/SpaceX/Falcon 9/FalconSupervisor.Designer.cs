namespace SpaceXComputer
{
    partial class FalconSupervisor
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
            this.lb_Debug = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lb_Debug
            // 
            this.lb_Debug.AutoSize = true;
            this.lb_Debug.Location = new System.Drawing.Point(53, 57);
            this.lb_Debug.Name = "lb_Debug";
            this.lb_Debug.Size = new System.Drawing.Size(35, 13);
            this.lb_Debug.TabIndex = 0;
            this.lb_Debug.Text = "label1";
            // 
            // FalconSupervisor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.lb_Debug);
            this.Name = "FalconSupervisor";
            this.Text = "FalconSupervisor";
            this.Load += new System.EventHandler(this.FalconSupervisor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lb_Debug;
    }
}