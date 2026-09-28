namespace AtomUI.Registration;

internal sealed class ControlRegistrationState
{
    private readonly Dictionary<string, bool> _packages = new(StringComparer.Ordinal);
    private Exception? _failure;
    private bool _frozen;

    internal void ThrowIfUnavailable()
    {
        if (_failure is not null)
        {
            throw new InvalidOperationException("Control package registration failed; this builder cannot be used again.", _failure);
        }
        if (_frozen)
        {
            throw new InvalidOperationException("Control package registration is already frozen.");
        }
    }

    internal bool IsStaged(string packageId)
    {
        ThrowIfUnavailable();
        return _packages.TryGetValue(packageId, out var staged) && staged;
    }

    internal void Enter(string packageId)
    {
        ThrowIfUnavailable();
        if (!_packages.TryAdd(packageId, false))
        {
            throw new InvalidOperationException($"Control package '{packageId}' is already registered or being registered.");
        }
    }

    internal void Stage(string packageId)
    {
        ThrowIfUnavailable();
        _packages[packageId] = true;
    }

    internal void Freeze()
    {
        ThrowIfUnavailable();
        if (_packages.Values.Any(static staged => !staged))
        {
            throw new InvalidOperationException("Cannot build while a control package is being registered.");
        }
        _frozen = true;
    }

    internal void Fail(Exception exception) => _failure ??= exception;
}
