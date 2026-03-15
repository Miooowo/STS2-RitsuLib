# Card Dynamic Var Toolkit / 卡牌动态变量规则

## Scope / 适用范围

This document defines the rules for dynamic variable creation, tooltip binding, and tooltip propagation provided by RitsuLib.

本文定义 RitsuLib 提供的动态变量创建、tooltip 绑定与 tooltip 传播规则。

## Provided APIs / 提供的接口

The relevant implementation files are:

相关实现文件如下：

- [Cards/DynamicVars/ModCardVars.cs](../Cards/DynamicVars/ModCardVars.cs)
- [Cards/DynamicVars/DynamicVarExtensions.cs](../Cards/DynamicVars/DynamicVarExtensions.cs)
- [Cards/DynamicVars/DynamicVarTooltipRegistry.cs](../Cards/DynamicVars/DynamicVarTooltipRegistry.cs)
- [Cards/Patches/DynamicVarTooltipPatches.cs](../Cards/Patches/DynamicVarTooltipPatches.cs)

## Variable construction rule / 变量构造规则

RitsuLib provides the following convenience constructors through `ModCardVars`:

RitsuLib 通过 `ModCardVars` 提供以下便捷构造器：

- `ModCardVars.Int(string name, decimal amount)`
- `ModCardVars.String(string name, string value = "")`

RitsuLib does not assign gameplay semantics to these variables.

RitsuLib 不为这些变量赋予任何玩法语义。

The meaning of a dynamic variable is defined entirely by the content author.

动态变量的具体含义完全由内容作者定义。

## Tooltip binding rule / Tooltip 绑定规则

Any `DynamicVar` may be associated with a tooltip factory through `DynamicVarExtensions`.

任意 `DynamicVar` 都可以通过 `DynamicVarExtensions` 绑定 tooltip 工厂。

Supported binding forms are:

支持的绑定形式包括：

- `WithTooltip(Func<DynamicVar, IHoverTip> tooltipFactory)`
- `WithTooltip(string titleTable, string titleKey, string? descriptionTable = null, string? descriptionKey = null, string? iconPath = null)`
- `WithSharedTooltip(string entryPrefix, string? iconPath = null)`

`WithSharedTooltip(entryPrefix)` resolves to the following key pair under `static_hover_tips`:

`WithSharedTooltip(entryPrefix)` 会在 `static_hover_tips` 下解析以下 key：

- `<entryPrefix>.title`
- `<entryPrefix>.description`

If `WithTooltip(...)` is called without `descriptionTable` or `descriptionKey`, the description lookup defaults to the title table and the `.description` form derived from the title key.

如果调用 `WithTooltip(...)` 时未提供 `descriptionTable` 或 `descriptionKey`，则描述查找默认使用标题 table 与由标题 key 派生出的 `.description` 形式。

## Tooltip materialization rule / Tooltip 实例化规则

`DynamicVarExtensions.CreateHoverTip()` asks `DynamicVarTooltipRegistry` for the tooltip factory currently associated with that variable and materializes a tooltip from it.

`DynamicVarExtensions.CreateHoverTip()` 会向 `DynamicVarTooltipRegistry` 查询当前变量关联的 tooltip 工厂，并据此实例化 tooltip。

If no tooltip factory is registered, no tooltip is produced.

如果未注册 tooltip 工厂，则不会生成 tooltip。

## Card hover tip rule / 卡牌悬浮提示规则

The patch defined in [Cards/Patches/DynamicVarTooltipPatches.cs](../Cards/Patches/DynamicVarTooltipPatches.cs) appends all registered dynamic-variable tooltips from `CardModel.DynamicVars` to the card hover-tip sequence.

[Cards/Patches/DynamicVarTooltipPatches.cs](../Cards/Patches/DynamicVarTooltipPatches.cs) 中定义的补丁会把 `CardModel.DynamicVars` 中所有已注册的动态变量 tooltip 追加到卡牌 hover-tip 序列中。

## Clone rule / 克隆规则

Tooltip metadata registered for a `DynamicVar` is copied to its clone when `DynamicVar.Clone()` is invoked.

当调用 `DynamicVar.Clone()` 时，已注册在原 `DynamicVar` 上的 tooltip 元数据会复制到克隆对象上。

## Localization rule / 本地化规则

RitsuLib does not provide built-in localization entries for dynamic variables.

RitsuLib 不为动态变量提供内置本地化词条。

If `WithSharedTooltip(...)` is used, the content author must provide the required localization entries.

如果使用 `WithSharedTooltip(...)`，则所需本地化词条必须由内容作者自行提供。

## Non-scope / 非职责范围

RitsuLib does not provide built-in gameplay behavior, built-in variable semantics, or built-in tooltip vocabulary for dynamic variables.

RitsuLib 不为动态变量提供内置玩法行为、内置变量语义或内置 tooltip 词汇。
