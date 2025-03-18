namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetItemsbyBarcodeResponseDTO
    {
        public TrackingResponse Response { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    public class TrackingItem
    {
        public string Barcode { get; set; }
        public string Status { get; set; }
        public string Status_Description { get; set; }
        public string Status_Date { get; set; }
        public string Location { get; set; }
        public string Postcode { get; set; }
        public string Delivery_Status { get; set; }
        public string Delivery_Description { get; set; }
        public string Delivery_Datetime { get; set; }
        public string Receiver_Name { get; set; }
        public string Signature { get; set; }
        public string StatusDetail { get; set; }
        public string Delivery_Officer_Name { get; set; }
        public string Delivery_Officer_Tel { get; set; }
        public string Office_Name { get; set; }
        public string Office_Tel { get; set; }
        public string Call_Center_Tel { get; set; }
    }

    public class TrackCount
    {
        public string TrackDate { get; set; }
        public int CountNumber { get; set; }
        public int TrackCountLimit { get; set; }
    }

    public class TrackingResponse
    {
        public Dictionary<string, List<TrackingItem>> Items { get; set; }
        public TrackCount TrackCount { get; set; }
    }
}