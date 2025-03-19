using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ThailandpostTracking.Models;

namespace ThailandpostTracking.DTOs.Thailandpost.Response
{
    public class GetTrackingHeaderResponseDTO
    {
        public Guid TrackingHeaderId { get; set; }
        public GetTrackingBatchResponseDTO TrackingBatch { get; set; }
        public Guid TmpImportTrackingId { get; set; }
        public GetTmpImportTrackingResponseDTO TmpImportTracking { get; set; }

        public string TrackingCode { get; set; }

        public string ReferenceCode { get; set; }

        public string Status { get; set; }

        public string Status_Description { get; set; }
        public DateTime? Status_Date { get; set; }
        public string StatusDetail { get; set; }

        public string Location { get; set; }

        public string Postcode { get; set; }

        public string Delivery_Status { get; set; }

        public string Delivery_Description { get; set; }
        public DateTime? Delivery_Datetime { get; set; }

        public string Receiver_Name { get; set; }
        public string Signature { get; set; }

        public string Delivery_Officer_Name { get; set; }

        public string Delivery_Officer_Tel { get; set; }

        public string Office_Name { get; set; }

        public string Office_Tel { get; set; }

        public string Call_Center_Tel { get; set; }
    }
}