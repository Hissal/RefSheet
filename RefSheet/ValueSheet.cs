namespace RefSheet;

public interface IValueSheet {
    TValue Read<TValue>() where TValue : struct;
    void Write<TValue>(TValue value) where TValue : struct;

    TValue Read<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull;
    void Write<TKey, TValue>(TKey key, TValue value) where TValue : struct where TKey : notnull;

    bool HasValue<TValue>() where TValue : struct;
    bool HasValue<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull;
}

public class ValueSheet : IValueSheet, IDisposable {
    readonly ValueSheetCore core;

    public ValueSheet() : this(new ValueSheetCore(new ValueContainerFactory())) { }
    public ValueSheet(IValueContainerFactory containerFactory) : this(new ValueSheetCore(containerFactory)) { }
    public ValueSheet(ValueSheetCore core) {
        this.core = core ?? throw new ArgumentNullException(nameof(core));
    }
    
    public TValue Read<TValue>() where TValue : struct =>
        core.Read<TValue>();
    public void Write<TValue>(TValue value) where TValue : struct =>
        core.Write(value);

    public TValue Read<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull =>
        core.Read<TKey, TValue>(key);
    public void Write<TKey, TValue>(TKey key, TValue value) where TValue : struct where TKey : notnull =>
        core.Write(key, value);

    public bool HasValue<TValue>() where TValue : struct =>
        core.HasValue<TValue>();
    public bool HasValue<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull =>
        core.HasValue<TKey, TValue>(key);

    public void Dispose() {
        core.Dispose();
    }
}

public class ValueSheetCore : IDisposable {
    readonly Dictionary<Type, IValueContainer> valueContainers = new();
    readonly Dictionary<(Type keyType, Type ValueType), IKeyedValueContainer> keyedValueContainers = new();

    readonly IValueContainerFactory containerFactory;
    
    public ValueSheetCore(IValueContainerFactory containerFactory) {
        this.containerFactory = containerFactory ?? throw new ArgumentNullException(nameof(containerFactory));
    }
    
    static (Type, Type) GetKeyedContainerKey<TKey, TValue>() where TKey : notnull where TValue : struct =>
        (typeof(TKey), typeof(TValue));

    public TValue Read<TValue>() where TValue : struct =>
        valueContainers.TryGetValue(typeof(TValue), out var container)
            ? ((ValueContainer<TValue>)container).Read()
            : default;

    public void Write<TValue>(TValue value) where TValue : struct {
        if (!valueContainers.TryGetValue(typeof(TValue), out var container)) {
            container = containerFactory.CreateValueContainer<TValue>();
            valueContainers[typeof(TValue)] = container;
        }

        ((IValueContainer<TValue>)container).Write(value);
    }

    public TValue Read<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull =>
        keyedValueContainers.TryGetValue(GetKeyedContainerKey<TKey, TValue>(), out var container) &&
        container is IKeyedValueContainer<TKey, TValue> keyedContainer
            ? keyedContainer.Read(key)
            : default;

    public void Write<TKey, TValue>(TKey key, TValue value) where TValue : struct where TKey : notnull {
        var dictKey = GetKeyedContainerKey<TKey, TValue>();
        if (!keyedValueContainers.TryGetValue(dictKey, out var container)) {
            container = containerFactory.CreateKeyedValueContainer<TKey, TValue>();
            keyedValueContainers[dictKey] = container;
        }

        ((IKeyedValueContainer<TKey, TValue>)container).Write(key, value);
    }

    public bool HasValue<TValue>() where TValue : struct => valueContainers.ContainsKey(typeof(TValue));

    public bool HasValue<TKey, TValue>(TKey key) where TValue : struct where TKey : notnull =>
        keyedValueContainers.TryGetValue(GetKeyedContainerKey<TKey, TValue>(), out var container) &&
        container is IKeyedValueContainer<TKey, TValue> keyedContainer && 
        keyedContainer.HasKey(key);

