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
            ├─ MarkSystem.TickMarks()       // 印记持续时间 -1
            ├─ CardSystem.DiscardHand()     // 手牌全部弃掉
            ├─ BattleModel.ClearSlots()
            ├─ 回合数 +1，恢复能量
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
