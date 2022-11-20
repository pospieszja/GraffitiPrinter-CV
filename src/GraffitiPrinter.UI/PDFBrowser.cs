using GraffitiPrinter.UI.Models;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.WinForms;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace GraffitiPrinter.UI
{
    public partial class PDFBrowser : Form
    {
        private GraffitiDocument _graffitiDocument { get; set; }
        private MemoryStream _ms { get; set; }

        public PDFBrowser(GraffitiDocument document)
        {
            InitializeComponent();
            _graffitiDocument = document;
            LoadPdf();
        }

        private void LoadPdf()
        {
            try
            {
                _ms = new MemoryStream(_graffitiDocument.Binary);
                pdfViewer1.LoadDocument(_ms);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnPrintDocumentPDF_Click(object sender, EventArgs e)
        {
            var doc = PdfDocument.Load(_graffitiDocument.Binary);

            var printDoc = new PdfPrintDocument(doc);
            PrintController printController = new StandardPrintController();
            printDoc.PrintController = printController;
            printDoc.Print();
            printDoc.Dispose();
            doc.Dispose();
        }

        private void PDFBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            _ms.Dispose();
            pdfViewer1.Dispose();
        }
    }
}
