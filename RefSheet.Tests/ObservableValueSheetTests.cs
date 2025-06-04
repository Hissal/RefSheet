using LH.RefSheet;

namespace RefSheet.Tests;

[TestFixture]
public class ObservableValueSheetTests {
    [Test]
    public void OnWrite_Unkeyed_ObserverReceivesValue() {
        var sheet = new ObservableValueSheet();
        int received = 0;
        using (sheet.OnWrite<int>(v => received = v)) {
            sheet.Write(123);
            Assert.That(received, Is.EqualTo(123));
        }
    }

    [Test]
    public void OnRead_Unkeyed_ObserverReceivesValue() {
        var sheet = new ObservableValueSheet();
        sheet.Write(77);
        int received = 0;
        using (sheet.OnRead<int>(v => received = v)) {
            var value = sheet.Read<int>();
            Assert.That(received, Is.EqualTo(value));
        }
    }

    [Test]
    public void OnWrite_Keyed_ObserverReceivesKeyValue() {
        var sheet = new ObservableValueSheet();
        string receivedKey = null;
        double receivedValue = 0;
        using (sheet.OnWrite<string, double>((k, v) => {
                   receivedKey = k;
                   receivedValue = v;
               })) {
            sheet.Write("pi", 3.14);
            Assert.That(receivedKey, Is.EqualTo("pi"));
            Assert.That(receivedValue, Is.EqualTo(3.14));
        }
    }

    [Test]
    public void OnRead_Keyed_ObserverReceivesKeyValue() {
        var sheet = new ObservableValueSheet();
        sheet.Write("answer", 42);
        string? receivedKey = null;
        int receivedValue = 0;
        using (sheet.OnRead<string, int>((k, v) => {
                   receivedKey = k;
                   receivedValue = v;
               })) {
            var value = sheet.Read<string, int>("answer");
            Assert.Multiple(() => {
                Assert.That(receivedKey, Is.EqualTo("answer"));
                Assert.That(receivedValue, Is.EqualTo(value));
            });
        }
    }

    [Test]
    public void OnWrite_Keyed_SpecificKey_OnlyReceivesMatchingKey() {
        var sheet = new ObservableValueSheet();
        int received = 0;
        using (sheet.OnWrite("foo", (int v) => received = v)) {
            sheet.Write("bar", 1);
            Assert.That(received, Is.EqualTo(0));
            sheet.Write("foo", 99);
            Assert.That(received, Is.EqualTo(99));
        }
    }

    [Test]
    public void OnRead_Keyed_SpecificKey_OnlyReceivesMatchingKey() {
        var sheet = new ObservableValueSheet();
        sheet.Write("foo", 10);
        sheet.Write("bar", 20);
        int received = 0;
        using (sheet.OnRead("foo", (int v) => received = v)) {
            sheet.Read<string, int>("bar");
            Assert.That(received, Is.EqualTo(0));
            sheet.Read<string, int>("foo");
            Assert.That(received, Is.EqualTo(10));
        }
    }

    [Test]
    public void Unsubscriber_RemovesObserver() {
        var sheet = new ObservableValueSheet();
        int received = 0;
        var sub = sheet.OnWrite<int>(v => received = v);
        sheet.Write(1);
        Assert.That(received, Is.EqualTo(1));
        sub.Dispose();
        sheet.Write(2);
        Assert.That(received, Is.EqualTo(1)); // Should not update after dispose
    }
}