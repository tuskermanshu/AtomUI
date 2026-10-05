namespace AtomUI.Generator;

// Compiled-contract domain only; final exact-AQN fingerprints are materialized in Core.
// Must match ThemeSchemaRegistry.SchemaFingerprintBuilder: each UTF-16 unit and integer is widened
// to UInt64 and added as eight little-endian bytes. This is deliberately not byte/UTF-8 hashing.
internal static class RegistrationFingerprint
{
    internal static ulong ComputeContract(RegistrationAsset asset, IReadOnlyList<string> globalNames)
    {
        ulong hash = 0;
        void Number(ulong value)
        {
            if (hash == 0)
            {
                hash = 14695981039346656037UL;
            }

            for (var index = 0; index < 8; index++) { hash = unchecked((hash ^ (byte)value) * 1099511628211UL); value >>= 8; }
        }
        void Text(string value) { Number((ulong)value.Length); foreach (var ch in value) { Number(ch); } }
        void Owner(RegistrationOwner owner) { Text(owner.Catalog); Text(owner.Id); Text(owner.Type.MetadataName); }
        Text("AtomUI.GeneratedControlThemeAssetContract"); Number(1); Text(asset.Id); Text(asset.Uri); Number((ulong)asset.Exports.Count);
        foreach (var export in asset.Exports.OrderBy(value => value.Target.MetadataName, StringComparer.Ordinal)
                     .ThenBy(value => value.KeyType is null ? 1 : 0)
                     .ThenBy(value => value.KeyType?.MetadataName ?? value.Key, StringComparer.Ordinal))
        { Text(export.Target.MetadataName); Number(export.KeyType is null ? 0UL : 1UL); Text(export.KeyType?.MetadataName ?? export.Key!); }
        Number((ulong)asset.Owners.Count); foreach (var owner in asset.Owners.OrderBy(value => value.Catalog, StringComparer.Ordinal).ThenBy(value => value.Id, StringComparer.Ordinal))
        {
            Owner(owner);
        }

        Number((ulong)asset.Bindings.Count); foreach (var binding in asset.Bindings.OrderBy(value => value.Owner.Catalog, StringComparer.Ordinal).ThenBy(value => value.Owner.Id, StringComparer.Ordinal)
                     .ThenBy(value => value.Property, StringComparer.Ordinal).ThenBy(value => value.Target.MetadataName, StringComparer.Ordinal))
        { Owner(binding.Owner); Text(binding.Property); Text(binding.Target.MetadataName); }
        Number((ulong)globalNames.Count); foreach (var name in globalNames)
        {
            Text(name);
        }

        return hash == 0 ? 14695981039346656037UL : hash;
    }
}
