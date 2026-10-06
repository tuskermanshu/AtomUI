// This adapter is compiled into a pinned ILC host; it is not a product runtime dependency.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.IL;
using Internal.TypeSystem;

namespace ILCompiler;

internal sealed class RegistrationDefinitions
{
    // P1 development transport. Product ConditionalRecord ingestion is a separate P2 boundary.
    internal sealed class Manifest
    {
        public int Format { get; set; }
        public GroupDefinition[] Groups { get; set; }
    }

    internal sealed class TypeIdentity
    {
        public string Assembly { get; set; }
        public string Name { get; set; }
    }

    internal sealed class MethodIdentity
    {
        public TypeIdentity Type { get; set; }
        public string Name { get; set; }
    }

    internal sealed class EntryDefinition
    {
        public string Key { get; set; }
        public string FragmentId { get; set; }
        public TypeIdentity Condition { get; set; }
        public MethodIdentity Fragment { get; set; }
    }

    internal sealed class GroupDefinition
    {
        public string Id { get; set; }
        public string PackageId { get; set; }
        public TypeIdentity Builder { get; set; }
        public MethodIdentity Collector { get; set; }
        public EntryDefinition[] Entries { get; set; }
    }

    internal sealed record Entry(EntryDefinition Definition, TypeDesc Condition, MethodDesc Method);

    internal sealed class Group
    {
        public GroupDefinition Definition { get; init; }
        public TypeDesc Builder { get; init; }
        public MethodDesc Collector { get; init; }
        public MethodDesc Entry { get; init; }
        public MethodDesc FullCollector { get; init; }
        public Entry[] Entries { get; init; }
        public string SymbolName { get; init; }
        public RegistrationTableNode Table { get; set; }
        public Entry[] FrozenEntries { get; set; }
        public bool ScannerRequested { get; set; }
    }

    public static Group[] Read(string path, CompilerTypeSystemContext context, ILProvider originalProvider)
    {
        var options = new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(path), options);
        if (manifest?.Format != 1 || manifest.Groups == null || manifest.Groups.Length == 0)
        {
            throw new InvalidOperationException("Unsupported ILC8 development manifest");
        }
        var result = new List<Group>();
        var collectors = new HashSet<MethodDesc>();
        foreach (GroupDefinition definition in manifest.Groups)
        {
            if (string.IsNullOrEmpty(definition.Id) || definition.Entries == null)
            {
                throw new InvalidOperationException("Missing group identity or entries");
            }
            TypeDesc builder = ResolveType(context, definition.Builder);
            if (builder.IsValueType || builder.ContainsSignatureVariables(treatGenericParameterLikeSignatureVariable: true))
            {
                throw new InvalidOperationException("Registration builder must be a closed reference type");
            }
            MethodDesc collector = ResolveMethod(context, definition.Collector, builder);
            if (!collectors.Add(collector))
            {
                throw new InvalidOperationException("Conflicting collector definitions");
            }
            byte[] body = originalProvider.GetMethodIL(collector)?.GetILBytes();
            if (body == null || body.Length != 1 || body[0] != (byte)ILOpcode.ret)
            {
                throw new InvalidOperationException("P1 transport requires the declared neutral collector template");
            }
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var entries = new List<Entry>();
            foreach (EntryDefinition entry in definition.Entries)
            {
                if (string.IsNullOrEmpty(entry.Key) || !keys.Add(entry.Key))
                {
                    throw new InvalidOperationException("Missing or duplicate condition key");
                }
                MethodDesc fragment = ResolveMethod(context, entry.Fragment, builder);
                if (((MetadataType)fragment.OwningType).Module != ((MetadataType)collector.OwningType).Module)
                {
                    throw new InvalidOperationException("Collector and registration thunk must share a defining module");
                }
                // A generated thunk is stateless. User factory/control cctors remain normal IL dependencies.
                if (fragment.OwningType.HasStaticConstructor)
                {
                    throw new InvalidOperationException("Generated registration thunk owner must not have a cctor");
                }
                entries.Add(new Entry(entry, ResolveType(context, entry.Condition), fragment));
            }
            string identity = definition.Collector.Type.Assembly + "/" + definition.Collector.Type.Name + "/" + definition.Collector.Name;
            string symbol = "__atomui_registration_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant();
            result.Add(new Group
            {
                Definition = definition,
                Builder = builder,
                Collector = collector,
                Entry = collector,
                Entries = entries.OrderBy(e => e.Definition.Key, StringComparer.Ordinal).ToArray(),
                SymbolName = symbol
            });
        }
        return result.OrderBy(g => g.SymbolName, StringComparer.Ordinal).ToArray();
    }

    private static TypeDesc ResolveType(CompilerTypeSystemContext context, TypeIdentity identity)
    {
        if (identity?.Assembly == null || identity.Name == null)
        {
            throw new InvalidOperationException("Missing complete type identity");
        }
        ModuleDesc module = context.ResolveAssembly(new AssemblyName(identity.Assembly), true);
        if (!string.Equals(module.Assembly.GetName().FullName, identity.Assembly, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Assembly identity mismatch: " + identity.Assembly);
        }
        TypeDesc type = module.GetTypeByCustomAttributeTypeName(identity.Name);
        if (type.GetTypeDefinition() is not MetadataType definition || definition.Module != module)
        {
            throw new InvalidOperationException("Defining assembly identity mismatch: " + identity.Name);
        }
        return type;
    }

    private static MethodDesc ResolveMethod(CompilerTypeSystemContext context, MethodIdentity identity, TypeDesc builder)
    {
        TypeDesc owner = ResolveType(context, identity?.Type);
        if (owner.HasInstantiation || owner.IsGenericDefinition)
        {
            throw new InvalidOperationException("Registration method owner must be non-generic");
        }
        MethodDesc[] methods = owner.GetMethods().Where(m => m.Name == identity.Name && m.Signature.Flags == MethodSignatureFlags.Static && !m.Signature.HasEmbeddedSignatureData &&
            m.Signature.Length == 1 && m.Signature[0] == builder && m.Signature.ReturnType.IsVoid && !m.HasInstantiation).ToArray();
        if (methods.Length != 1 || methods[0].IsPInvoke || methods[0].IsAbstract || methods[0].IsInternalCall)
        {
            throw new InvalidOperationException("Expected one managed static void registration method with exact builder argument");
        }
        return methods[0];
    }
}
