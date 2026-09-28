namespace AtomUI.Theme.Schema;

internal static class ControlTokenIdentityCanonicalizer
{
    internal static ControlTokenIdentity Merge(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        if (left != right)
        {
            throw new ArgumentException("Only equal Token identities can be canonicalized.");
        }
        if (left.OwnerType is { } owner && right.OwnerType is { } other && owner != other)
        {
            throw new ThemeSchemaException(
                $"Control Token identity '{left}' has conflicting owners '{owner}' and '{other}'.");
        }
        return left.OwnerType is not null ? left : right;
    }

    internal static ControlTokenIdentity[] Canonicalize(IEnumerable<ControlTokenIdentity> identities)
    {
        var canonical = new Dictionary<ControlTokenIdentity, ControlTokenIdentity>();
        foreach (var identity in identities)
        {
            canonical[identity] = canonical.TryGetValue(identity, out var existing) ? Merge(existing, identity) : identity;
        }
        return canonical.Values.OrderBy(static identity => identity.Catalog, StringComparer.Ordinal)
                               .ThenBy(static identity => identity.Id, StringComparer.Ordinal).ToArray();
    }

    internal static void Set<T>(Dictionary<ControlTokenIdentity, T> entries, ControlTokenIdentity identity, T value)
    {
        if (entries.ContainsKey(identity))
        {
            // Dictionary value replacement preserves its original key. Upgrade the key before replacing the value.
            var previous = entries.Keys.First(key => key == identity);
            identity = Merge(previous, identity);
            entries.Remove(previous);
        }
        entries.Add(identity, value);
    }

    internal static bool Matches(ControlTokenIdentity identity, Type controlType) =>
        identity.OwnerType is null || identity.OwnerType == controlType;
}
