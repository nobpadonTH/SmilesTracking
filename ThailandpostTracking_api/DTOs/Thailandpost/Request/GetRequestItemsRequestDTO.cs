namespace ThailandpostTracking.DTOs.Thailandpost.Request
{
    public class GetRequestItemsRequestDTO
    {
        public string Status { get; set; }
        public string Language { get; set; }
        public List<string> Barcode { get; set; }
    }
}