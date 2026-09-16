using System.Data.Entity;

namespace SmartBizERP.Models
{
    public partial class SmartBizERPModel : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
    }
}
