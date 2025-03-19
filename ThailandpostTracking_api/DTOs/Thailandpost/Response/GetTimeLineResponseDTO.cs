using System.ComponentModel.DataAnnotations;

namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetTimeLineResponseDTO
    {
        public List<GetTrackingDetailResponseDTO> TrackingDetails { get; set; }
        public Guid TmpImportTrackingId { get; set; }
        public GetTmpImportTrackingResponseDTO TmpImportTracking { get; set; }
        public Guid TrackingBatchId { get; set; }
        public GetTrackingBatchResponseDTO TrackingBatch { get; set; }
    }
}