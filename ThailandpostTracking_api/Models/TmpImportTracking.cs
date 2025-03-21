﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ThailandpostTracking.Models
{
    [Table("TmpImportTracking")]
    public partial class TmpImportTracking
    {
        [Key]
        public Guid TmpImportTrackingId { get; set; }
        [StringLength(255)]
        [Unicode(false)]
        public string TrackingCode { get; set; }
        [StringLength(255)]
        [Unicode(false)]
        public string ReferenceCode { get; set; }
        [StringLength(200)]
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public bool? IsInsert { get; set; }
        public DateTime? TransactionDate { get; set; }
        public bool? IsResult { get; set; }
        [StringLength(255)]
        public string Message { get; set; }
        public bool? IsActive { get; set; }
        public string Remark { get; set; }

        [InverseProperty("TmpImportTracking")]
        public virtual TrackingHeader TrackingHeader { get; set; }
    }
}