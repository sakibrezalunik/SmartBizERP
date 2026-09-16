using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBizERP.Models
{
    public partial class SalesOrderDetail
    {
        public int SalesOrderDetailId { get; set; }
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalAmount { get; set; }

        public virtual SalesOrder SalesOrder { get; set; }
        public virtual Product Product { get; set; }
    }
}
