namespace SmartBizERP.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Warehouse
    {
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(20)]
        public string WarehouseCode { get; set; }

        [Required]
        [StringLength(100)]
        public string WarehouseName { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