    public void Dispose() {
        foreach (var container in valueContainers.Values) {
            if (container is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        valueContainers.Clear();

        foreach (var container in keyedValueContainers.Values) {
            if (container is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        keyedValueContainers.Clear();
        
        GC.SuppressFinalize(this);
    }
}

public interface IValueContainerFactory {
    IValueContainer<TValue> CreateValueContainer<TValue>() where TValue : struct;
    IKeyedValueContainer<TKey, TValue> CreateKeyedValueContainer<TKey, TValue>()
        where TValue : struct where TKey : notnull;
}

public class ValueContainerFactory : IValueContainerFactory {
    public IValueContainer<TValue> CreateValueContainer<TValue>() where TValue : struct {
        return new ValueContainer<TValue>();
    }

    public IKeyedValueContainer<TKey, TValue> CreateKeyedValueContainer<TKey, TValue>()
        where TValue : struct where TKey : notnull {
        return new KeyedValueContainer<TKey, TValue>();
    }
}

public static class ValueSheetExtensions {
    public static bool TryRead<TValue>(this IValueSheet valueSheet, out TValue value) where TValue : struct {
        if (valueSheet.HasValue<TValue>()) {
            value = valueSheet.Read<TValue>();
            return true;
        }
        
        value = default;
        return false;
    }
    public static bool TryRead<TKey, TValue>(this IValueSheet valueSheet, TKey key, out TValue value)
        where TValue : struct where TKey : notnull {
        if (valueSheet.HasValue<TKey, TValue>(key)) {
            value = valueSheet.Read<TKey, TValue>(key);
            return true;
        }

        value = default;
        return false;
    }
    
    public static TValue GetValueOrDefault<TValue>(this IValueSheet valueSheet) where TValue : struct => 
        valueSheet.TryRead(out TValue value) ? value : default;
    public static TValue GetValueOrDefault<TKey, TValue>(this IValueSheet valueSheet, TKey key) where TValue : struct where TKey : notnull =>
        valueSheet.TryRead(key, out TValue value) ? value : default;
    
    public static bool TryReadNonDefault<TValue>(this IValueSheet valueSheet, out TValue value) where TValue : struct {
        if (valueSheet.TryRead(out value)) 
            return !EqualityComparer<TValue>.Default.Equals(value, default);
        
        return false;
    }
    public static bool TryReadNonDefault<TKey, TValue>(this IValueSheet valueSheet, TKey key, out TValue value)
        where TValue : struct where TKey : notnull {
        if (valueSheet.TryRead(key, out value)) 
            return !EqualityComparer<TValue>.Default.Equals(value, default);
        
        return false;
    }
}

public interface IValueContainer {
    Type ValueType { get; }
}

public interface IValueContainer<TValue> : IValueContainer where TValue : struct {
    TValue Read();
    void Write(TValue value);
}

public interface IKeyedValueContainer {
    Type KeyType { get; }
    Type ValueType { get; }
}

public interface IKeyedValueContainer<in TKey, TValue> : IKeyedValueContainer
    where TValue : struct where TKey : notnull {
    TValue Read(TKey key);
    void Write(TKey key, TValue value);
    bool HasKey(TKey key);
}

public class ValueContainer<TValue> : IValueContainer<TValue>, IDisposable where TValue : struct {
    TValue containedValue;

    public Type ValueType => typeof(TValue);
    public TValue Read() => containedValue;
    public void Write(TValue value) => containedValue = value;
    public void Dispose() {
        if (containedValue is IDisposable disposable)
            disposable.Dispose();
        
        GC.SuppressFinalize(this);
    }
}

public class KeyedValueContainer<TKey, TValue> : IKeyedValueContainer<TKey, TValue>, IDisposable
    where TValue : struct where TKey : notnull {
    readonly Dictionary<TKey, TValue> keyedValues = new();

    public Type KeyType => typeof(TKey);
    public Type ValueType => typeof(TValue);

    public TValue Read(TKey key) => keyedValues.GetValueOrDefault(key);
    public void Write(TKey key, TValue value) => keyedValues[key] = value;
    public bool HasKey(TKey key) => keyedValues.ContainsKey(key);
    public void Dispose() {
        foreach (var value in keyedValues.Values) {
            if (value is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        keyedValues.Clear();
        
        GC.SuppressFinalize(this);
    }
}