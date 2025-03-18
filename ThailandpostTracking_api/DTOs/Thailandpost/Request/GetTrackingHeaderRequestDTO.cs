namespace ThailandpostTracking.DTOs.Thailandpost.Request
{
    public class GetTrackingHeaderRequestDTO : PaginationDto
    {
        public string? TrackingCode { get; set; }
        public string? OrderingField { get; set; }
        public bool AscendingOrder { get; set; } = true;
    }
}