using System.ComponentModel.DataAnnotations;

namespace ThailandpostTracking.DTOs.Thailandpost.Request
{
    public class GetTrackingDetailRequestDTO
    {
        [Required]
        public Guid TrackingHeaderId { get; set; }

        [Required]
        public Guid TrackingBatchId { get; set; }
    }
}