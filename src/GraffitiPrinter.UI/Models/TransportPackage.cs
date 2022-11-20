namespace GraffitiPrinter.UI.Models
{
    public class TransportPackage
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public string PackageType { get; set; }
        public int PackageQuantity { get; set; }
        public string PackageWeigth{ get; set; }
        public string PartNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Ingredients { get; set; }
        public string Restrictions { get; set; }
        public string CertificateUnit { get; set; }
        public string ReferenceOrderNumber { get; set; }
        public string ReferenceItemNumber { get; set; }
        public string OriginCountry { get; set; }
        public string SupplierCode { get; set; }
        public string ExternalCompanyCode { get; set; }
        public string IsDangerousGood { get; set; }
    }
}
