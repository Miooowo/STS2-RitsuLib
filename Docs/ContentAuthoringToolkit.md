# Content Authoring Toolkit / 内容创作规则

## Scope / 适用范围

This document defines the authoring rules for content registration, key naming, and asset override behavior provided by RitsuLib.

本文定义 RitsuLib 提供的内容注册、键命名与资源覆写行为规则。

This document is normative. If code behavior differs from informal examples elsewhere, the runtime implementation prevails.

本文为规范性说明。若其他非正式示例与实际代码行为不一致，以运行时实现为准。

## Registration APIs / 注册接口

RitsuLib exposes the following registration entry points:

RitsuLib 提供以下注册入口：

- `RitsuLibFramework.CreateContentPack(modId)`
- `RitsuLibFramework.GetContentRegistry(modId)`
- `RitsuLibFramework.GetKeywordRegistry(modId)`
- `RitsuLibFramework.GetTimelineRegistry(modId)`
- `RitsuLibFramework.GetUnlockRegistry(modId)`

`CreateContentPack(modId)` is a convenience wrapper. It does not define a different runtime rule set.

`CreateContentPack(modId)` 只是便捷封装，不定义另一套独立的运行时规则。

## Supported registration categories / 支持的注册类别

The content pack builder defined in [Scaffolding/Content/ModContentPackBuilder.cs](../Scaffolding/Content/ModContentPackBuilder.cs) supports the following registration categories:

[Scaffolding/Content/ModContentPackBuilder.cs](../Scaffolding/Content/ModContentPackBuilder.cs) 中定义的内容包构建器支持以下注册类别：

- character
- card
- relic
- potion
- power
- orb
- shared event
- act event
- shared ancient
- act ancient
- keyword
- story
- epoch
- unlock rule

The builder executes registered steps in insertion order when `Apply()` is called.

构建器在调用 `Apply()` 时按添加顺序执行各个注册步骤。

## Registration timing / 注册时机

All content registration must be completed before the framework freezes content registration.

所有内容注册必须在框架冻结内容注册之前完成。

After registration is frozen, additional content registration is invalid and may throw.

内容注册冻结后，继续注册内容属于无效操作，并且可能抛出异常。

## Model id rule / 模型 id 规则

For models registered through the RitsuLib content registry, `ModelId.Entry` uses the following fixed format:

对于通过 RitsuLib 内容注册器注册的模型，`ModelId.Entry` 使用以下固定格式：

- `<modid>_<category>_<typename>`

Each segment is normalized to an uppercase underscore-separated identifier.

每个字段都会被规范化为全大写、以下划线分隔的标识符。

For mod id `STS2-WineFox`, examples are:

以 mod id `STS2-WineFox` 为例：

- `WineFoxStrike` card -> `STS2_WINE_FOX_CARD_WINE_FOX_STRIKE`
- `HandCrank` relic -> `STS2_WINE_FOX_RELIC_HAND_CRANK`
- `WineFox` character -> `STS2_WINE_FOX_CHARACTER_WINE_FOX`

If two registered models under the same mod id and the same model category share the same CLR type name, they resolve to the same entry and must be renamed.

如果同一 mod id、同一模型类别下有两个已注册模型共享相同 CLR 类型名，则它们会解析为同一 entry，必须通过重命名解决。

## Localization rule / 本地化规则

Game localization keys are written directly against the fixed `ModelId.Entry` defined above.

游戏本地化 key 直接基于上文定义的固定 `ModelId.Entry` 编写。

Examples:

示例：

- `STS2_WINE_FOX_CARD_WINE_FOX_STRIKE.title`
- `STS2_WINE_FOX_CARD_WINE_FOX_STRIKE.description`
- `STS2_WINE_FOX_RELIC_HAND_CRANK.title`
- `STS2_WINE_FOX_CHARACTER_WINE_FOX.title`

`RitsuLibFramework.CreateModLocalization(...)` is separate from the game's `LocString` model-key pipeline.

`RitsuLibFramework.CreateModLocalization(...)` 与游戏的 `LocString` 模型 key 管线相互独立。

## Asset override rule / 资源覆写规则

RitsuLib provides template-based asset override support through the interfaces and patches defined under [Scaffolding/Content/Patches/ContentAssetOverridePatches.cs](../Scaffolding/Content/Patches/ContentAssetOverridePatches.cs).

RitsuLib 通过 [Scaffolding/Content/Patches/ContentAssetOverridePatches.cs](../Scaffolding/Content/Patches/ContentAssetOverridePatches.cs) 中定义的接口与补丁提供基于模板的资源覆写支持。

Supported asset override categories are:

支持的资源覆写类别包括：

- card portrait / beta portrait / frame / portrait border / energy icon / frame material
- relic icon / icon outline / big icon
- power icon / big icon
- orb icon / visuals scene
- potion image / outline

An override is applied only when all of the following conditions are satisfied:

只有在满足以下全部条件时，资源覆写才会生效：

1. the model implements the matching override interface, directly or through a matching `Mod*Template`
2. the override member returns a non-empty path
3. the referenced resource exists when existence is required by the patch
1. 模型直接实现了对应的 override 接口，或通过匹配的 `Mod*Template` 间接实现
2. override 成员返回非空路径
3. 在对应补丁要求存在性校验时，被引用资源实际存在

## Compatibility rule / 兼容规则

RitsuLib applies its fixed-entry rule only to model types explicitly registered through the RitsuLib content registry.

RitsuLib 的固定 entry 规则只作用于那些通过 RitsuLib 内容注册器显式注册的模型类型。

The fixed-entry rule is applied at `ModelDb.GetEntry(Type)`.

固定 entry 规则的处理点为 `ModelDb.GetEntry(Type)`。

## Related documents / 相关文档

- [CharacterAndUnlockScaffolding.md](CharacterAndUnlockScaffolding.md)
- [CardDynamicVarToolkit.md](CardDynamicVarToolkit.md)
