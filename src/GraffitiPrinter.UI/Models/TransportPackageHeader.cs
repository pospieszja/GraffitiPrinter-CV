using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraffitiPrinter.UI.Models
{
    public class TransportPackageHeader
    {
        public int TransportPackageNumber { get; set; }
        public string TransportPackageAnteeoNumber { get; set; }
        public string PackageUnit { get; set; }
        public int PackageQuantityHeader { get; set; }
        public string PickupDate { get; set; }
        public string Type { get; set; }
        public string Issuer { get; set; }
        public string CreatedAt { get; set; }
        public string TrackingNumber { get; set; }
        public string CustomerName { get; set; }
        public string CollectiveProofNumber { get; set; }
        public string Info { get; set; }
        public string Status { get; set; }
        public int GraffitiStatus { get; set; }
        public int GoodIssueStatus { get; set; }
        public string QualityDocuments { get; set; }
        public bool IsPrinted { get; set; }
        public string ElectronicInvoice { get; set; }
        public string SchenkerStatus { get; set; }
    }
}
