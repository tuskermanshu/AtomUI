using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Theme.Resources;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ControlRegistrationRuntime
{
    public const int AbiVersion = 1;
    internal const string TrimmedSwitchName = "AtomUI.Registration.Trimmed";

    [FeatureSwitchDefinition(TrimmedSwitchName)]
    public static bool IsTrimmed => AppContext.TryGetSwitch(TrimmedSwitchName, out var trimmed) && trimmed;

    public static IAtomUIBuilder RegisterPackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: false);

    public static IAtomUIBuilder EnsurePackage(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare = null,
        Action<IAtomUIBuilder>? complete = null) =>
        Register(builder, packageId, createProvider, collect, prepare, complete, ensure: true);

    public static void CollectFragments(
        ControlPackageRegistrationBuilder builder,
        IReadOnlyDictionary<string, Type> map,
        IReadOnlyList<string> candidateKeys)
    {
        ArgumentNullException.ThrowIfNull(builder);
        try
        {
            ArgumentNullException.ThrowIfNull(map);
            ArgumentNullException.ThrowIfNull(candidateKeys);
            var selected = new HashSet<Type>();
            for (var index = 0; index < candidateKeys.Count; index++)
            {
                if (!map.TryGetValue(candidateKeys[index], out var proxy) || !selected.Add(proxy))
                {
                    continue;
                }
                if (!proxy.IsSealed || !typeof(ControlRegistrationFragmentAttribute).IsAssignableFrom(proxy) || proxy.ContainsGenericParameters)
                {
                    throw new InvalidOperationException($"Invalid control registration proxy '{proxy}'.");
                }
                var fragment = proxy.GetCustomAttribute<ControlRegistrationFragmentAttribute>(inherit: false);
                if (fragment is null || fragment.GetType() != proxy)
                {
                    throw new InvalidOperationException($"Control registration proxy '{proxy}' must have its own fragment Attribute.");
                }
                builder.AddFragment(fragment);
            }
        }
        catch (Exception exception)
        {
            builder.Fail(exception);
            throw;
        }
    }

    private static IAtomUIBuilder Register(
        IAtomUIBuilder builder,
        string packageId,
        Func<IControlThemesProvider> createProvider,
        Action<ControlPackageRegistrationBuilder> collect,
        Action<IAtomUIBuilder>? prepare,
        Action<IAtomUIBuilder>? complete,
        bool ensure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not AtomUIBuilder root)
        {
            throw new ArgumentException("Generated control packages require the application AtomUI builder.", nameof(builder));
        }
        var state = root.ControlRegistrationState;
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
            ArgumentNullException.ThrowIfNull(createProvider);
            ArgumentNullException.ThrowIfNull(collect);
            if (ensure && state.IsStaged(packageId))
            {
                return builder;
            }
            state.Enter(packageId);
            prepare?.Invoke(builder);
            state.ThrowIfUnavailable();
            var provider = createProvider();
            ArgumentNullException.ThrowIfNull(provider);
            var collection = new ControlPackageRegistrationBuilder();
            collect(collection);
            state.ThrowIfUnavailable();
            builder.Theme.AddControlPackage(collection.Build(packageId, provider));
            complete?.Invoke(builder);
            state.Stage(packageId);
            return builder;
        }
        catch (Exception exception)
        {
            state.Fail(exception);
            throw;
        }
    }
}
