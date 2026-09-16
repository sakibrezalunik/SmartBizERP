using System;

namespace SmartBizERP.Models
{
    public partial class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Details { get; set; }
        public DateTime ActionDate { get; set; }

        public virtual User User { get; set; }
    }
}
