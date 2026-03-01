---
description: 项目系统架构指南
globs:
alwaysApply: false
---
# 架构指南

## 基础工具
- 使用 [QFramework.cs](mdc:Assets/Scripts/Core/QFramework.cs) 作为基础架构
    - 将模块分为 `Model` 、 `System` 、 `Controller` 、 `Utility`
    - 大多数 `Monobehaviour` 都应该实现 `IController` 来接入QFramework
    - 在 Controller 中使用 Command 对 `Model` 进行操作
    - 在 Controller 中使用 Query 对 `Model` 信息进行查询
    - 在 Controller 中注册 Event 来实现事件回调
    - 在 System、Model、Command中完成操作时发送 Event 通知 Controller
- 使用 `UniTask` 处理异步操作
    - 尽量使用异步操作代替协程、 `Update` 等
    - 使用 `Forget()` 处理不需要等待的异步操作
    - 及时合理的位置释放 `AsyncOperationHandle`
    - 使用 [GlobalCancellation.cs](mdc:Assets/Scripts/Core/GlobalCancellation.cs) 注入 CancellationToken
- 使用Unity的 `Addressables` 进行资源管理
    - 使用 `AddressablesManager.Release` 释放异步资源加载产生的handle
    - 使用 `Addressables.InstantiateAsync` 加载 Prefab
    - 使用 `Addressables.ReleaseInstance` 卸载实例
- 使用 Unity 的新输入系统 InputSystem
    - 通过 `PlayerInput` 类处理输入事件
    - 在 `OnEnable` 中注册输入事件，在 `OnDisable` 中注销
- 在开发中使用 `Odin Inspector` 构建方便使用的Editor UI

## 数据配置定义、加载与实例化
- 在 JSON 文件中保存游戏数据
- 通过 Config 类获取数据
- 通过 Loader 类将 JSON 文件反序列化到 Conifg 类
- 将反序列化的数据信息保存在 System 或 Model 中
- 在 System 中可根据数据信息创建相关实例
- 在 Controller 中发送 Command 调用 System 创建实例
    - 简单情形下也可直接调用 System
