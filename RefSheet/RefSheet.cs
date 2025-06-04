namespace LH.RefSheet;

public interface IRefSheet {
    TRef Get<TRef>() where TRef : class;
    void Set<TRef>(TRef? value) where TRef : class;

    TRef Get<TKey, TRef>(TKey key) where TRef : class where TKey : notnull;
    void Set<TKey, TRef>(TKey key, TRef? value) where TRef : class where TKey : notnull;

    bool HasRef<TRef>() where TRef : class;
    bool HasRef<TKey, TRef>(TKey key) where TRef : class where TKey : notnull;
}

public class RefSheet : IRefSheet, IDisposable {
    readonly RefSheetCore core;
   
    public RefSheet() : this(new RefSheetCore(new KeyedRefContainerFactory())) { }
    public RefSheet(IKeyedRefContainerFactory keyedContainerFactory) : this(new RefSheetCore(keyedContainerFactory)) { }
    public RefSheet(RefSheetCore core) {
        this.core = core ?? throw new ArgumentNullException(nameof(core));
    }
    
    public TRef Get<TRef>() where TRef : class =>
        core.Get<TRef>();
    public void Set<TRef>(TRef? value) where TRef : class =>
        core.Set(value);
    
    public TRef Get<TKey, TRef>(TKey key) where TRef : class where TKey : notnull =>
        core.Get<TKey, TRef>(key);
    public void Set<TKey, TRef>(TKey key, TRef? value) where TRef : class where TKey : notnull =>
        core.Set(key, value);
    
    public bool HasRef<TRef>() where TRef : class =>
        core.HasRef<TRef>();
    public bool HasRef<TKey, TRef>(TKey key) where TRef : class where TKey : notnull =>
        core.HasRef<TKey, TRef>(key);

    public void Dispose() {
        core.Dispose();
    }
}

public class RefSheetCore : IDisposable {
    readonly Dictionary<Type, object> refs = new();
    readonly Dictionary<(Type, Type), IKeyedRefContainer> keyedRefContainers = new();
    
    readonly IKeyedRefContainerFactory keyedContainerFactory;
    
    public RefSheetCore(IKeyedRefContainerFactory keyedContainerFactory) {
        this.keyedContainerFactory = keyedContainerFactory ?? throw new ArgumentNullException(nameof(keyedContainerFactory));
    }
    
    (Type, Type) GetKeyedContainerKey<TKey, TRef>() where TKey : notnull where TRef : class =>
        (typeof(TRef), typeof(TKey));
    
    IKeyedRefContainer<TKey, TRef> CreateKeyedContainer<TKey, TRef>() where TKey : notnull where TRef : class {
        var container = keyedContainerFactory.Create<TKey, TRef>();
        keyedRefContainers[GetKeyedContainerKey<TKey, TRef>()] = container;
        return container;
    }
    
    public TRef Get<TRef>() where TRef : class => (TRef)refs[typeof(TRef)];

    public void Set<TRef>(TRef? value) where TRef : class {
        if (value is null) refs.Remove(typeof(TRef));
        else refs[typeof(TRef)] = value;
    }

    public TRef Get<TKey, TRef>(TKey key) where TRef : class where TKey : notnull {
        if (keyedRefContainers.TryGetValue(GetKeyedContainerKey<TKey, TRef>(), out var container))
            return ((IKeyedRefContainer<TKey, TRef>)container).Get(key);
        
        throw new NullReferenceException("Reference not found for key: " + key);
    }
    public void Set<TKey, TRef>(TKey key, TRef? value) where TRef : class where TKey : notnull {
        if (!keyedRefContainers.TryGetValue(GetKeyedContainerKey<TKey, TRef>(), out var container))
            container = CreateKeyedContainer<TKey, TRef>();

        ((IKeyedRefContainer<TKey, TRef>)container).Set(key, value);
    }

    public bool HasRef<TRef>() where TRef : class => refs.ContainsKey(typeof(TRef));
    public bool HasRef<TKey, TRef>(TKey key) where TRef : class where TKey : notnull => 
        keyedRefContainers.TryGetValue(GetKeyedContainerKey<TKey, TRef>(), out var container) &&
        ((IKeyedRefContainer<TKey, TRef>)container).HasRef(key);

    public IKeyedRefContainer<TKey, TRef> GetKeyedContainer<TKey, TRef>() where TKey : notnull where TRef : class {
        if (keyedRefContainers.TryGetValue(GetKeyedContainerKey<TKey, TRef>(), out var container))
            return (IKeyedRefContainer<TKey, TRef>)container;

        var newContainer = CreateKeyedContainer<TKey, TRef>();
        return newContainer;
    }

    public void Dispose() {
        foreach (var reference in refs.Values) {
            if (reference is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        refs.Clear();
        
        foreach (var container in keyedRefContainers.Values) {
            if (container is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        keyedRefContainers.Clear();
        
        GC.SuppressFinalize(this);
    }
}

public interface IKeyedRefContainerFactory {
    public IKeyedRefContainer<TKey, TRef> Create<TKey, TRef>() where TRef : class where TKey : notnull;
}

public class KeyedRefContainerFactory : IKeyedRefContainerFactory {
    public IKeyedRefContainer<TKey, TRef> Create<TKey, TRef>() where TKey : notnull where TRef : class {
        return new KeyedRefContainer<TKey, TRef>();
    }
}

public interface IKeyedRefContainer {
    Type KeyType { get; }
    Type RefType { get; }
}
public interface IKeyedRefContainer<in TKey, TRef> : IKeyedRefContainer where TRef : class where TKey : notnull {
    TRef Get(TKey key);
    void Set(TKey key, TRef? value);
    bool HasRef(TKey key);
}

public class KeyedRefContainer<TKey, TRef> : IKeyedRefContainer<TKey, TRef>, IDisposable where TKey : notnull where TRef : class {
    public Type KeyType => typeof(TKey);
    public Type RefType => typeof(TRef);

    readonly Dictionary<TKey, TRef> refs = new();
    
    public TRef Get(TKey key) =>
        refs.GetValueOrDefault(key) ?? throw new NullReferenceException("Reference not found for key: " + key);
    public void Set(TKey key, TRef? value) {
        if (value == null) refs.Remove(key);
        else refs[key] = value;
    }
    public bool HasRef(TKey key) => refs.ContainsKey(key);
    public void Dispose() {
        foreach (var reference in refs.Values) {
            if (reference is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        refs.Clear();
        
        GC.SuppressFinalize(this);
    }
}

public static class RefSheetExtensions {
    public static bool TryGet<TRef>(this IRefSheet refSheet, out TRef? reference) where TRef : class {
        if (refSheet.HasRef<TRef>()) {
            reference = refSheet.Get<TRef>();
            return true;
        }
        
        reference = null;
        return false;
    }
    public static bool TryGet<TKey, TRef>(this IRefSheet refSheet, TKey key, out TRef? reference) where TRef : class where TKey : notnull {
        if (refSheet.HasRef<TKey, TRef>(key)) {
            reference = refSheet.Get<TKey, TRef>(key);
            return true;
        }
        
        reference = null;
        return false;
    }
    
    public static TRef? GetRefOrNull<TRef>(this IRefSheet refSheet) where TRef : class => 
        refSheet.TryGet(out TRef? value) ? value : null;
    
    public static TRef? GetRefOrNull<TKey, TRef>(this IRefSheet refSheet, TKey key) where TRef : class where TKey : notnull => 
        refSheet.TryGet(key, out TRef? value) ? value : null;
}