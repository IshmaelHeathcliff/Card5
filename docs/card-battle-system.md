---
description: 卡牌对战系统架构说明，包含文件位置、扩展方式和关键类
globs: Assets/Scripts/**/*.cs
alwaysApply: false
---

# 卡牌对战系统架构

## 关键文件位置

| 类型 | 路径 |
|------|------|
| Architecture 入口 | `Assets/Scripts/Core/GameArchitecture.cs` |
| 游戏启动入口 | `Assets/Scripts/GameManager.cs` |
| 战斗 Model | `Assets/Scripts/Gameplay/Battle/BattleModel.cs` |
| 牌组 Model | `Assets/Scripts/Gameplay/Battle/DeckModel.cs` |
| 印记 Model | `Assets/Scripts/Gameplay/Marks/MarkModel.cs` |
| 战斗 System | `Assets/Scripts/Gameplay/Battle/BattleSystem.cs` |
| 卡牌 System | `Assets/Scripts/Gameplay/Battle/CardSystem.cs` |
| 印记 System | `Assets/Scripts/Gameplay/Marks/MarkSystem.cs` |
| 所有事件 | `Assets/Scripts/Gameplay/Events/BattleEvents.cs` |
| 效果基类 | `Assets/Scripts/Data/CardEffectSO.cs` |
| 具体效果 | `Assets/Scripts/Data/Effects/` |

---

## 战斗核心流程

```
GameManager.StartBattle()
  └─ StartBattleCommand
       └─ BattleSystem.StartBattle()
            ├─ DeckModel.InitDeck()       // 加载 DeckPresetData，建完整牌组
            ├─ BattleModel.InitBattle()   // 重置 HP、能量、回合
            ├─ CardSystem.Shuffle()       // Fisher-Yates 洗牌
            ├─ CardSystem.DrawCards(8)    // 首回合抽 8 张
            └─ 发送 BattleStartedEvent / TurnStartedEvent
```

### 出牌流程

```
HandViewController  监听 HandRefreshedEvent
  └─ 实例化（或从对象池取出）CardViewController

玩家拖拽卡牌到 CardSlotView
  └─ PlayCardCommand(cardData, slotIndex)
       └─ BattleSystem.TryPlayCard()
            ├─ 验证：槽位为空 & 能量足够
            ├─ 更新 BattleModel.PlaySlots[slotIndex]
            ├─ 扣除能量
            └─ 发送 CardPlayedEvent / EnergyChangedEvent

玩家点击「结束回合」
  └─ EndTurnCommand
       └─ BattleSystem.EndTurn()
            ├─ BattleSystem.ResolveSlots()  // 按槽位顺序结算
            │    ├─ MarkSystem.ExecuteSlotMarks(BeforeCardEffects)
            │    ├─ CardEffectSO.Execute(BattleContext)  // 卡牌效果
            │    └─ MarkSystem.ExecuteSlotMarks(AfterCardEffects)
            ├─ BattleRewardSystem.TryOfferTurnReward() // 若本回合结算过卡牌，生成奖励并暂停后续流程
            ├─ SelectBattleRewardCommand // 玩家完成所有奖励组选择后继续
            ├─ CardSystem.DiscardHand()     // 手牌全部弃掉
            ├─ EnemyTurn()
            ├─ 回合数 +1，恢复能量
            ├─ MarkSystem.TickMarks()       // 新回合开始时推进印记持续时间
            ├─ CardSystem.DrawCards(8)      // 新回合抽 8 张
            └─ 发送 TurnEndedEvent / TurnStartedEvent
```

### 撤牌 / 换牌

```
从槽位拖回手牌区
  └─ ReturnCardToHandCommand
       └─ BattleSystem.TryReturnCardFromSlot()

从槽位拖到另一槽位
  └─ SwapSlotsCommand
       └─ BattleSystem.TrySwapSlots()
```

### 重抽流程

```
玩家点击「重抽」
  └─ RedrawCardsCommand(selectedCards)
       └─ BattleSystem.TryRedrawCards()
            ├─ 验证 RedrawsRemaining > 0
            ├─ CardSystem.RedrawCards()    // 选中手牌→弃牌堆，重新抽同等数量
            ├─ RedrawsRemaining -1
            └─ 发送 RedrawCountChangedEvent
```

### 战斗奖励流程

