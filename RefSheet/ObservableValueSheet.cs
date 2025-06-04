namespace RefSheet;

public interface IObservableValueSheet : IValueSheet {
    IDisposable OnWrite<TValue>(IObserver<TValue> observer) where TValue : struct;
    IDisposable OnRead<TValue>(IObserver<TValue> observer) where TValue : struct;
    
    IDisposable OnWrite<TKey, TValue>(IObserver<KeyValuePair<TKey, TValue>> observer) where TKey : notnull where TValue : struct;
    IDisposable OnRead<TKey, TValue>(IObserver<KeyValuePair<TKey, TValue>> observer) where TKey : notnull where TValue : struct;
}

public class ObservableValueSheet : IObservableValueSheet, IDisposable {
    readonly IValueSheet backingSheet;

    readonly Dictionary<Type, List<object>> onWriteObservers = new ();
    readonly Dictionary<Type, List<object>> onReadObservers = new ();
    
    readonly Dictionary<(Type, Type), List<object>> keyedOnWriteObservers = new ();
    readonly Dictionary<(Type, Type), List<object>> keyedOnReadObservers = new ();
    
    public ObservableValueSheet() : this(new ValueSheet()) { }
    public ObservableValueSheet(IValueContainerFactory containerFactory) : this(new ValueSheet(containerFactory)) { }
    public ObservableValueSheet(IValueSheet backingSheet) {
        ArgumentNullException.ThrowIfNull(backingSheet);
        this.backingSheet = backingSheet;
    }

    public IDisposable OnWrite<TValue>(IObserver<TValue> observer) where TValue : struct {
        ArgumentNullException.ThrowIfNull(observer);

        var type = typeof(TValue);
        if (!onWriteObservers.TryGetValue(type, out var list)) {
            list = new List<object>();
            onWriteObservers[type] = list;
        }
        
        list.Add(observer);
        return new Unsubscriber<TValue>(list, observer);
    }
    public IDisposable OnRead<TValue>(IObserver<TValue> observer) where TValue : struct {
        ArgumentNullException.ThrowIfNull(observer);

        var type = typeof(TValue);
        if (!onReadObservers.TryGetValue(type, out var list)) {
            list = new List<object>();
            onReadObservers[type] = list;
        }
        
        list.Add(observer);
        return new Unsubscriber<TValue>(list, observer);
    }
    public IDisposable OnWrite<TKey, TValue>(IObserver<KeyValuePair<TKey, TValue>> observer) where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(observer);
        
        var keyedType = (typeof(TKey), typeof(TValue));
        if (!keyedOnWriteObservers.TryGetValue(keyedType, out var list)) {
            list = new List<object>();
            keyedOnWriteObservers[keyedType] = list;
        }
        
