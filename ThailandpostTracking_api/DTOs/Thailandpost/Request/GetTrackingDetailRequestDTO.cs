using System.ComponentModel.DataAnnotations;

namespace ThailandpostTracking.DTOs.Thailandpost.Request
{
    public class GetTrackingDetailRequestDTO : PaginationDto
    {
        [Required]
        public Guid TrackingHeaderId { get; set; }

        [Required]
        public Guid TrackingBatchId { get; set; }

        public string? OrderingField { get; set; }
        public bool AscendingOrder { get; set; } = true;
    }
}