```
BattleSystem.EndTurn()
  └─ ResolveSlots()
       └─ 若至少结算过 1 张卡牌，调用 BattleRewardSystem.TryOfferTurnReward()
            ├─ 根据 BattleRewardConfigData 生成本次奖励组
            ├─ 当前卡牌奖励组优先从 CardLibraryData 牌库中按解锁条件筛选
            ├─ 从已解锁卡牌中按权重无放回随机抽 3 张
            ├─ 若奖励组未配置牌库，则兼容旧的配置卡池随机
            └─ 发送 BattleRewardOfferedEvent，战斗流程暂停

玩家选择奖励选项
  └─ SelectBattleRewardCommand(offerId, optionId)
       └─ BattleRewardSystem.ClaimReward()
            ├─ 卡牌奖励：CardSystem.AddCardToDeck()，加入弃牌堆并同步 FullDeck
            ├─ 若仍有奖励组，继续发送剩余待选项
            └─ 全部奖励组完成后发送 BattleRewardCompletedEvent，并继续敌方回合/下一回合
```

`BattleRewardConfigData` 支持一次奖励配置多个奖励组；每个奖励组都是多选一。当前已实现 `Card` 类型，默认用于卡牌三选一；`Mark` 类型仅保留枚举扩展位，后续再接入印记奖励逻辑。

`CardLibraryData` 是卡牌奖励牌库。牌库条目包含：

- `Card`：可作为奖励的卡牌配置。
- `Weight`：随机权重，数值越高越容易出现。
- `UnlockConditions`：解锁条件列表，全部满足时该卡牌才进入本次奖励候选。

当前支持的解锁条件：

| 条件 | 说明 |
|------|------|
| `Always` | 始终解锁 |
| `MinTurnNumber` | 当前回合数大于等于指定值 |
| `MaxTurnNumber` | 当前回合数小于等于指定值 |
| `MinDeckCardCount` | 玩家完整牌组数量大于等于指定值 |
| `HasCardInDeck` | 玩家完整牌组中拥有指定卡 |
| `DoesNotHaveCardInDeck` | 玩家完整牌组中没有指定卡 |
| `PlayerHpPercentAtMost` | 玩家当前 HP 百分比小于等于指定值 |

默认配置与 UI：

- `Assets/Data/Preset/CardLibrary/DefaultCardLibrary.asset`：默认卡牌奖励牌库，包含基础、进阶、超级伤害卡及回合数解锁条件。
- `Assets/Data/Preset/Reward/BattleRewardConfig.asset`：默认战斗奖励配置，包含 1 个引用默认牌库的卡牌三选一奖励组。
- `Assets/Prefabs/BattleRewardOption.prefab`：单个奖励选项视图。
- `Assets/Prefabs/BattleRewardPopup.prefab`：奖励选择弹窗，已挂到 `Assets/Scenes/Main.unity` 的 `View/BattleRewardPopup`。
- `Assets/Scripts/Editor/BattleRewardSetupUtility.cs`：可通过 Unity 菜单 `Card5/Setup Battle Reward UI` 重新生成默认配置与 UI。

---

## 新增卡牌效果

继承 `CardEffectSO`，加 `[CreateAssetMenu]`，实现 `Execute(BattleContext ctx)`：

```csharp
[CreateAssetMenu(fileName = "XxxEffect", menuName = "Card5/Effects/Xxx")]
public class XxxEffectSO : CardEffectSO
{
    public override void Execute(BattleContext context)
    {
        // context.BattleModel          — 玩家战斗状态（HP、能量、槽位）
        // context.DeckModel            — 牌组状态（抽牌堆、弃牌堆）
        // context.Enemy                — 敌人（TakeDamage、Heal）
        // context.BattleSystem         — 调用战斗系统方法
        // context.MarkSystem           — 施加印记
        // context.LeftNeighbor         — 左邻槽卡牌（可为 null）
        // context.RightNeighbor        — 右邻槽卡牌（可为 null）
        // context.SlotIndex            — 当前槽位索引（0-4）
        // context.CurrentCard          — 当前卡牌数据
        // context.DealDamage(amount)   — 便捷伤害方法（含事件）
        // context.ApplyHeal(amount)    — 便捷治疗方法（含事件）
    }
}
```

---

## 印记系统

### 两种印记目标

