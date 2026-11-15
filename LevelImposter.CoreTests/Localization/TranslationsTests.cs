using System;
using LevelImposter.Localization;
using LevelImposter.CoreTests.Infrastructure;

namespace LevelImposter.CoreTests.Localization;

public sealed class TranslationsTests : IDisposable
{
    private readonly Func<string, string?>? _previousLoader;

    public TranslationsTests()
    {
        _previousLoader = TranslationResources.TextLoader;
        TranslationResources.TextLoader = TestPaths.LoadAssetText;
    }

    public void Dispose()
    {
        TranslationResources.TextLoader = _previousLoader;
    }

    [Theory]
    [InlineData(LILanguage.English)]
    [InlineData(LILanguage.Latam)]
    [InlineData(LILanguage.Spanish)]
    [InlineData(LILanguage.Japanese)]
    public void SetLanguage_ShouldLoadPackagedTranslations(LILanguage language)
    {
        Translation.SetLanguage(language);

        var value = Translation.Get("main_menu.maps");

        value.ShouldNotBeNullOrWhiteSpace();
        value.ShouldNotBe("main_menu.maps");
    }

    [Fact]
    public void Get_ShouldFormatArguments()
    {
        Translation.SetLanguage(LILanguage.English);

        Translation.Get("shop.map.author", "Digi").ShouldBe("by Digi");
    }

    [Fact]
    public void Get_ShouldReturnKey_WhenTranslationIsMissing()
    {
        Translation.SetLanguage(LILanguage.English);

        Translation.Get("test.missing.key").ShouldBe("test.missing.key");
    }

    [Fact]
    public void Get_ShouldReturnRawText_WhenFormatArgumentsDoNotMatch()
    {
        Translation.SetLanguage(LILanguage.English);

        // main_menu.update_failed expects two arguments; a mismatch must not throw.
        var value = Translation.Get("main_menu.update_failed", "only-one");

        value.ShouldNotBeNullOrWhiteSpace();
        value.ShouldNotBe("main_menu.update_failed");
    }

    [Fact]
    public void LoadAssetText_ShouldReadPackagedResources()
    {
        TestPaths.LoadAssetText("i18n.en-us.json").ShouldNotBeNullOrEmpty();
        TestPaths.LoadAssetText("i18n.missing.json").ShouldBeNull();
    }
}
