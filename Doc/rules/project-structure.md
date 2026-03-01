---
description: 项目文件结构指南
globs:
alwaysApply: false
---
# 项目结构指南

## 目录结构
### 核心目录 `Assets/Scripts/`
- `Core/` - 核心系统和基础功能
    - `Scene/` - 场景加载管理
- `Data/` - 数据与资源系统相关
    - `Config/` - Config 配置类，辅助JSON配置文件的读取
    - `SavaLoad/` - 加载与保存工具
    - `Translate/` - 翻译工具
- `Editor/` - Unity 编辑器扩展
- `Gameplay/` - 游戏游玩机制相关
    - `Character/` - 角色相关系统
        - `Damage/` - 伤害系统
        - `Enemy/` - 敌人角色相关实现
        - `Modifier/` - 词条系统
        - `Player/` - 玩家角色相关实现
        - `Stat/` - 属性与数据系统
        - `State/` - 状态与Buff系统
    - `Item/` - 资源与道具管理
    - `Skill/` - 技能系统
- `UI/` - 用户界面相关
- `Utilities/` - 辅助工具类

### 数据目录 `Assets/Data/`
- `Preset/` - 存放配置文件，预定义游戏数据
- `Saves/` - 存放存档文件

### 主要文件
- [GameManager.cs](mdc:Assets/Scripts/GameManager.cs) - 游戏管理器
- [QFramework.cs](mdc:Assets/Scripts/Core/QFramework.cs) - QFramework 框架
- [Survivor.cs](mdc:Assets/Scripts/Survivor.cs) - QFramework 框架入口

### 角色系统
- [CharacterController.cs](mdc:Assets/Scripts/Character/CharacterController.cs) - 角色控制器基类
- [MoveController.cs](mdc:Assets/Scripts/Character/MoveController.cs) - 移动控制器基类
- [AttackerController.cs](mdc:Assets/Scripts/Character/Damage/AttackerController.cs) - 攻击器控制器基类
- [Attacker.cs](mdc:Assets/Scripts/Character/Damage/Attacker.cs) - 攻击器基类
- [Damageable.cs](mdc:Assets/Scripts/Character/Damage/Damageable.cs) - 受伤器基类
- [CharacterModel.cs](mdc:Assets/Scripts/GamePlay/Character/CharacterModel.cs) - 角色模型

### Model
- [CharacterModel.cs](mdc:Assets/Scripts/GamePlay/Character/CharacterModel.cs) - 角色模型
    - [PlayerModel.cs](mdc:Assets/Scripts/GamePlay/Character/Player/PlayerModel.cs) - 玩家模型
    - [EnemyModel.cs](mdc:Assets/Scripts/GamePlay/Character/Enemy/EnemyModel.cs) - 敌人模型
- [SceneModel.cs](mdc:Assets/Scripts/Core/Scene/SceneModel.cs) - 场景模型

### System
- [InputSystem.cs](mdc:Assets/Scripts/Core/InputSystem.cs) - 玩家输入控制系统
- [ModifierSystem.cs](mdc:Assets/Scripts/GamePlay/Character/Modifier/ModifierSystem.cs) - 词条系统
- [StateCreateSystem.cs](mdc:Assets/Scripts/GamePlay/Character/State/StateCreateSystem.cs) - 状态创建系统
- [DropSystem.cs](mdc:Assets/Scripts/GamePlay/Item/DropSystem.cs) - 掉落物控制系统
- [SkillSystem.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillSystem.cs) - 技能系统
- [SkillGachaSystem.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillGachaSystem.cs) - 技能抽取系统
- [SkillReleaseSystem.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillRelease/SkillReleaseSystem.cs) - 技能释放（吞噬）系统
- [ResourceSystem.cs](mdc:Assets/Scripts/GamePlay/Item/ResourceSystem.cs) - 资源管理系统
- [CountSystem.cs](mdc:Assets/Scripts/GamePlay/CountSystem.cs) - 计数系统
- [AttackerCreateSystem.cs](mdc:Assets/Scripts/GamePlay/Character/Damage/AttackerCreateSystem.cs) - 攻击器创建系统

### Utility
- [SaveLoadUtility.cs](mdc:Assets/Scripts/Data/SaveLoad/SaveLoadUtility.cs) - 加载保存工具
