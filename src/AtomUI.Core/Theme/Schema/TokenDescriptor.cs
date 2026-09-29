using System.ComponentModel;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Schema;

public sealed class TokenDescriptor
{
    private readonly TokenValueCodec _codec;
    private readonly TokenValueAccessor _accessor;

    public TokenDescriptor(
        string name,
        int slot,
        TokenStage stage,
        Type valueType,
        object resourceKey,
        Func<string, object?> parser,
        Func<object?, string> formatter,
        Func<AbstractDesignToken, object?> getter,
        Action<AbstractDesignToken, object?> setter,
        Func<AbstractDesignToken, object?> resourceProjector)
    {
        SchemaIdentifier.Validate(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentNullException.ThrowIfNull(valueType);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(getter);
        ArgumentNullException.ThrowIfNull(setter);
        ArgumentNullException.ThrowIfNull(resourceProjector);

        Name        = name;
        Slot        = slot;
        Stage       = stage;
        ValueType   = valueType;
        ResourceKey = resourceKey;
        _codec      = new DelegateTokenValueCodec(valueType, parser, formatter);
        _accessor   = new DelegateTokenValueAccessor(getter, setter, resourceProjector);
    }

    private TokenDescriptor(
        string name,
        int slot,
        TokenStage stage,
        TokenValueCodec codec,
        object resourceKey,
        TokenValueAccessor accessor)
    {
        SchemaIdentifier.Validate(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentNullException.ThrowIfNull(codec);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(accessor);

        Name        = name;
        Slot        = slot;
        Stage       = stage;
        ValueType   = codec.ValueType;
        ResourceKey = resourceKey;
        _codec      = codec;
        _accessor   = accessor;
    }

    /// <summary>
    /// Generated-code entry point: the value codec is shared per value type and the accessor per token class,
    /// so a descriptor carries no per-token delegates.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TokenDescriptor CreateGenerated(
        string name,
        int slot,
        TokenStage stage,
        TokenValueCodec codec,
        object resourceKey,
        TokenValueAccessor accessor) =>
        new(name, slot, stage, codec, resourceKey, accessor);

    public string Name { get; }
    public int Slot { get; }
    public TokenStage Stage { get; }
    public Type ValueType { get; }
    public object ResourceKey { get; }

    public object? Parse(string value) => _codec.Parse(value);

    public string Format(object? value) => _codec.Format(value);

    public object? GetValue(AbstractDesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return _accessor.GetValue(token, Slot);
    }

    public void SetValue(AbstractDesignToken token, object? value)
    {
        ArgumentNullException.ThrowIfNull(token);
        _accessor.SetValue(token, Slot, value);
    }

    public object? ProjectResourceValue(AbstractDesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return _accessor.ProjectResourceValue(token, Slot);
    }
}
