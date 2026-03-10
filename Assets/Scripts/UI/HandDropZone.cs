using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 挂到手牌区域（如 HandContainer 或其子物体）上，作为拖放目标。
    /// 从槽位拖出的卡牌放到此区域时会被放回手牌。
    /// 确保该 GameObject 上有 Graphic（如 Image，可设为透明）且 Raycast Target 勾选，否则无法被检测到。
    /// </summary>
    public class HandDropZone : MonoBehaviour
    {
    }
}
