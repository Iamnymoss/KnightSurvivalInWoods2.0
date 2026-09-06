using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private Tilemap[] extraTilemapsToClear;

    [Header("Tiles from the first level")]
    [SerializeField] private TileBase[] floorTiles;
    [SerializeField] private TileBase[] wallTiles;
    [SerializeField] private TileBase[] waterTiles;
    [SerializeField] private TileBase pathTile;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] breakablePrefabs;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject portalPrefab;

    [Header("Map size")]
    [SerializeField] private int minWidth = 24;
    [SerializeField] private int maxWidth = 36;
    [SerializeField] private int minHeight = 24;
    [SerializeField] private int maxHeight = 36;
    [Range(0f, 20f)]
    [SerializeField] private float waterChance = 5f;

    [Header("SampleScene-like population")]
    [Min(1f)][SerializeField] private float floorCellsPerEnemy = 900f;
    [Min(1f)][SerializeField] private float floorCellsPerBreakable = 275f;
    [Min(1f)][SerializeField] private float floorCellsPerObstacle = 90f;
    [Min(0)][SerializeField] private int safeRadius = 3;

    [Header("Увеличение сложности")]

    [Tooltip("Сколько новых врагов добавляется на каждом уровне")]
    [Min(1)]
    [SerializeField] private int additionalEnemiesPerLevel = 1;

    [Tooltip("Максимальное количество врагов. 0 = без ограничения")]
    [Min(0)]
    [SerializeField] private int maxEnemyCount = 30;

    private readonly List<Vector3Int> _floorPositions = new List<Vector3Int>();
    private readonly List<GameObject> _generatedEntities = new List<GameObject>();

    private void Start()
    {
        GenerateNewLevel();
    }

    public void GenerateNewLevel()
    {
        ClearLevel();

        int width = Random.Range(minWidth, maxWidth + 1);
        int height = Random.Range(minHeight, maxHeight + 1);
        HashSet<Vector3Int> waterPositions = CreateWaterClusters(width, height);

        PaintMap(width, height, waterPositions);
        if (_floorPositions.Count < 2)
        {
            Debug.LogError("LevelGenerator could not create enough walkable floor cells.");
            return;
        }

        Vector3Int playerCell = PickPlayerCell(width, height);
        Vector3Int portalCell = PickFarthestCell(playerCell);
        HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();
        List<Vector3Int> pathCells = PaintPath(playerCell, portalCell);
        foreach (Vector3Int pathCell in pathCells)
        {
            occupied.Add(pathCell);
        }

        ReserveArea(occupied, playerCell, safeRadius);
        ReserveArea(occupied, portalCell, safeRadius);

        if (Player.Instance != null)
        {
            Player.Instance.transform.position = CellCenter(playerCell);
        }

        GameObject generatedPortal =
            Spawn(portalPrefab, portalCell, occupied);

        if (generatedPortal != null &&
            generatedPortal.GetComponent<Portal>() == null)
        {
            generatedPortal.AddComponent<Portal>();
        }

        List<Vector3Int> candidates = new List<Vector3Int>(_floorPositions);
        ShufflePositions(candidates);

        int currentLevel = LevelManager.Instance != null
            ? Mathf.Max(1, LevelManager.Instance.currentLevel)
            : 1;

        int baseEnemyCount = Mathf.Max(
            1,
            Mathf.RoundToInt(
                _floorPositions.Count / floorCellsPerEnemy
            )
        );

        int enemiesAddedPerLevel =
            Mathf.Max(1, additionalEnemiesPerLevel);

        int enemyCount =
            baseEnemyCount +
            (currentLevel - 1) * enemiesAddedPerLevel;

        if (maxEnemyCount > 0)
        {
            enemyCount = Mathf.Min(enemyCount, maxEnemyCount);
        }

        Debug.Log(
            $"Генерация уровня {currentLevel}. " +
            $"Запланировано врагов: {enemyCount}"
        );

        int breakableCount = Mathf.RoundToInt(_floorPositions.Count / floorCellsPerBreakable);
        int obstacleCount = Mathf.RoundToInt(_floorPositions.Count / floorCellsPerObstacle);

        SpawnGroup(enemyPrefabs, enemyCount, candidates, occupied, 1);
        SpawnGroup(breakablePrefabs, breakableCount, candidates, occupied, 1);
        SpawnGroup(obstaclePrefabs, obstacleCount, candidates, occupied, 1);

        if (NavMeshRebakeHelper.Instance != null)
        {
            NavMeshRebakeHelper.Instance.RequestRebake();
        }
    }

    private void PaintMap(int width, int height, HashSet<Vector3Int> waterPositions)
    {
        for (int x = -2; x < width + 2; x++)
        {
            for (int y = -2; y < height + 2; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                bool isBorder = x < 0 || x >= width || y < 0 || y >= height;

                if (isBorder)
                {
                    SetRandomTile(wallTilemap, cell, wallTiles);
                }
                else if (waterPositions.Contains(cell))
                {
                    SetRandomTile(waterTilemap, cell, waterTiles);
                }
                else
                {
                    SetRandomTile(floorTilemap, cell, floorTiles);
                    _floorPositions.Add(cell);
                }
            }
        }
    }

    private HashSet<Vector3Int> CreateWaterClusters(int width, int height)
    {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();
        int targetCount = Mathf.RoundToInt(width * height * waterChance / 100f);
        int attempts = 0;

        while (result.Count < targetCount && attempts++ < targetCount * 8)
        {
            int clusterRadius = Random.Range(1, 3);
            int centerX = Random.Range(2, Mathf.Max(3, width - 2));
            int centerY = Random.Range(2, Mathf.Max(3, height - 2));

            for (int x = -clusterRadius; x <= clusterRadius; x++)
            {
                for (int y = -clusterRadius; y <= clusterRadius; y++)
                {
                    if (result.Count >= targetCount)
                    {
                        break;
                    }

                    if (x * x + y * y <= clusterRadius * clusterRadius + Random.Range(0, 2))
                    {
                        result.Add(new Vector3Int(centerX + x, centerY + y, 0));
                    }
                }
            }
        }

        return result;
    }

    private Vector3Int PickPlayerCell(int width, int height)
    {
        Vector3Int preferredCorner = Random.value < 0.5f
            ? new Vector3Int(2, 2, 0)
            : new Vector3Int(width - 3, height - 3, 0);

        return PickClosestCell(preferredCorner);
    }

    private Vector3Int PickClosestCell(Vector3Int target)
    {
        Vector3Int best = _floorPositions[0];
        int bestDistance = int.MaxValue;

        foreach (Vector3Int cell in _floorPositions)
        {
            int distance = Mathf.Abs(cell.x - target.x) + Mathf.Abs(cell.y - target.y);
            if (distance < bestDistance)
            {
                best = cell;
                bestDistance = distance;
            }
        }

        return best;
    }

    private Vector3Int PickFarthestCell(Vector3Int origin)
    {
        Vector3Int best = _floorPositions[0];
        int bestDistance = -1;

        foreach (Vector3Int cell in _floorPositions)
        {
            int distance = Mathf.Abs(cell.x - origin.x) + Mathf.Abs(cell.y - origin.y);
            if (distance > bestDistance)
            {
                best = cell;
                bestDistance = distance;
            }
        }

        return best;
    }

    private List<Vector3Int> PaintPath(Vector3Int start, Vector3Int destination)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        Vector3Int current = start;

        while (current != destination)
        {
            MakePathCell(current, result);

            bool canMoveX = current.x != destination.x;
            bool canMoveY = current.y != destination.y;
            bool moveX = canMoveX && (!canMoveY || Random.value < 0.5f);

            current += moveX
                ? new Vector3Int(destination.x > current.x ? 1 : -1, 0, 0)
                : new Vector3Int(0, destination.y > current.y ? 1 : -1, 0);
        }

        MakePathCell(destination, result);
        return result;
    }

    private void MakePathCell(Vector3Int cell, List<Vector3Int> pathCells)
    {
        if (waterTilemap != null && waterTilemap.HasTile(cell))
        {
            waterTilemap.SetTile(cell, null);
            SetRandomTile(floorTilemap, cell, floorTiles);
            if (!_floorPositions.Contains(cell))
            {
                _floorPositions.Add(cell);
            }
        }

        if (pathTilemap != null && pathTile != null)
        {
            pathTilemap.SetTile(cell, pathTile);
        }

        pathCells.Add(cell);
    }

    private void SpawnGroup(GameObject[] prefabs, int count, List<Vector3Int> candidates,
        HashSet<Vector3Int> occupied, int spacing)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        int spawned = 0;
        foreach (Vector3Int cell in candidates)
        {
            if (spawned >= count)
            {
                break;
            }

            if (IsAreaOccupied(occupied, cell, spacing))
            {
                continue;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Spawn(prefab, cell, occupied);
            ReserveArea(occupied, cell, spacing);
            spawned++;
        }
    }

    private GameObject Spawn(
        GameObject prefab,
        Vector3Int cell,
        HashSet<Vector3Int> occupied
    )
    {
        if (prefab == null)
        {
            Debug.LogError("LevelGenerator: не назначен префаб.");
            return null;
        }

        GameObject instance = Instantiate(
            prefab,
            CellCenter(cell),
            Quaternion.identity
        );

        _generatedEntities.Add(instance);
        occupied.Add(cell);

        return instance;
    }

    private Vector3 CellCenter(Vector3Int cell)
    {
        return floorTilemap.GetCellCenterWorld(cell);
    }

    private static void SetRandomTile(Tilemap tilemap, Vector3Int cell, TileBase[] tiles)
    {
        if (tilemap != null && tiles != null && tiles.Length > 0)
        {
            tilemap.SetTile(cell, tiles[Random.Range(0, tiles.Length)]);
        }
    }

    private static bool IsAreaOccupied(HashSet<Vector3Int> occupied, Vector3Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (occupied.Contains(center + new Vector3Int(x, y, 0)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ReserveArea(HashSet<Vector3Int> occupied, Vector3Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                occupied.Add(center + new Vector3Int(x, y, 0));
            }
        }
    }

    private void ClearLevel()
    {
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();
        if (waterTilemap != null) waterTilemap.ClearAllTiles();
        if (pathTilemap != null) pathTilemap.ClearAllTiles();

        if (extraTilemapsToClear != null)
        {
            foreach (Tilemap tilemap in extraTilemapsToClear)
            {
                if (tilemap != null) tilemap.ClearAllTiles();
            }
        }

        foreach (GameObject entity in _generatedEntities)
        {
            if (entity != null) Destroy(entity);
        }

        _generatedEntities.Clear();
        _floorPositions.Clear();

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(enemy);
        foreach (GameObject portal in GameObject.FindGameObjectsWithTag("Portal")) Destroy(portal);
    }

    private static void ShufflePositions(List<Vector3Int> positions)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3Int temporary = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temporary;
        }
    }
}