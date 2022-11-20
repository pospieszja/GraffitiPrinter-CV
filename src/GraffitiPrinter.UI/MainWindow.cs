using GraffitiPrinter.UI.DAL;
using GraffitiPrinter.UI.Infrastructure;
using GraffitiPrinter.UI.Models;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GraffitiPrinter.UI
{
    public partial class MainWindow : Form
    {
        private readonly GraffitiRepository _repositoryGraffiti;

        private double _divider;
        private int transportOrderNumber;
        private List<TransportPackage> _transportOrders;
        private List<TransportPackageHeader> _transportOrdersHeaders;

        public MainWindow()
        {
            InitializeComponent();
            _repositoryGraffiti = new GraffitiRepository();
            _transportOrders = new List<TransportPackage>();
            _transportOrdersHeaders = new List<TransportPackageHeader>();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView3.AutoGenerateColumns = false;
            dataGridView4.AutoGenerateColumns = false;
            dataGridView2.Columns["Wz"].DefaultCellStyle.NullValue = null;
            dataGridView2.Columns["Wydruk"].DefaultCellStyle.NullValue = null;
            dataGridView2.Columns["Completed"].DefaultCellStyle.NullValue = null;

            SetPickupDates();
            DeleteTempFiles();
            CreateTempDirectory();
        }

        private void SetTransportOrderNumber()
        {
            transportOrderNumber = Int32.Parse(dataGridView2.CurrentRow.Cells["TransportPackageNumber"].Value.ToString());
        }

        private void SetPickupDates()
        {
            var today = DateTime.Today;
            dpPickupDateFrom.Value = today.AddDays(-1);
            dpPickupDateTo.Value = today.AddDays(1);
        }

        private void DeleteTempFiles()
        {
            var path = Path.Combine(Path.GetTempPath(), "GraffitiPrinter");
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "GraffitiPrinter");
            Directory.CreateDirectory(path);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            var dict = PrepareDictionaryForTemplate();

            var fileName = String.Concat(transportOrderNumber + ".pdf");

            var document2Pdf = new Document2PDF();
            document2Pdf.Generate(dict);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CleanTextBox();

            if (!String.IsNullOrEmpty(tbxTransportOrderNumber.Text))
            {
                var transportOrderNumber = Int32.Parse(tbxTransportOrderNumber.Text);
                _transportOrdersHeaders = _repositoryGraffiti.GetTransportPackageHeaderById(transportOrderNumber).ToList();

                if (_transportOrdersHeaders.Count() > 0)
                {
                    dataGridView2.DataSource = _transportOrdersHeaders;
                    PrepareColorsAndIcons();
                    return;
                }

                MessageBox.Show($"Brak zlecenia");
            }
        }

        private void GetTransportOrderPositions(int transportOrderNumber)
        {
            _transportOrders = _repositoryGraffiti.GetTransportPackage(transportOrderNumber).ToList();

            if (_transportOrders.Count() > 0)
            {
                dataGridView1.DataSource = _transportOrders;
                ActivateButtonsAndFields();
                return;
            }

            DisableButtonsAndFields();
            MessageBox.Show($"Brak awizacji o numerze {transportOrderNumber}");
        }

        private void ActivateButtonsAndFields()
        {
            btnGeneratePDF.Enabled = true;
            btnPrintLabelToZebra110.Enabled = true;
            btnPrintLabelCollectiveToZebra110.Enabled = true;
            btnCollectiveProofPrint.Enabled = true;
            btnLabelPrint.Enabled = true;
            btnWayBillPrint.Enabled = true;
            dataGridView1.Enabled = true;
            btnMarkAsPrinted.Enabled = true;
            btnUnlockOrder.Enabled = true;
        }

        private void DisableButtonsAndFields()
        {
            btnGeneratePDF.Enabled = false;
            btnPrintLabelToZebra110.Enabled = false;
            btnCollectiveProofPrint.Enabled = false;
            btnLabelPrint.Enabled = false;
            btnWayBillPrint.Enabled = false;
            btnMarkAsPrinted.Enabled = false;
            btnUnlockOrder.Enabled = false;
        }

        private void AllowOnlyNumbers(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !(e.KeyChar == ','))
            {
                e.Handled = true;
            }
        }

        private void CleanTextBox()
        {
            tbxTotalWeight.Text = "";
            tbxPackageQuantity.Text = "";
        }

        private void RecalculateQuantities(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(tbxPackageQuantity.Text))
            {
                tbxTotalWeight.Text = (Double.Parse(tbxPackageQuantity.Text) * _divider).ToString();
            }
        }

        private void RecalculateWeight()
        {
            if (!String.IsNullOrEmpty(tbxTotalWeight.Text))
            {
                tbxPackageQuantity.Text = (Double.Parse(tbxTotalWeight.Text) / _divider).ToString();
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var zebra = new Zebra();
            var dict = PrepareDictionaryForTemplate();
            dict.Add("NumberOfLabels", tbxNumberOfLabels.Text);

            zebra.Print("192.168.2.110", dict, 1);
        }

        private Dictionary<string, string> PrepareDictionaryForTemplate()
        {
            var dict = new Dictionary<string, string>();

            var currentRowIndex = dataGridView1.CurrentCell.RowIndex;
            var selectedTransportOrder = _transportOrders[currentRowIndex];

            var properties = typeof(TransportPackage).GetProperties();

            foreach (var property in properties)
            {
                dict.Add(property.Name, property.GetValue(selectedTransportOrder)?.ToString());
            }

            dict.Add("TransportWeigth", tbxTotalWeight.Text.ToString());
            dict.Add("TransportQuantity", tbxPackageQuantity.Text.ToString());

            return dict;
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            _divider = Double.Parse(dataGridView1.CurrentRow.Cells["Quantity"].Value.ToString()) / Int32.Parse(dataGridView1.CurrentRow.Cells["PackageQuantity"].Value.ToString());

            tbxTotalWeight.Text = dataGridView1.CurrentRow.Cells["Quantity"].Value.ToString();
            tbxPackageQuantity.Text = dataGridView1.CurrentRow.Cells["PackageQuantity"].Value.ToString();
            tbxNumberOfLabels.Text = dataGridView1.CurrentRow.Cells["PackageQuantity"].Value.ToString();
        }

        private void btnWayBillPrint_Click(object sender, EventArgs e)
        {
            Process.Start($"http://192.168.1.27/v2/index.php?action=get_documents&graffiti_transport_order={transportOrderNumber}&schenker_document_type=LP");
        }

        private void btnLabelPrint_Click(object sender, EventArgs e)
        {
            Process.Start($"http://192.168.1.27/v2/index.php?action=get_documents&graffiti_transport_order={transportOrderNumber}&schenker_document_type=LABEL");
        }

        private void btnCollectiveProofPrint_Click(object sender, EventArgs e)
        {
            var collectiveProof = _repositoryGraffiti.GetCollectiveProofInformation(transportOrderNumber);

            if (collectiveProof.CollectiveNumber > 0)
            {
                Process.Start($"http://192.168.1.27/v2/index.php?action=get_documents_zdn&id_paczki_zdn={collectiveProof.PackageId}&id_paczki_zdn_lp={collectiveProof.PackageOrder}&nr_zdn={collectiveProof.CollectiveNumber}");
                return;
            }

            MessageBox.Show($"Brak zbiorczego dowodu nadania dla zlecenia pakowania nr: {transportOrderNumber}");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchTransportOrder();
        }

        private void PrepareColorsAndIconsForItems()
        {
            dataGridView1.ClearSelection();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string isDangerousGood = "TAK";

                if (row.Cells["IsDangerousGood"].Value?.ToString() == isDangerousGood)
                {
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                }
            }
        }

        private void PrepareColorsAndIcons()
        {
            Icon ic = new Icon(@"ico\Wz_16x16.ico");
            Icon ic2 = new Icon(@"ico\drukarka_16_16.ico");
            Icon ic3 = new Icon(@"ico\check-mark-16.ico");

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                int goodIssueStatus;
                if (int.TryParse(row.Cells["GoodIssueStatus"].Value.ToString(), out goodIssueStatus))
                {
                    if (goodIssueStatus == 1)
                    {
                        row.Cells["Wz"].Value = ic;
                    }
                }

                bool isPrinted;
                if (bool.TryParse(row.Cells["IsPrinted"].Value.ToString(), out isPrinted))
                {
                    if (isPrinted == true)
                    {
                        row.Cells["Wydruk"].Value = ic2;
                    }
                }

                int status;
                if (int.TryParse(row.Cells["GraffitiStatus"].Value.ToString(), out status))
                {

                    if (status == 1 || status == 3)
                    {
                        row.Cells["TransportPackageNumber"].Style.BackColor = Color.Yellow;
                        continue;
                    }

                    if (status == 2 || status == 4)
                    {
                        row.Cells["TransportPackageNumber"].Style.BackColor = Color.Green;
                        continue;
                    }

                    if (status == 5)
                    {
                        row.Cells["TransportPackageNumber"].Style.BackColor = Color.Green;
                        row.Cells["Completed"].Value = ic3;
                        continue;
                    }

                    if (status == -1)
                    {
                        row.Cells["TransportPackageNumber"].Style.BackColor = Color.Red;
                        continue;
                    }
                }
            }
        }

        private void SearchTransportOrder()
        {
            _transportOrdersHeaders = _repositoryGraffiti.GetTransportPackageHeaderByPickupDate(dpPickupDateFrom.Value, dpPickupDateTo.Value, rbOpenOrders.Checked, rbCloseOrders.Checked).ToList();

            if (_transportOrdersHeaders.Count() > 0)
            {
                dataGridView2.DataSource = _transportOrdersHeaders;
                PrepareColorsAndIcons();
                return;
            }

            MessageBox.Show($"Brak zleceń w szukanym okresie");
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            DisablePrintButtons();
            SetTransportOrderNumber();
            SetTransportOrderInfo();
            GetTransportOrderPositions(transportOrderNumber);
            ClearDocumentsGrid();
            GetDocuments(transportOrderNumber);
            EnablePrintButtons();
            SwitchButtonIssuedToCarrier();
            SwitchButtonUnlockOrder();
            PrepareColorsAndIconsForItems();
        }

        private void SwitchButtonUnlockOrder()
        {
            var selectedHeader = (TransportPackageHeader)dataGridView2.CurrentRow.DataBoundItem;

            if (selectedHeader.GraffitiStatus == 5)
            {
                btnUnlockOrder.Enabled = true;
                return;
            }

            btnUnlockOrder.Enabled = false;
        }

        private void SwitchButtonIssuedToCarrier()
        {
            var selectedHeader = (TransportPackageHeader)dataGridView2.CurrentRow.DataBoundItem;

            if (selectedHeader.GraffitiStatus == 2)
            {
                btnIssuedToCarrier.Enabled = true;
                return;
            }

            btnIssuedToCarrier.Enabled = false;
        }

        private void SetTransportOrderInfo()
        {
            tbxTransportPackageInfo.Text = dataGridView2.CurrentRow.Cells["Info"].Value.ToString();
        }

        private void EnablePrintButtons()
        {
            if (dataGridView3.DataSource != null)
            {
                btnPrintAll.Enabled = true;
            }

            if (dataGridView4.DataSource != null)
            {
                btnPrintAllInvoices.Enabled = true;
            }
        }

        private void DisablePrintButtons()
        {
            btnPrintAll.Enabled = false;
            btnPrintAllInvoices.Enabled = false;
        }

        private void GetDocuments(int transportOrderNumber)
        {
            var documents = _repositoryGraffiti.GetGraffitiDocument(transportOrderNumber, 1);
            var invoices = _repositoryGraffiti.GetGraffitiDocument(transportOrderNumber, 2);

            if (documents.Count() > 0)
            {
                dataGridView3.DataSource = documents;
            }

            if (invoices.Count() > 0)
            {
                dataGridView4.DataSource = invoices;
            }
        }

        private void ClearDocumentsGrid()
        {
            dataGridView3.DataSource = null;
            dataGridView4.DataSource = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var zebra = new Zebra();
            var dict = PrepareDictionaryForTemplate();
            dict.Add("NumberOfLabels", tbxNumberOfLabels.Text);

            zebra.Print("192.168.2.110", dict, 2);
        }

        private void dataGridView3_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            GraffitiDocument selectedDocument = (GraffitiDocument)dataGridView3.CurrentRow.DataBoundItem;

            PDFBrowser pdfViewerWindow = new PDFBrowser(selectedDocument);

            pdfViewerWindow.Owner = this;
            pdfViewerWindow.ShowDialog();
        }

        private void dataGridView4_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            GraffitiDocument selectedDocument = (GraffitiDocument)dataGridView4.CurrentRow.DataBoundItem;

            PDFBrowser pdfViewerWindow = new PDFBrowser(selectedDocument);

            pdfViewerWindow.Owner = this;
            pdfViewerWindow.ShowDialog();
        }

        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                GraffitiDocument graffitiDocument = (GraffitiDocument)row.DataBoundItem;

                try
                {
                    var doc = PdfDocument.Load(graffitiDocument.Binary);
                    var printDoc = new PdfPrintDocument(doc);
                    PrintController printController = new StandardPrintController();
                    printDoc.PrintController = printController;
                    printDoc.Print();
                    printDoc.Dispose();
                    doc.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void btnPrintAllInvoices_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView4.Rows)
            {
                GraffitiDocument graffitiDocument = (GraffitiDocument)row.DataBoundItem;

                try
                {
                    var doc = PdfDocument.Load(graffitiDocument.Binary);
                    var printDoc = new PdfPrintDocument(doc);
                    PrintController printController = new StandardPrintController();
                    printDoc.PrintController = printController;
                    printDoc.Print();
                    printDoc.Dispose();
                    doc.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void SetIssuedToCarrier(object sender, EventArgs e)
        {
            _repositoryGraffiti.SetIssuedToCarrier(transportOrderNumber);
        }

        private void btnMarkAsPrinted_Click(object sender, EventArgs e)
        {
            _repositoryGraffiti.SetTransportPackageAsPrinted(transportOrderNumber);
            SearchTransportOrder();
        }

        private void tbxTotalWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RecalculateWeight();
            }
        }

        private void btnUnlockOrder_Click(object sender, EventArgs e)
        {
            string message = $"Czy chcesz odblokować zlecenie - {transportOrderNumber} ?";
            string title = "Odblokowanie zlecenia";

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            if (result == DialogResult.Yes)
            {
                _repositoryGraffiti.SetUnlockOrder(transportOrderNumber);
                SearchTransportOrder();
            }
        }
    }
}
