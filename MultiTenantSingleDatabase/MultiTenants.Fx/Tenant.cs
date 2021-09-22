
using System.Collections.Generic;

namespace MultiTenants.Fx;

public record Tenant(int Id, string Identifier)
{
    public Dictionary<string, object> Items { get; init; } =
        new Dictionary<string, object>();
}
