using System.Data.Entity;

namespace SmartBizERP.Models
{
    public partial class SmartBizERPModel : DbContext
    {
        public virtual DbSet<WarehouseStock> WarehouseStocks { get; set; }
        public virtual DbSet<StockTransaction> StockTransactions { get; set; }
    }
}
