using System.ComponentModel;
using Avalonia.Controls;

namespace AtomUI.Registration;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum ControlResourcePhase
{
    Shared = 0,
    Control = 1
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlThemeResourceRegistration
{
    public ControlThemeResourceRegistration(string assetId, ControlResourcePhase phase, int order, Func<IResourceProvider> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
        ArgumentNullException.ThrowIfNull(factory);
        if (phase is not (ControlResourcePhase.Shared or ControlResourcePhase.Control))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }
        AssetId = assetId;
        Phase = phase;
        Order = order;
        Factory = factory;
    }

    public string AssetId { get; }
    public ControlResourcePhase Phase { get; }
    public int Order { get; }
    public Func<IResourceProvider> Factory { get; }

    internal bool HasSameMetadata(ControlThemeResourceRegistration other) =>
        AssetId == other.AssetId && Phase == other.Phase && Order == other.Order && Factory.Equals(other.Factory);
}
