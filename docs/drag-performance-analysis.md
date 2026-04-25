---
description: 卡牌拖拽卡顿问题分析与优化要点
globs: Assets/Scripts/UI/*.cs
alwaysApply: false
---

# 拖拽卡顿问题分析

## 已识别的性能问题

### 1. 每帧 GetComponent（高影响）

| 位置 | 问题 | 修复 |
|------|------|------|
| `CardViewController.OnDrag` | 每帧调用 `_rootCanvas.GetComponent<RectTransform>()` | 在 Awake 中缓存 `RectTransform _canvasRect` |
| `CardSlotView.UpdateDragPreviewPosition` | 每帧调用 `_dragPreview.GetComponent<RectTransform>()` | 创建预览时缓存 `RectTransform _dragPreviewRect` |

GetComponent 会遍历 GameObject 及其子节点，拖拽时每帧 60 次调用易造成卡顿。

### 2. 世界坐标与 Canvas 重建（中影响）

- `CardViewController.OnDrag` 使用 `transform.position = worldPos`（世界坐标）。若 Canvas 为 Screen Space - Camera，会触发坐标变换与可能的标记脏区域。
- 建议：用 Canvas 的 RectTransform 做 `ScreenPointToLocalPointInRectangle` 得到 localPosition，再赋给卡牌的 `rectTransform.localPosition`，减少不必要变换并利于合批。

### 3. 松手时分配与 GetComponent（低影响）

- `FindSlotUnderPointer` / `OnEndDrag` 中 `new List<RaycastResult>()` 与循环内 `GetComponent<CardViewController>()` / `GetComponent<CardSlotView>()` 仅在松手时执行一次，影响较小；若需进一步优化可复用 List 或缓存组件引用。

### 4. 布局与层级变化（中影响）

- 手牌开始拖拽时会从 `HandContainer` 临时移动到 `UILayerManager` 创建的 `DragLayer`，HandContainer 的 LayoutGroup 会重新布局剩余手牌，可能触发一次 RebuildLayout。
- 槽位在 BeginDrag 时调用 `RefreshUI()`，若槽位在 LayoutGroup 下可能触发布局。
- 建议：手牌容器使用 CanvasGroup 或占位符避免布局抖动；槽位区域尽量独立于敏感布局。不要再给单张手牌添加独立 Canvas 来处理置顶，拖拽置顶统一交给 `DragLayer`。

### 5. Canvas 与 Graphic 数量（环境相关）

- 大量手牌 + 多个 Graphic（Image、TextMeshPro）会增大 Canvas 的 Rebuild 成本。
- 当前手牌共用根 Canvas，单张 `HandCard` 只保留 `CanvasGroup` 控制拖拽时射线阻挡，不再包含独立 Canvas 和 GraphicRaycaster。
- 可考虑：继续减少嵌套、或对非可见区域做裁剪/禁用。

## 优化优先级

1. **必做**：去掉拖拽过程中每帧的 GetComponent，改为缓存引用。
2. **推荐**：手牌 OnDrag 使用缓存的 Canvas RectTransform + `ScreenPointToLocalPointInRectangle` 写 localPosition。
3. **可选**：松手时复用 RaycastResult 列表、减少 GetComponent 次数；检查手牌/槽位所在 Canvas 与布局结构。

## 本次补充

- 手牌容器已移除 `HorizontalLayoutGroup`，改为脚本计算布局，避免布局系统在拖拽和动画过程中频繁重建。
- 手牌拖拽过程会实时重排其余卡牌的位置预览，重点优化“重叠手牌 + 插入预览”的可读性。
- 右键自动出牌和手牌与槽位交换，统一通过临时卡牌视图播放飞行动画，减少视觉瞬移。
- 拖拽避让只在预览插入位变化时才重排，且旋转补间统一使用归一化角度，避免卡牌绕圈。
