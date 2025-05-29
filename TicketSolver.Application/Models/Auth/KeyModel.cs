namespace TicketSolver.Application.Models.Auth;

public class KeyModel
{
    public Guid TenantKey { get; set; } = Guid.Empty;
    public int TypeKey { get; set; } = 0;
    public KeyModel(Guid tenantAdminKey, int typeKey)
    {
        TypeKey = typeKey;
        TenantKey = tenantAdminKey;
    }
    public KeyModel(Guid tenantAdminKey)
    {
        TenantKey = tenantAdminKey;
    }
    public KeyModel()
    {
    }
}
