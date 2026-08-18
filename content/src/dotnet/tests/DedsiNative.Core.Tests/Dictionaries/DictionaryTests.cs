using DedsiNative.Dictionaries;
using Xunit;
using DictionaryAggregate = DedsiNative.Dictionaries.Dictionary;

namespace DedsiNative.Core.Tests.Dictionaries;

/// <summary>
/// 字典聚合领域规则测试。
/// </summary>
public sealed class DictionaryTests
{
    /// <summary>
    /// 创建字典时应保存系统归属和分组名称。
    /// </summary>
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        var dictionaryId = Ulid.NewUlid().ToString();
        var systemId = Ulid.NewUlid().ToString();

        var dictionary = new DictionaryAggregate(
            dictionaryId,
            systemId,
            "统一身份认证",
            "账户状态");

        Assert.Equal(dictionaryId, dictionary.Id);
        Assert.Equal(systemId, dictionary.SystemId);
        Assert.Equal("统一身份认证", dictionary.SystemName);
        Assert.Equal("账户状态", dictionary.Name);
        Assert.Empty(dictionary.Items);
    }

    /// <summary>
    /// 字典和系统标识必须是合法的 26 位 ULID。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Invalid_Identifiers()
    {
        var validId = Ulid.NewUlid().ToString();

        Assert.Throws<ArgumentException>(() => new DictionaryAggregate(
            "invalid",
            validId,
            "系统",
            "字典"));
        Assert.Throws<ArgumentException>(() => new DictionaryAggregate(
            validId,
            "invalid",
            "系统",
            "字典"));
    }

    /// <summary>
    /// 同一字典分组内不允许重复编码。
    /// </summary>
    [Fact]
    public void AddItem_Should_Reject_Duplicate_Code()
    {
        var dictionary = CreateDictionary();
        dictionary.AddItem(
            Ulid.NewUlid().ToString(),
            "normal",
            "正常",
            null,
            0,
            true,
            false,
            null);

        Assert.Throws<ArgumentException>(() => dictionary.AddItem(
            Ulid.NewUlid().ToString(),
            "normal",
            "正常副本",
            null,
            1,
            true,
            false,
            null));
    }

    /// <summary>
    /// 设置新的默认项时应清除旧默认项。
    /// </summary>
    [Fact]
    public void AddItem_Should_Keep_At_Most_One_Default()
    {
        var dictionary = CreateDictionary();
        dictionary.AddItem(
            Ulid.NewUlid().ToString(), "first", "第一项", null, 0, true, true, null);
        dictionary.AddItem(
            Ulid.NewUlid().ToString(), "second", "第二项", null, 1, true, true, null);

        Assert.Single(dictionary.Items, item => item.IsDefault);
        Assert.Equal("second", dictionary.Items.Single(item => item.IsDefault).Code);
    }

    /// <summary>
    /// 停用默认项时应自动清除默认标记。
    /// </summary>
    [Fact]
    public void ChangeItem_Should_Clear_Default_When_Disabled()
    {
        var dictionary = CreateDictionary();
        var itemId = Ulid.NewUlid().ToString();
        dictionary.AddItem(itemId, "normal", "正常", null, 0, true, true, null);

        dictionary.ChangeItem(itemId, "normal", "正常", null, 0, false, true, null);

        Assert.False(dictionary.Items.Single().IsEnabled);
        Assert.False(dictionary.Items.Single().IsDefault);
    }

    /// <summary>
    /// 修改父项时应拒绝形成层级环。
    /// </summary>
    [Fact]
    public void ChangeItem_Should_Reject_Parent_Cycle()
    {
        var dictionary = CreateDictionary();
        var parentId = Ulid.NewUlid().ToString();
        var childId = Ulid.NewUlid().ToString();
        dictionary.AddItem(parentId, "parent", "父项", null, 0, true, false, null);
        dictionary.AddItem(childId, "child", "子项", null, 0, true, false, parentId);

        Assert.Throws<ArgumentException>(() => dictionary.ChangeItem(
            parentId,
            "parent",
            "父项",
            null,
            0,
            true,
            false,
            childId));
    }

    private static DictionaryAggregate CreateDictionary()
    {
        return new DictionaryAggregate(
            Ulid.NewUlid().ToString(),
            Ulid.NewUlid().ToString(),
            "系统",
            "字典");
    }
}
