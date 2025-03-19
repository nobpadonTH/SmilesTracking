using System.ComponentModel.DataAnnotations;

namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetTmpImportTrackingResponseDTO
    {
        public Guid? TmpImportTrackingId { get; set; }
        public string ReferenceCode { get; set; }
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
    }
}