namespace App.Core.Domain.Security
{
    /// <summary>
    /// Fine-grained permission (controller/action mapping for RBAC).
    /// </summary>
    public class Privilege : BaseEntity
    {
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
