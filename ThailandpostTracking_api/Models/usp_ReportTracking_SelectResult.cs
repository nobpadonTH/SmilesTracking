﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThailandpostTracking.Models
{
    public partial class usp_ReportTracking_SelectResult
    {
        public string TrackingCode { get; set; }
        public string ReferenceCode { get; set; }
        public string PolicyNo { get; set; }
        public string Status { get; set; }
        public string Status_Description { get; set; }
        public DateTime? Status_Date { get; set; }
        public TimeSpan? Status_Time { get; set; }
        public string StatusDetail { get; set; }
        public string Location { get; set; }
        public string Postcode { get; set; }
        public string Delivery_Status { get; set; }
        public string Delivery_Description { get; set; }
        public DateTime? Delivery_Date { get; set; }
        public TimeSpan? Delivery_Time { get; set; }
        public string Receiver_Name { get; set; }
        public string Signature { get; set; }
        public string Delivery_Officer_Name { get; set; }
        public string Delivery_Officer_Tel { get; set; }
        public string Office_Name { get; set; }
        public string Office_Tel { get; set; }
        public string Call_Center_Tel { get; set; }
    }
}
