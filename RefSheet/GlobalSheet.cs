namespace RefSheet;

public static class GlobalSheet {
    static IRefSheet? s_refSheet;
    static ObservableValueSheet? s_valueSheet;
    
    public static void Configure(IRefSheet? refSheet = null, IValueSheet? valueSheet = null) {
        if (s_refSheet != null || s_valueSheet != null)
            throw new InvalidOperationException("GlobalSheet has already been configured! Call Reset() to reconfigure.");
        
        s_refSheet = refSheet ?? new RefSheet();
        s_valueSheet = new ObservableValueSheet(valueSheet ?? new ValueSheet());
    }

    public static void Reset() {
        if (s_refSheet is IDisposable disposable)
            disposable.Dispose();
        
        if (s_valueSheet is IDisposable valueDisposable)
            valueDisposable.Dispose();
        
        s_refSheet = null;
        s_valueSheet = null;
    }
    
    public static TRef GetRef<TRef>() where TRef : class {
        ThrowIfNotConfigured();
        return s_refSheet!.Get<TRef>();
    }
    public static void SetRef<TRef>(TRef refValue) where TRef : class {
        ThrowIfNotConfigured();
        s_refSheet!.Set(refValue);
    }
    
    public static TRef GetRef<TKey, TRef>(TKey key) where TKey : notnull where TRef : class {
        ThrowIfNotConfigured();
        return s_refSheet!.Get<TKey, TRef>(key);
    }
    public static void SetRef<TKey, TRef>(TKey key, TRef refValue) where TKey : notnull where TRef : class {
        ThrowIfNotConfigured();
        s_refSheet!.Set(key, refValue);
    }
    public static bool HasRef<TRef>() where TRef : class {
        ThrowIfNotConfigured();
        return s_refSheet!.HasRef<TRef>();
    }
    public static bool HasRef<TKey, TRef>(TKey key) where TKey : notnull where TRef : class {
        ThrowIfNotConfigured();
        return s_refSheet!.HasRef<TKey, TRef>(key);
    }
    
    public static TValue ReadValue<TValue>() where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.Read<TValue>();
    }
    public static void WriteValue<TValue>(TValue value) where TValue : struct {
        ThrowIfNotConfigured();
        s_valueSheet!.Write(value);
    }
    public static TValue ReadValue<TKey, TValue>(TKey key) where TKey : notnull where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.Read<TKey, TValue>(key);
    }
    public static void WriteValue<TKey, TValue>(TKey key, TValue value) where TKey : notnull where TValue : struct {
        ThrowIfNotConfigured();
        s_valueSheet!.Write(key, value);
    }
    public static bool HasValue<TValue>() where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.HasValue<TValue>();
    }
    public static bool HasValue<TKey, TValue>(TKey key) where TKey : notnull where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.HasValue<TKey, TValue>(key);
    }
    
    public static IDisposable OnWrite<TValue>(Action<TValue> onWrite) where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.OnWrite(onWrite);
    }
    public static IDisposable OnRead<TValue>(Action<TValue> onRead) where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.OnRead(onRead);
    }
    public static IDisposable OnWrite<TKey, TValue>(TKey key, Action<TValue> onWrite) where TKey : notnull where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.OnWrite(key, onWrite);
    }
    public static IDisposable OnRead<TKey, TValue>(TKey key, Action<TValue> onRead) where TKey : notnull where TValue : struct {
        ThrowIfNotConfigured();
        return s_valueSheet!.OnRead(key, onRead);
    }
    
    static void ThrowIfNotConfigured() {
        if (s_refSheet is null || s_valueSheet is null) {
            throw new InvalidOperationException("GlobalSheet has not been configured! Call Configure() first.");
        }
    }
}