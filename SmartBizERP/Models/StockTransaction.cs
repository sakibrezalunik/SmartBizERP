using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBizERP.Models
{
    [Table("StockTransactions")]
    public partial class StockTransaction
    {
        public long StockTransactionId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? CreatedBy { get; set; }

        public virtual Product Product { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}
