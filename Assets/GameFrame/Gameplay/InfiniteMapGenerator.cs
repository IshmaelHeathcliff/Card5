using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay
{
    [System.Serializable]
    public class TerrainType
    {
        public TileBase Tile;
        public int Weight;
    }

    public class InfiniteMapGenerator : MonoBehaviour
    {
        [Header("地形设置")]
        [SerializeField] List<TerrainType> _terrainTypes = new List<TerrainType>();

        [Header("地图设置")]
        [SerializeField] int _chunkSize = 20;          // 每个区块的大小
        [SerializeField] int _visibleChunks = 3;       // 可见区块数量

        [Header("渲染设置")]
        [SerializeField] string _sortingLayerName = "Default";  // 排序层级名称
        [SerializeField] SortingLayer _sortingLayer;             // 排序层级
        [SerializeField] int _orderInLayer;                     // 层级内排序
        [SerializeField] Material _tileMaterial;                // 瓦片材质

        [Header("生成设置")]
        [SerializeField] Transform _camera;            // 玩家Transform
        [SerializeField] float _updateInterval = 0.5f; // 更新间隔
        [SerializeField] Grid _grid;                   // Tilemap父Grid

        Dictionary<Vector2Int, GameObject> _activeChunks = new Dictionary<Vector2Int, GameObject>();
        Vector2Int _currentChunk;
        float _lastUpdateTime;

        void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main.transform;
            }
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<Grid>();
            }
            GenerateVisibleChunks();
        }

        void Update()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateChunks();
                _lastUpdateTime = Time.time;
            }
        }

        void UpdateChunks()
        {
            var playerChunk = new Vector2Int(
                Mathf.FloorToInt(_camera.position.x / _chunkSize),
                Mathf.FloorToInt(_camera.position.y / _chunkSize)
            );
            if (playerChunk != _currentChunk)
            {
                _currentChunk = playerChunk;
                GenerateVisibleChunks();
            }
        }

        void GenerateVisibleChunks()
        {
            int halfVisible = _visibleChunks / 2;
            var chunksToKeep = new List<Vector2Int>();
            for (int x = -halfVisible; x <= halfVisible; x++)
            {
                for (int y = -halfVisible; y <= halfVisible; y++)
                {
                    var chunkPos = new Vector2Int(_currentChunk.x + x, _currentChunk.y + y);
                    chunksToKeep.Add(chunkPos);
                    if (!_activeChunks.ContainsKey(chunkPos))
                    {
                        GenerateChunk(chunkPos);
                    }
                }
            }
            var chunksToRemove = new List<Vector2Int>();
            foreach (Vector2Int chunk in _activeChunks.Keys)
            {
                if (!chunksToKeep.Contains(chunk))
                {
                    Destroy(_activeChunks[chunk]);
                    chunksToRemove.Add(chunk);
                }
            }
            foreach (Vector2Int chunk in chunksToRemove)
            {
                _activeChunks.Remove(chunk);
            }
        }

        void GenerateChunk(Vector2Int chunkPos)
        {
            var chunkObj = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}");
            chunkObj.transform.parent = _grid != null ? _grid.transform : transform;

            // 添加并配置Tilemap
            var tilemap = chunkObj.AddComponent<Tilemap>();
            var tilemapRenderer = chunkObj.AddComponent<TilemapRenderer>();

            // 设置渲染属性
            tilemapRenderer.sortingLayerName = _sortingLayerName;
            tilemapRenderer.sortingOrder = _orderInLayer;
            if (_tileMaterial != null)
            {
                tilemapRenderer.material = _tileMaterial;
            }

            chunkObj.transform.position = new Vector3(chunkPos.x * _chunkSize, chunkPos.y * _chunkSize, 0);

            for (int x = 0; x < _chunkSize; x++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    TileBase tile = GetRandomTerrainTile();
                    if (tile != null)
                    {
                        tilemap.SetTile(tilePos, tile);
                    }
                }
            }
            _activeChunks.Add(chunkPos, chunkObj);
        }

        TileBase GetRandomTerrainTile()
        {
            if (_terrainTypes == null || _terrainTypes.Count == 0)
            {
                return null;
            }
            int totalWeight = 0;
            foreach (var type in _terrainTypes)
            {
                totalWeight += type.Weight;
            }
            int rand = Random.Range(0, totalWeight);
            int sum = 0;
            foreach (var type in _terrainTypes)
            {
                sum += type.Weight;
                if (rand < sum)
                {
                    return type.Tile;
                }
            }
            return _terrainTypes[0].Tile;
        }

        // void OnDrawGizmos()
        // {
        //     if (_camera != null)
        //     {
        //         var chunkCenter = new Vector3(
        //             Mathf.Floor(_camera.position.x / _chunkSize) * _chunkSize + _chunkSize / 2f,
        //             Mathf.Floor(_camera.position.y / _chunkSize) * _chunkSize + _chunkSize / 2f,
        //             0
        //         );
        //         Gizmos.color = Color.yellow;
        //         Gizmos.DrawWireCube(chunkCenter, new Vector3(_chunkSize, _chunkSize, 0));
        //     }
        // }

#if UNITY_EDITOR
        void OnValidate()
        {
            // 验证SortingLayer是否存在
            string[] sortingLayerNames = UnityEngine.SortingLayer.layers.Select(l => l.name).ToArray();
            if (!sortingLayerNames.Contains(_sortingLayerName))
            {
                Debug.LogWarning($"SortingLayer '{_sortingLayerName}' 不存在，将使用 'Default'");
                _sortingLayerName = "Default";
            }
        }
#endif
    }
}
