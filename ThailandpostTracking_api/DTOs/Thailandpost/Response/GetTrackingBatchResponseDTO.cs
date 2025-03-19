namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetTrackingBatchResponseDTO
    {
        public Guid? TrackingBatchId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}