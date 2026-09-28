using System.ComponentModel;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlPackageRegistrationBuilder
{
    private readonly List<ControlTokenDescriptor> _controls = [];
    private readonly List<ControlSemanticDescriptor> _semantics = [];
    private readonly Dictionary<string, ControlThemeAssetDescriptor> _assets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ControlThemeResourceRegistration> _resources = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> _fragments = new(StringComparer.Ordinal);
    private Exception? _failure;
    private bool _sealed;

    internal ControlPackageRegistrationBuilder() { }

    public void AddControl(ControlTokenDescriptor descriptor) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _controls.Add(descriptor);
    });

    public void AddSemanticControl(ControlSemanticDescriptor descriptor) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _semantics.Add(descriptor);
    });

    public void AddThemeAsset(ControlThemeAssetDescriptor descriptor) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_assets.TryGetValue(descriptor.AssetId, out var existing))
        {
            if (!existing.HasSameMetadata(descriptor))
            {
                throw new InvalidOperationException($"Theme asset '{descriptor.AssetId}' has conflicting metadata.");
            }
            return;
        }
        _assets.Add(descriptor.AssetId, descriptor);
    });

    public void AddResource(ControlThemeResourceRegistration registration) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (_resources.TryGetValue(registration.AssetId, out var existing))
        {
            if (!existing.HasSameMetadata(registration))
            {
                throw new InvalidOperationException($"Theme resource '{registration.AssetId}' has conflicting metadata or factory.");
            }
            return;
        }
        _resources.Add(registration.AssetId, registration);
    });

    public void AddThemeAsset(ControlThemeAssetDescriptor descriptor, ControlThemeResourceRegistration registration) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(registration);
        if (descriptor.AssetId != registration.AssetId)
        {
            throw new ArgumentException("Theme asset and resource registration must have the same AssetId.");
        }
        AddThemeAsset(descriptor);
        AddResource(registration);
    });

    public void AddFragment(ControlRegistrationFragmentAttribute fragment) => Mutate(() =>
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment.FragmentId);
        var type = fragment.GetType();
        if (_fragments.TryGetValue(fragment.FragmentId, out var existing))
        {
            if (existing != type)
            {
                throw new InvalidOperationException($"Registration fragment '{fragment.FragmentId}' has conflicting proxy types.");
            }
            return;
        }
        _fragments.Add(fragment.FragmentId, type);
        fragment.Add(this);
    });

    internal ControlPackageRegistration Build(string id, IControlThemesProvider provider)
    {
        ThrowIfUnavailable();
        _sealed = true;
        foreach (var asset in _assets.Values)
        {
            if (!_resources.ContainsKey(asset.AssetId))
            {
                throw new InvalidOperationException($"Theme asset '{asset.AssetId}' has no resource factory.");
            }
        }
        return new(id, _controls, _assets.Values, _semantics, provider, _resources.Values);
    }

    internal void Fail(Exception exception) => _failure ??= exception;

    private void Mutate(Action action)
    {
        ThrowIfUnavailable();
        try { action(); }
        catch (Exception exception)
        {
            _failure ??= exception;
            throw;
        }
    }

    private void ThrowIfUnavailable()
    {
        if (_failure is not null || _sealed)
        {
            throw new InvalidOperationException("The package collection is failed or already staged.", _failure);
        }
    }
}
