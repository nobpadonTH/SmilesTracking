namespace ThailandpostTracking.DTOs.Thailandpost.Request
{
    public class GetItemsbyBarcodeRequestDTO
    {
        public string Status { get; set; }
        public string Language { get; set; }
        public List<string> Barcode { get; set; }
    }
}