| 类型 | 说明 | 施加方法 |
|------|------|---------|
| 槽位印记 | 绑定到槽位编号，与槽无关于卡牌 | `MarkSystem.ApplyMarkToSlot(slotIndex, markData)` |
| 卡牌印记 | 绑定到具体 `CardData` 实例 | `MarkSystem.ApplyMarkToCard(cardData, markData)` |

### 触发时机（`MarkTrigger` 枚举）

| 值 | 时机 |
|----|------|
| `BeforeCardEffects` | 卡牌效果执行前 |
| `AfterCardEffects` | 卡牌效果执行后 |

### 持续时间

- `Duration > 0`：剩余回合数，每次 `TickMarks()` 时 -1，归零后自动移除
- `Duration == -1`：永久印记，不会自动移除

---

## 敌人扩展

实现 `IEnemyBehavior`，注入到 `EnemyController`：

```csharp
public class MyEnemyBehavior : IEnemyBehavior
{
    void OnTurnStart() { ... }
    void OnTurnEnd()   { ... }
    void OnTakeDamage() { ... }
}

// 在某处注入：
enemyController.SetBehavior(new MyEnemyBehavior());
```

---

## 命令一览

| 命令 | 返回值 | 说明 |
|------|--------|------|
| `StartBattleCommand` | void | 初始化并启动战斗 |
| `DrawCardCommand` | void | 抽一张牌 |
| `PlayCardCommand` | bool | 将手牌放入指定槽位 |
| `ReturnCardToHandCommand` | bool | 从槽位撤回手牌 |
| `SwapSlotsCommand` | bool | 交换/移动两个槽位的卡牌 |
| `EndTurnCommand` | void | 结束回合并触发结算 |
| `RedrawCardsCommand` | bool | 重抽选中的手牌 |
| `AddCardToDeckCommand` | void | 新增卡牌到牌库（写入弃牌堆，同步 FullDeck） |
| `RemoveCardFromDeckCommand` | bool | 从牌库移除一张卡牌（FullDeck + DrawPile/DiscardPile） |
| `SelectBattleRewardCommand` | bool | 领取指定奖励组选项；全部奖励领取完成后恢复战斗流程 |

---

## 事件一览

| 事件 | 触发时机 |
|------|---------|
| `BattleStartedEvent` | 战斗初始化完成 |
| `BattleEndedEvent` | 战斗结束（含 IsVictory） |
| `TurnStartedEvent` | 新回合开始 |
| `TurnEndedEvent` | 回合结算完成 |
| `CardDrawnEvent` | 单张牌被抽取 |
| `HandRefreshedEvent` | 手牌整体刷新（HandViewController 监听此事件重建 View） |
| `CardAddedToHandEvent` | 单张牌加入手牌 |
| `CardRemovedFromHandEvent` | 单张牌离开手牌 |
| `CardPlayedEvent` | 卡牌放入槽位 |
| `CardRemovedFromSlotEvent` | 卡牌从槽位移除 |
| `SlotsSwappedEvent` | 两个槽位内容交换 |
| `SlotEffectsResolvedEvent` | 本回合所有槽位效果结算完毕 |
| `DamageDealtEvent` | 伤害事件（含来源、目标、数值） |
| `HealAppliedEvent` | 治疗事件 |
| `PlayerHpChangedEvent` | 玩家 HP 变化 |
| `EnemyHpChangedEvent` | 敌人 HP 变化 |
| `PlayerDiedEvent` | 玩家死亡 |
| `EnemyDiedEvent` | 敌人死亡 |
| `EnergyChangedEvent` | 能量变化 |
| `RedrawCountChangedEvent` | 剩余重抽次数变化 |
| `DrawPileChangedEvent` | 抽牌堆数量变化 |
| `DiscardPileChangedEvent` | 弃牌堆数量变化 |
| `MarkAppliedEvent` | 印记施加 |
| `MarkRemovedEvent` | 印记移除 |
| `CardAddedToDeckEvent` | 新卡牌加入牌库（弃牌堆 + FullDeck） |
| `CardRemovedFromDeckEvent` | 卡牌从牌库移除 |
| `BattleRewardOfferedEvent` | 战斗奖励生成，等待玩家选择 |
| `BattleRewardOptionClaimedEvent` | 某个奖励组选项被领取 |
| `BattleRewardCompletedEvent` | 本次所有奖励组领取完成 |
