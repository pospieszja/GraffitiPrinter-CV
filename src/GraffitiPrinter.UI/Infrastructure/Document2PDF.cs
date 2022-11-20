using PdfSharp;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace GraffitiPrinter.UI.Infrastructure
{
    public class Document2PDF
    {
        private PdfDocument _pdf;
        private string _labelTemplateA5 = File.ReadAllText("config/layout_sizeA5.html");

        private readonly PageSize _labelPageSize = PageSize.A5;

        public void Generate(Dictionary<string, string> dict)
        {
            foreach (var item in dict)
            {
                _labelTemplateA5 = _labelTemplateA5.Replace($"[{item.Key}]", item.Value);
            }

            _pdf = PdfGenerator.GeneratePdf(_labelTemplateA5, _labelPageSize);

            string outputPath = Path.GetRandomFileName();
            outputPath = Path.ChangeExtension(outputPath, "pdf");
            outputPath = Path.Combine(Path.GetTempPath(), "GraffitiPrinter", outputPath);

            _pdf.Save(outputPath);

            System.Diagnostics.Process.Start(outputPath);
        }
    }
}