using UnityEngine;

namespace Card5
{
    public static class UILayerManager
    {
        const string LayerRootName = "UILayers";

        static readonly UILayer[] LayerOrder =
        {
            UILayer.Drag,
            UILayer.Popup,
            UILayer.System
        };

        public static RectTransform MoveToLayer(Transform target, UILayer layer, bool worldPositionStays = true)
        {
            if (target == null) return null;

            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null) return null;

            RectTransform layerRoot = GetLayer(canvas, layer);
            if (layerRoot == null) return null;

            target.SetParent(layerRoot, worldPositionStays);
            target.SetAsLastSibling();
            return layerRoot;
        }

        public static RectTransform GetLayer(Canvas canvas, UILayer layer)
        {
            if (canvas == null) return null;

            RectTransform layersRoot = GetOrCreateLayersRoot(canvas);
            if (layersRoot == null) return null;

            EnsureLayerOrder(layersRoot);
            return GetOrCreateLayer(layersRoot, layer);
        }

        static RectTransform GetOrCreateLayersRoot(Canvas canvas)
        {
            Transform existing = canvas.transform.Find(LayerRootName);
            if (existing != null)
            {
                var rect = existing as RectTransform;
                if (rect != null)
                {
                    StretchToParent(rect);
                    rect.SetAsLastSibling();
                    return rect;
                }
            }

            var rootObject = new GameObject(LayerRootName, typeof(RectTransform));
            rootObject.transform.SetParent(canvas.transform, false);
            var rootRect = (RectTransform)rootObject.transform;
            StretchToParent(rootRect);
            rootRect.SetAsLastSibling();
            return rootRect;
        }

        static void EnsureLayerOrder(RectTransform layersRoot)
        {
            for (int i = 0; i < LayerOrder.Length; i++)
            {
                RectTransform layer = GetOrCreateLayer(layersRoot, LayerOrder[i]);
                layer.SetSiblingIndex(i);
            }

            layersRoot.SetAsLastSibling();
        }

        static RectTransform GetOrCreateLayer(RectTransform layersRoot, UILayer layer)
        {
            string layerName = GetLayerName(layer);
            Transform existing = layersRoot.Find(layerName);
            if (existing != null)
            {
                var rect = existing as RectTransform;
                if (rect != null)
                {
                    StretchToParent(rect);
                    return rect;
                }
            }

            var layerObject = new GameObject(layerName, typeof(RectTransform));
            layerObject.transform.SetParent(layersRoot, false);
            var layerRect = (RectTransform)layerObject.transform;
            StretchToParent(layerRect);
            return layerRect;
        }

        static string GetLayerName(UILayer layer)
        {
            return layer switch
            {
                UILayer.Drag   => "DragLayer",
                UILayer.Popup  => "PopupLayer",
                UILayer.System => "SystemLayer",
                _              => $"{layer}Layer"
            };
        }

        static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition3D = Vector3.zero;
        }
    }
}
