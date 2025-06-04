using LH.RefSheet;

namespace RefSheet.Tests;

[TestFixture]
public class GlobalSheetTests {
    class Dummy {
        public int Value { get; set; }
    }

    [SetUp]
    public void SetUp() {
        GlobalSheet.Configure();
    }

    [TearDown]
    public void TearDown() {
        GlobalSheet.Reset();
    }

    [Test]
    public void SetAndGetRef_Unkeyed_Works() {
        var dummy = new Dummy { Value = 42 };
        GlobalSheet.SetRef(dummy);
        Assert.That(GlobalSheet.GetRef<Dummy>(), Is.EqualTo(dummy));
    }

    [Test]
    public void SetAndGetRef_Keyed_Works() {
        var dummy = new Dummy { Value = 99 };
        GlobalSheet.SetRef("foo", dummy);
        Assert.That(GlobalSheet.GetRef<string, Dummy>("foo"), Is.EqualTo(dummy));
    }

    [Test]
    public void HasRef_ReturnsCorrectly() {
        Assert.That(GlobalSheet.HasRef<Dummy>(), Is.False);
        GlobalSheet.SetRef(new Dummy());
        Assert.That(GlobalSheet.HasRef<Dummy>(), Is.True);
    }

    [Test]
    public void HasRef_Keyed_ReturnsCorrectly() {
        Assert.That(GlobalSheet.HasRef<string, Dummy>("bar"), Is.False);
        GlobalSheet.SetRef("bar", new Dummy());
        Assert.That(GlobalSheet.HasRef<string, Dummy>("bar"), Is.True);
    }

    [Test]
    public void WriteAndReadValue_Unkeyed_Works() {
        GlobalSheet.WriteValue(123);
        Assert.That(GlobalSheet.ReadValue<int>(), Is.EqualTo(123));
    }

    [Test]
    public void WriteAndReadValue_Keyed_Works() {
        GlobalSheet.WriteValue("pi", 3.14);
        Assert.That(GlobalSheet.ReadValue<string, double>("pi"), Is.EqualTo(3.14));
    }

    [Test]
    public void HasValue_ReturnsCorrectly() {
        Assert.That(GlobalSheet.HasValue<int>(), Is.False);
        GlobalSheet.WriteValue(1);
        Assert.That(GlobalSheet.HasValue<int>(), Is.True);
    }

    [Test]
    public void HasValue_Keyed_ReturnsCorrectly() {
        Assert.That(GlobalSheet.HasValue<string, int>("x"), Is.False);
        GlobalSheet.WriteValue("x", 7);
        Assert.That(GlobalSheet.HasValue<string, int>("x"), Is.True);
    }

    [Test]
    public void OnWrite_Unkeyed_ObserverReceivesValue() {
        int received = 0;
        using (GlobalSheet.OnWrite<int>(v => received = v)) {
            GlobalSheet.WriteValue(42);
            Assert.That(received, Is.EqualTo(42));
        }
    }

    [Test]
    public void OnRead_Unkeyed_ObserverReceivesValue() {
        GlobalSheet.WriteValue(77);
        int received = 0;
        using (GlobalSheet.OnRead<int>(v => received = v)) {
            var value = GlobalSheet.ReadValue<int>();
            Assert.That(received, Is.EqualTo(value));
        }
    }

    [Test]
    public void OnWrite_Keyed_ObserverReceivesValue() {
        int received = 0;
        using (GlobalSheet.OnWrite("foo", (int v) => received = v)) {
            GlobalSheet.WriteValue("foo", 99);
            Assert.That(received, Is.EqualTo(99));
        }
    }

    [Test]
    public void OnRead_Keyed_ObserverReceivesValue() {
        GlobalSheet.WriteValue("bar", 88);
        int received = 0;
        using (GlobalSheet.OnRead("bar", (int v) => received = v)) {
            var value = GlobalSheet.ReadValue<string, int>("bar");
            Assert.That(received, Is.EqualTo(value));
        }
    }

    [Test]
    public void ThrowsIfNotConfigured() {
        // Save current state
        var type = typeof(GlobalSheet);
        var refField = type.GetField("s_refSheet",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var valueField = type.GetField("s_valueSheet",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var oldRef = refField.GetValue(null);
        var oldValue = valueField.GetValue(null);

        // Unset
        refField.SetValue(null, null);
        valueField.SetValue(null, null);

        try {
            Assert.Throws<InvalidOperationException>(() => GlobalSheet.GetRef<Dummy>());
            Assert.Throws<InvalidOperationException>(() => GlobalSheet.SetRef(new Dummy()));
            Assert.Throws<InvalidOperationException>(() => GlobalSheet.ReadValue<int>());
            Assert.Throws<InvalidOperationException>(() => GlobalSheet.WriteValue(1));
            Assert.Throws<InvalidOperationException>(() => GlobalSheet.OnWrite<int>(_ => { }));
        }
        finally {
            // Restore
            refField.SetValue(null, oldRef);
            valueField.SetValue(null, oldValue);
        }
    }
}