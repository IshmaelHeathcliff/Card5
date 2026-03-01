using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class GlobalCancellation : MonoBehaviour
    {
        // 全局取消令牌源
        static CancellationTokenSource s_globalCts;

        void Awake()
        {
            // 初始化全局令牌
            s_globalCts = new CancellationTokenSource();

            // 注册应用退出事件
            Application.quitting += OnApplicationQuitting;

            // 编辑器模式下处理PlayMode退出
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        // 获取组合令牌（推荐使用）
        public static CancellationTokenSource GetCombinedTokenSource(MonoBehaviour behaviour = null)
        {
            var tokens = new List<CancellationToken>
            {
                s_globalCts.Token
            };

            if (behaviour != null)
            {
                tokens.Add(behaviour.GetCancellationTokenOnDestroy());
            }

            return CancellationTokenSource.CreateLinkedTokenSource(tokens.ToArray());
        }

        void OnApplicationQuitting()
        {
            // 触发全局取消
            s_globalCts?.Cancel();
            s_globalCts?.Dispose();
            s_globalCts = null;
        }

#if UNITY_EDITOR
        void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                s_globalCts?.Cancel();
            }
        }
#endif
    }
}
