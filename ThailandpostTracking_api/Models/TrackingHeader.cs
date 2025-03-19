﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ThailandpostTracking.Models
{
    [Table("TrackingHeader")]
    public partial class TrackingHeader
    {
        public TrackingHeader()
        {
            TrackingDetails = new HashSet<TrackingDetail>();
        }

        [Key]
        public Guid TrackingHeaderId { get; set; }
        public Guid? TrackingBatchId { get; set; }
        public Guid? TmpImportTrackingId { get; set; }
        [StringLength(255)]
        [Unicode(false)]
        public string TrackingCode { get; set; }
        [StringLength(255)]
        [Unicode(false)]
        public string ReferenceCode { get; set; }
        [StringLength(200)]
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        /// <summary>
        /// สถานะ
        /// </summary>
        [StringLength(50)]
        [Unicode(false)]
        public string Status { get; set; }
        /// <summary>
        /// คำอธิบายสถานะ
        /// </summary>
        [StringLength(255)]
        public string Status_Description { get; set; }
        /// <summary>
        /// สถานะของวันที่
        /// </summary>
        public DateTime? Status_Date { get; set; }
        /// <summary>
        /// รายละเอียดเพิ่มเติมของสถานะ
        /// </summary>
        public string StatusDetail { get; set; }
        /// <summary>
        /// สถานที่ตั้ง
        /// </summary>
        [StringLength(255)]
        public string Location { get; set; }
        /// <summary>
        /// รหัสไปรษณีย์
        /// </summary>
        [StringLength(50)]
        [Unicode(false)]
        public string Postcode { get; set; }
        /// <summary>
        /// สถานะการจัดส่ง
        /// </summary>
        [StringLength(50)]
        [Unicode(false)]
        public string Delivery_Status { get; set; }
        /// <summary>
        /// คำอธิบายการจัดส่ง
        /// </summary>
        [StringLength(255)]
        public string Delivery_Description { get; set; }
        /// <summary>
        /// วันที่จัดส่ง
        /// </summary>
        public DateTime? Delivery_Datetime { get; set; }
        /// <summary>
        /// ชื่อผู้รับ
        /// </summary>
        [StringLength(255)]
        public string Receiver_Name { get; set; }
        /// <summary>
        /// ลายเซ็น
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// ชื่อเจ้าหน้าที่นำจ่าย
        /// </summary>
        [StringLength(255)]
        public string Delivery_Officer_Name { get; set; }
        /// <summary>
        /// เบอร์ติดต่อเจ้าหน้าที่นำจ่าย
        /// </summary>
        [StringLength(100)]
        [Unicode(false)]
        public string Delivery_Officer_Tel { get; set; }
        /// <summary>
        /// ชื่อที่ทำการไปรษณีย์
        /// </summary>
        [StringLength(255)]
        public string Office_Name { get; set; }
        /// <summary>
        /// บอร์ติดต่อที่ทำการไปรษณีย์
        /// </summary>
        [StringLength(100)]
        [Unicode(false)]
        public string Office_Tel { get; set; }
        /// <summary>
        /// THP Contact Center
        /// </summary>
        [StringLength(100)]
        [Unicode(false)]
        public string Call_Center_Tel { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedByUserId { get; set; }

        [ForeignKey("TmpImportTrackingId")]
        [InverseProperty("TrackingHeaders")]
        public virtual TmpImportTracking TmpImportTracking { get; set; }
        [ForeignKey("TrackingBatchId")]
        [InverseProperty("TrackingHeaders")]
        public virtual TrackingBatch TrackingBatch { get; set; }
        [InverseProperty("TrackingHeader")]
        public virtual ICollection<TrackingDetail> TrackingDetails { get; set; }
    }
}