using System.ComponentModel.DataAnnotations;

namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetTimeLineResponseDTO
    {
        public Guid TrackingBatchId { get; set; }
        public GetTrackingBatchResponseDTO TrackingBatch { get; set; }
        public Guid TmpImportTrackingId { get; set; }
        public GetTmpImportTrackingResponseDTO TmpImportTracking { get; set; }

        public List<GetTrackingDetailResponseDTO> TrackingDetails { get; set; }
    }
}