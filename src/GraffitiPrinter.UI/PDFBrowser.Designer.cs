namespace GraffitiPrinter.UI
{
    partial class PDFBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFBrowser));
            this.pdfViewer1 = new Patagames.Pdf.Net.Controls.WinForms.PdfViewer();
            this.btnPrintDocumentPDF = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pdfViewer1
            // 
            this.pdfViewer1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pdfViewer1.CurrentIndex = -1;
            this.pdfViewer1.CurrentPageHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.pdfViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfViewer1.Document = null;
            this.pdfViewer1.FormHighlightColor = System.Drawing.Color.Transparent;
            this.pdfViewer1.FormsBlendMode = Patagames.Pdf.Enums.BlendTypes.FXDIB_BLEND_MULTIPLY;
            this.pdfViewer1.LoadingIconText = "Loading...";
            this.pdfViewer1.Location = new System.Drawing.Point(0, 0);
            this.pdfViewer1.MouseMode = Patagames.Pdf.Net.Controls.WinForms.MouseModes.Default;
            this.pdfViewer1.Name = "pdfViewer1";
            this.pdfViewer1.OptimizedLoadThreshold = 1000;
            this.pdfViewer1.Padding = new System.Windows.Forms.Padding(10);
            this.pdfViewer1.PageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pdfViewer1.PageAutoDispose = true;
            this.pdfViewer1.PageBackColor = System.Drawing.Color.White;
            this.pdfViewer1.PageBorderColor = System.Drawing.Color.Black;
            this.pdfViewer1.PageMargin = new System.Windows.Forms.Padding(10);
            this.pdfViewer1.PageSeparatorColor = System.Drawing.Color.Gray;
            this.pdfViewer1.RenderFlags = ((Patagames.Pdf.Enums.RenderFlags)((Patagames.Pdf.Enums.RenderFlags.FPDF_LCD_TEXT | Patagames.Pdf.Enums.RenderFlags.FPDF_NO_CATCH)));
            this.pdfViewer1.ShowCurrentPageHighlight = true;
            this.pdfViewer1.ShowLoadingIcon = true;
            this.pdfViewer1.ShowPageSeparator = true;
            this.pdfViewer1.Size = new System.Drawing.Size(948, 644);
            this.pdfViewer1.SizeMode = Patagames.Pdf.Net.Controls.WinForms.SizeModes.Zoom;
            this.pdfViewer1.TabIndex = 0;
            this.pdfViewer1.TextSelectColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.pdfViewer1.TilesCount = 2;
            this.pdfViewer1.UseProgressiveRender = true;
            this.pdfViewer1.ViewMode = Patagames.Pdf.Net.Controls.WinForms.ViewModes.Vertical;
            this.pdfViewer1.Zoom = 1F;
            // 
            // btnPrintDocumentPDF
            // 
            this.btnPrintDocumentPDF.Location = new System.Drawing.Point(13, 13);
            this.btnPrintDocumentPDF.Name = "btnPrintDocumentPDF";
            this.btnPrintDocumentPDF.Size = new System.Drawing.Size(75, 23);
            this.btnPrintDocumentPDF.TabIndex = 1;
            this.btnPrintDocumentPDF.Text = "Drukuj PDF";
            this.btnPrintDocumentPDF.UseVisualStyleBackColor = true;
            this.btnPrintDocumentPDF.Click += new System.EventHandler(this.btnPrintDocumentPDF_Click);
            // 
            // PDFBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 644);
            this.Controls.Add(this.btnPrintDocumentPDF);
            this.Controls.Add(this.pdfViewer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PDFBrowser";
            this.Text = "GraffitiPrinter - przeglądarka PDF";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PDFBrowser_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private Patagames.Pdf.Net.Controls.WinForms.PdfViewer pdfViewer1;
        private System.Windows.Forms.Button btnPrintDocumentPDF;
    }
}