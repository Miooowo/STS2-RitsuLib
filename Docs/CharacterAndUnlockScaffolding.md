# Character and Unlock Scaffolding / 角色与解锁规则

## Scope / 适用范围

This document defines the registration and dependency rules for character-related content, stories, epochs, and unlock conditions.

本文定义角色相关内容、story、epoch 与解锁条件的注册及依赖规则。

## Character-related content structure / 角色相关内容结构

Character content commonly depends on the following model groups:

角色内容通常依赖以下模型组：

- card pool
- relic pool
- potion pool
- character model
- story model
- epoch model
- unlock rule

For template-based authoring, the following base types are relevant:

在基于模板的创作方式下，以下基类具有直接关联：

- `TypeListCardPoolModel`
- `TypeListRelicPoolModel`
- `TypeListPotionPoolModel`
- `ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>`
- `ModStoryTemplate`
- `CharacterUnlockEpochTemplate<TCharacter>`
- `CardUnlockEpochTemplate`
- `RelicUnlockEpochTemplate`
- `PotionUnlockEpochTemplate`

## Registration rule / 注册规则

Character-related content is registered through the content, timeline, and unlock registries.

角色相关内容通过 content、timeline 与 unlock 注册器完成注册。

The minimum registration set for a character release is determined by the content actually referenced by that character.

一个角色发行所需的最小注册集合，取决于该角色实际引用的内容。

If a character references a card pool, relic pool, or potion pool, those pools and their referenced models must be registered before runtime lookup occurs.

如果角色引用了 card pool、relic pool 或 potion pool，则这些 pool 以及它们引用的模型都必须在运行时查找发生前完成注册。

## Unlock rule / 解锁规则

Unlock conditions are defined separately from content registration.

解锁条件与内容注册分开定义。

RitsuLib provides unlock registration helpers for the following conditions:

RitsuLib 为以下条件提供了解锁注册辅助：

- require epoch for a model
- unlock epoch after run as a character
- unlock epoch after win as a character
- unlock epoch after ascension win as a character
- unlock epoch after run count

Unlock rules do not replace content registration. A model must still be registered even if it is gated by an unlock condition.

解锁规则不替代内容注册。即使一个模型受解锁条件限制，它本身仍然必须先被注册。

## Registration timing / 注册时机

All character content, stories, epochs, and unlock rules must be registered before content registration is frozen.

所有角色内容、story、epoch 与解锁规则都必须在内容注册冻结前完成注册。

## Model id and localization rule / 模型 id 与本地化规则

Character-related models registered through RitsuLib follow the same fixed `ModelId.Entry` rule described in [ContentAuthoringToolkit.md](ContentAuthoringToolkit.md).

通过 RitsuLib 注册的角色相关模型，统一遵循 [ContentAuthoringToolkit.md](ContentAuthoringToolkit.md) 中定义的固定 `ModelId.Entry` 规则。

Character localization keys, unlock text keys, and default asset path derivations must be written against that fixed entry.

角色本地化 key、unlock text key 与默认资源路径推导，都必须围绕该固定 entry 编写。

Example:

示例：

- character type `WineFox` in mod `STS2-WineFox` -> `STS2_WINE_FOX_CHARACTER_WINE_FOX`

## Dependency rule / 依赖规则

If a character model resolves other models by type through `ModelDb.GetId(type)` and `ModelDb.GetById<TModel>(...)`, all referenced types must remain stable and registered.

如果角色模型通过 `ModelDb.GetId(type)` 与 `ModelDb.GetById<TModel>(...)` 按类型解析其他模型，则所有被引用类型都必须保持稳定并且已完成注册。

Renaming a referenced CLR type changes its derived fixed entry.

重命名被引用的 CLR 类型会改变其派生出的固定 entry。

## Related document / 相关文档

- [ContentAuthoringToolkit.md](ContentAuthoringToolkit.md)
