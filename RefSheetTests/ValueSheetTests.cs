using RefSheet;

namespace RefSheetTests;

public class ValueSheetTests {
    [Test]
    public void WriteAndRead_UnkeyedValue_Works() {
        var sheet = new ValueSheet();
        sheet.Write<int>(42);
        Assert.That(sheet.Read<int>(), Is.EqualTo(42));
    }

    [Test]
    public void HasValue_UnkeyedValue_ReturnsTrueAfterWrite() {
        var sheet = new ValueSheet();
        Assert.That(sheet.HasValue<int>(), Is.False);
        sheet.Write<int>(123);
        Assert.That(sheet.HasValue<int>(), Is.True);
    }

    [Test]
    public void WriteAndRead_KeyedValue_Works() {
        var sheet = new ValueSheet();
        sheet.Write<string, double>("pi", 3.14);
        Assert.That(sheet.Read<string, double>("pi"), Is.EqualTo(3.14));
    }

    [Test]
    public void HasValue_KeyedValue_ReturnsTrueAfterWrite() {
        var sheet = new ValueSheet();
        Assert.That(sheet.HasValue<string, double>("e"), Is.False);
        sheet.Write<string, double>("e", 2.71);
        Assert.That(sheet.HasValue<string, double>("e"), Is.True);
    }

    [Test]
    public void Read_UnwrittenValue_ReturnsDefault() {
        var sheet = new ValueSheet();
        Assert.Multiple(() => {
            Assert.That(sheet.Read<int>(), Is.EqualTo(0));
            Assert.That(sheet.Read<string, double>("missing"), Is.EqualTo(0.0));
        });
    }

    [Test]
    public void TryRead_UnkeyedValue_Works() {
        var sheet = new ValueSheet();
        sheet.Write<int>(99);
        Assert.Multiple(() => {
            Assert.That(sheet.TryRead<int>(out var value), Is.True);
            Assert.That(value, Is.EqualTo(99));
        });
    }

    [Test]
    public void TryRead_KeyedValue_Works() {
        var sheet = new ValueSheet();
        sheet.Write<string, int>("answer", 42);
        Assert.Multiple(() => {
            Assert.That(sheet.TryRead("answer", out int value), Is.True);
            Assert.That(value, Is.EqualTo(42));
        });
    }

    [Test]
    public void TryRead_UnwrittenValue_ReturnsFalse() {
        var sheet = new ValueSheet();
        Assert.Multiple(() => {
            Assert.That(sheet.TryRead<int>(out var _), Is.False);
            Assert.That(sheet.TryRead("nope", out int _), Is.False);
        });
    }

    [Test]
    public void GetValueOrDefault_UnwrittenValue_ReturnsDefault() {
        var sheet = new ValueSheet();
        Assert.Multiple(() => {
            Assert.That(sheet.GetValueOrDefault<int>(), Is.EqualTo(0));
            Assert.That(sheet.GetValueOrDefault<string, double>("missing"), Is.EqualTo(0.0));
        });
    }

    [Test]
    public void GetValueOrDefault_WrittenValue_ReturnsWrittenValue() {
        var sheet = new ValueSheet();
        sheet.Write<int>(100);
        sheet.Write<string, double>("golden", 42.0);

        Assert.Multiple(() => {
            Assert.That(sheet.GetValueOrDefault<int>(), Is.EqualTo(100));
            Assert.That(sheet.GetValueOrDefault<string, double>("golden"), Is.EqualTo(42.0));
        });
    }

    [Test]
    public void TryReadNonDefault_UnkeyedValue_ReturnsTrueForNonDefaultValue() {
        var sheet = new ValueSheet();
        sheet.Write<int>(42);
        Assert.Multiple(() => {
            Assert.That(sheet.TryReadNonDefault(out int value), Is.True);
            Assert.That(value, Is.EqualTo(42));
        });
    }

    [Test]
    public void TryReadNonDefault_UnkeyedValue_ReturnsFalseForDefaultValue() {
        var sheet = new ValueSheet();
        sheet.Write<int>(0);
        Assert.Multiple(() => {
            Assert.That(sheet.TryReadNonDefault(out int value), Is.False);
            Assert.That(value, Is.EqualTo(0));
        });
    }

    [Test]
    public void TryReadNonDefault_KeyedValue_ReturnsTrueForNonDefaultValue() {
        var sheet = new ValueSheet();
        sheet.Write<string, int>("key", 42);
        Assert.Multiple(() => {
            Assert.That(sheet.TryReadNonDefault("key", out int value), Is.True);
            Assert.That(value, Is.EqualTo(42));
        });
    }

    [Test]
    public void TryReadNonDefault_KeyedValue_ReturnsFalseForDefaultValue() {
        var sheet = new ValueSheet();
        sheet.Write<string, int>("key", 0);
        Assert.Multiple(() => {
            Assert.That(sheet.TryReadNonDefault("key", out int value), Is.False);
            Assert.That(value, Is.EqualTo(0));
        });
    }

    [Test]
    public void TryReadNonDefault_KeyedValue_ReturnsFalseForMissingKey() {
        var sheet = new ValueSheet();
        Assert.Multiple(() => {
            Assert.That(sheet.TryReadNonDefault("missing", out int value), Is.False);
            Assert.That(value, Is.EqualTo(0));
        });
    }
}