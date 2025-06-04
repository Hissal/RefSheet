using NUnit.Framework;
using RefSheet;

namespace RefSheetTests;

[TestFixture]
public class RefSheetTests {
    class Dummy {
        public int Value { get; set; }
    }

    [Test]
    public void SetAndGet_UnkeyedReference_Works() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy { Value = 42 };
        sheet.Set(dummy);
        Assert.That(sheet.Get<Dummy>(), Is.EqualTo(dummy));
    }

    [Test]
    public void SetAndGet_KeyedReference_Works() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy { Value = 99 };
        sheet.Set("foo", dummy);
        Assert.That(sheet.Get<string, Dummy>("foo"), Is.EqualTo(dummy));
    }

    [Test]
    public void HasRef_ReturnsCorrectly() {
        var sheet = new RefSheet.RefSheet();
        Assert.That(sheet.HasRef<Dummy>(), Is.False);
        sheet.Set(new Dummy());
        Assert.That(sheet.HasRef<Dummy>(), Is.True);
    }

    [Test]
    public void HasRef_Keyed_ReturnsCorrectly() {
        var sheet = new RefSheet.RefSheet();
        Assert.That(sheet.HasRef<string, Dummy>("bar"), Is.False);
        sheet.Set("bar", new Dummy());
        Assert.That(sheet.HasRef<string, Dummy>("bar"), Is.True);
    }

    [Test]
    public void Get_UnsetReference_Throws() {
        var sheet = new RefSheet.RefSheet();
        Assert.Throws<KeyNotFoundException>(() => sheet.Get<Dummy>());
    }

    [Test]
    public void Get_KeyedUnsetReference_Throws() {
        var sheet = new RefSheet.RefSheet();
        Assert.Throws<NullReferenceException>(() => sheet.Get<string, Dummy>("baz"));
    }

    [Test]
    public void Set_NullReference_RemovesReference() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy();
        sheet.Set(dummy);
        Assert.That(sheet.HasRef<Dummy>(), Is.True);
        sheet.Set<Dummy>(null);
        Assert.That(sheet.HasRef<Dummy>(), Is.False);
    }

    [Test]
    public void Set_NullKeyedReference_RemovesReference() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy();
        sheet.Set("x", dummy);
        Assert.That(sheet.HasRef<string, Dummy>("x"), Is.True);
        sheet.Set<string, Dummy>("x", null);
        Assert.That(sheet.HasRef<string, Dummy>("x"), Is.False);
    }

    [Test]
    public void Extensions_TryGet_Works() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy();
        sheet.Set(dummy);
        Assert.Multiple(() => {
            Assert.That(sheet.TryGet<Dummy>(out var result), Is.True);
            Assert.That(result, Is.EqualTo(dummy));
        });
    }

    [Test]
    public void Extensions_TryGet_Keyed_Works() {
        var sheet = new RefSheet.RefSheet();
        var dummy = new Dummy();
        sheet.Set("y", dummy);
        Assert.Multiple(() => {
            Assert.That(sheet.TryGet("y", out Dummy result), Is.True);
            Assert.That(result, Is.EqualTo(dummy));
        });
    }

    [Test]
    public void Extensions_GetRefOrNull_Works() {
        var sheet = new RefSheet.RefSheet();
        Assert.That(sheet.GetRefOrNull<Dummy>(), Is.Null);
        var dummy = new Dummy();
        sheet.Set(dummy);
        Assert.That(sheet.GetRefOrNull<Dummy>(), Is.EqualTo(dummy));
    }

    [Test]
    public void Extensions_GetRefOrNull_Keyed_Works() {
        var sheet = new RefSheet.RefSheet();
        Assert.That(sheet.GetRefOrNull<string, Dummy>("z"), Is.Null);
        var dummy = new Dummy();
        sheet.Set("z", dummy);
        Assert.That(sheet.GetRefOrNull<string, Dummy>("z"), Is.EqualTo(dummy));
    }
}