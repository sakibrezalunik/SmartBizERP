using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBizERP.Models
{
    [Table("WarehouseStock")]
    public partial class WarehouseStock
    {
        public int WarehouseStockId { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }

        public virtual Warehouse Warehouse { get; set; }
        public virtual Product Product { get; set; }
    }
}