        list.Add(observer);
        return new Unsubscriber<TKey, TValue>(list, observer);
    }
    public IDisposable OnRead<TKey, TValue>(IObserver<KeyValuePair<TKey, TValue>> observer) where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(observer);
        
        var keyedType = (typeof(TKey), typeof(TValue));
        if (!keyedOnReadObservers.TryGetValue(keyedType, out var list)) {
            list = new List<object>();
            keyedOnReadObservers[keyedType] = list;
        }
        
        list.Add(observer);
        return new Unsubscriber<TKey, TValue>(list, observer);
    }

    public TValue Read<TValue>() where TValue : struct {
        var value = backingSheet.Read<TValue>();
       
        if (!onReadObservers.TryGetValue(typeof(TValue), out var list))
            return value;
        
        foreach (var observer in list) {
            if (observer is IObserver<TValue> typedObserver) {
                typedObserver.OnNext(value);
            }
        }
        
        return value;
    }
    public void Write<TValue>(TValue value) where TValue : struct {
        backingSheet.Write(value);
        
        if (!onWriteObservers.TryGetValue(typeof(TValue), out var list))
            return;
        
        foreach (var observer in list) {
            if (observer is IObserver<TValue> typedObserver) {
                typedObserver.OnNext(value);
            }
        }
    }
    public TValue Read<TKey, TValue>(TKey key) where TKey : notnull where TValue : struct {
        var value = backingSheet.Read<TKey, TValue>(key);
        
        if (!keyedOnReadObservers.TryGetValue((typeof(TKey), typeof(TValue)), out var list))
            return value;
        
        foreach (var observer in list) {
            if (observer is IObserver<KeyValuePair<TKey, TValue>> typedObserver) {
                typedObserver.OnNext(KeyValuePair.Create(key, value));
            }
        }
        
        return value;
    }
    public void Write<TKey, TValue>(TKey key, TValue value) where TKey : notnull where TValue : struct {
        backingSheet.Write(key, value);
        
        if (!keyedOnWriteObservers.TryGetValue((typeof(TKey), typeof(TValue)), out var list))
            return;
        
        foreach (var observer in list) {
            if (observer is IObserver<KeyValuePair<TKey, TValue>> typedObserver) {
                typedObserver.OnNext(KeyValuePair.Create(key, value));
            }
        }
    }
    public bool HasValue<TValue>() where TValue : struct => 
        backingSheet.HasValue<TValue>();
    public bool HasValue<TKey, TValue>(TKey key) where TKey : notnull where TValue : struct => 
        backingSheet.HasValue<TKey, TValue>(key);

    public sealed class Unsubscriber<TValue> : IDisposable where TValue : struct {
        readonly List<object> observers;
        readonly IObserver<TValue> observer;

        public Unsubscriber(List<object> observers, IObserver<TValue> observer) {
            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose() {
            if (observers.Contains(observer)) {
                observers.Remove(observer);
            }
        }
    }
    
    public sealed class Unsubscriber<TKey, TValue> : IDisposable where TValue : struct {
        readonly List<object> observers;
        readonly IObserver<KeyValuePair<TKey, TValue>> observer;

        public Unsubscriber(List<object> observers, IObserver<KeyValuePair<TKey, TValue>> observer) {
            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose() {
            if (observers.Contains(observer)) {
                observers.Remove(observer);
            }
        }
    }

    public void Dispose() {
        if (backingSheet is IDisposable disposableSheet) {
            disposableSheet.Dispose();
        }
    }
}

public class ValueSheetObserver<TValue> : IObserver<TValue> where TValue : struct {
    readonly Action<TValue> onNext;

    public ValueSheetObserver(Action<TValue> onNext) {
        this.onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
    }

    public void OnCompleted() {
        // No action needed for completion
    }

    public void OnError(Exception error) {
        // Handle error if necessary
    }

    public void OnNext(TValue value) {
        onNext(value);
    }
}
public class ValueSheetObserver<TKey, TValue> : IObserver<KeyValuePair<TKey, TValue>> 
    where TKey : notnull where TValue : struct {
    readonly Action<TKey, TValue> onNext;

    public ValueSheetObserver(Action<TKey, TValue> onNext) {
        this.onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
    }

    public void OnCompleted() {
        // No action needed for completion
    }

    public void OnError(Exception error) {
        // Handle error if necessary
    }

    public void OnNext(KeyValuePair<TKey, TValue> kvp) {
        onNext(kvp.Key, kvp.Value);
    }
}

public static class ObservableValueSheetExtensions {
    public static IDisposable OnWrite<TValue>(this IObservableValueSheet sheet, Action<TValue> action) where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TValue>(action);
        return sheet.OnWrite(observer);
    }
    public static IDisposable OnRead<TValue>(this IObservableValueSheet sheet, Action<TValue> action) where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TValue>(action);
        return sheet.OnRead(observer);
    }
    
    public static IDisposable OnWrite<TKey, TValue>(this IObservableValueSheet sheet, Action<TKey, TValue> action) where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TKey, TValue>(action);
        return sheet.OnWrite(observer);
    }
    public static IDisposable OnRead<TKey, TValue>(this IObservableValueSheet sheet, Action<TKey, TValue> action) where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TKey, TValue>(action);
        return sheet.OnRead(observer);
    }

    public static IDisposable OnWrite<TKey, TValue>(this IObservableValueSheet sheet, TKey key, Action<TValue> action)
        where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TKey, TValue>((observedKey, observedValue) => {
            if (EqualityComparer<TKey>.Default.Equals(observedKey, key)) {
                action(observedValue);
            }
        });
        
        return sheet.OnWrite(observer);
    }
    public static IDisposable OnRead<TKey, TValue>(this IObservableValueSheet sheet, TKey key, Action<TValue> action)
        where TKey : notnull where TValue : struct {
        ArgumentNullException.ThrowIfNull(action);
        
        var observer = new ValueSheetObserver<TKey, TValue>((observedKey, observedValue) => {
            if (EqualityComparer<TKey>.Default.Equals(observedKey, key)) {
                action(observedValue);
            }
        });
        
        return sheet.OnRead(observer);
    }
}