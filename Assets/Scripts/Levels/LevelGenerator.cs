using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("БЛОК 1: Холсты (Тайлмапы со сцены)")]
    [SerializeField] private Tilemap floorTilemap;   // Для травы (Grass)
    [SerializeField] private Tilemap wallTilemap;    // Основной слой для скал (Cliff 1)
    [SerializeField] private Tilemap waterTilemap;   // Слой для воды (Water)
    [SerializeField] private Tilemap[] extraTilemapsToClear; // Сюда закинь Cliff 2, Cliff 3, Path, чтобы они стирались

    [Header("БЛОК 2: Кисточки (Тайлы из окна Project)")]
    [SerializeField] private TileBase[] floorTiles;  // Разные виды травы/земли
    [SerializeField] private TileBase[] wallTiles;   // Твои 3 вида скалы
    [SerializeField] private TileBase[] waterTiles;  // Тайлы воды

    [Header("БЛОК 3: Списки префабов")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] breakablePrefabs;
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Обязательные префабы")]
    [SerializeField] private GameObject portalPrefab;

    [Header("Настройки размера карты")]
    [SerializeField] private int minWidth = 15;
    [SerializeField] private int maxWidth = 30;
    [SerializeField] private int minHeight = 15;
    [SerializeField] private int maxHeight = 30;
    [Range(0f, 100f)]
    [SerializeField] private float waterChance = 5f; // Шанс появления лужи воды вместо земли

    private List<Vector3Int> _floorPositions = new List<Vector3Int>();

    void Start()
    {
        GenerateNewLevel();
    }

    public void GenerateNewLevel()
    {
        // 1. Очищаем ВСЕ холсты
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();
        if (waterTilemap != null) waterTilemap.ClearAllTiles();

        foreach (Tilemap tm in extraTilemapsToClear)
        {
            if (tm != null) tm.ClearAllTiles();
        }

        _floorPositions.Clear();
        ClearOldEntities();

        // 2. Выбираем размер
        int currentWidth = Random.Range(minWidth, maxWidth);
        int currentHeight = Random.Range(minHeight, maxHeight);

        // 3. Рисуем карту
        for (int x = -2; x < currentWidth + 2; x++)
        {
            for (int y = -2; y < currentHeight + 2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // Если это граница карты (стены)
                if (x < 0 || x >= currentWidth || y < 0 || y >= currentHeight)
                {
                    if (wallTiles.Length > 0 && wallTilemap != null)
                    {
                        // Выбираем случайную скалу из твоих 3-х видов
                        TileBase randomWall = wallTiles[Random.Range(0, wallTiles.Length)];
                        wallTilemap.SetTile(tilePos, randomWall);
                    }
                }
                else
                {
                    // Внутри карты: делаем шанс на появление воды
                    if (Random.Range(0f, 100f) < waterChance && waterTiles.Length > 0 && waterTilemap != null)
                    {
                        TileBase randomWater = waterTiles[Random.Range(0, waterTiles.Length)];
                        waterTilemap.SetTile(tilePos, randomWater);
                        // Не добавляем воду в _floorPositions, чтобы игрок и враги не спавнились в воде
                    }
                    else if (floorTiles.Length > 0 && floorTilemap != null)
                    {
                        // Рисуем обычную землю
                        TileBase randomFloor = floorTiles[Random.Range(0, floorTiles.Length)];
                        floorTilemap.SetTile(tilePos, randomFloor);
                        _floorPositions.Add(tilePos);
                    }
                }
            }
        }

        if (_floorPositions.Count == 0) return; // Защита от ошибок

        ShufflePositions(_floorPositions);

        // 4. Ставим игрока
        if (Player.Instance != null)
        {
            Vector3 playerStartWorldPos = floorTilemap.CellToWorld(_floorPositions[0]) + new Vector3(0.5f, 0.5f, 0);
            Player.Instance.transform.position = playerStartWorldPos;
        }

        // 5. Ставим портал в конец
        Vector3 portalWorldPos = floorTilemap.CellToWorld(_floorPositions[_floorPositions.Count - 1]) + new Vector3(0.5f, 0.5f, 0);
        Instantiate(portalPrefab, portalWorldPos, Quaternion.identity);

        // 6. Спавним предметы и врагов
        for (int i = 1; i < _floorPositions.Count - 1; i++)
        {
            Vector3 spawnWorldPos = floorTilemap.CellToWorld(_floorPositions[i]) + new Vector3(0.5f, 0.5f, 0);
            float chance = Random.Range(0f, 100f);

            if (chance < 5f && enemyPrefabs.Length > 0)
            {
                Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnWorldPos, Quaternion.identity);
            }
            else if (chance < 12f && breakablePrefabs.Length > 0)
            {
                Instantiate(breakablePrefabs[Random.Range(0, breakablePrefabs.Length)], spawnWorldPos, Quaternion.identity);
            }
            else if (chance < 20f && obstaclePrefabs.Length > 0)
            {
                Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnWorldPos, Quaternion.identity);
            }
        }
    }

    private void ClearOldEntities()
    {
        GameObject[] oldEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in oldEnemies) Destroy(enemy);

        GameObject[] oldPortals = GameObject.FindGameObjectsWithTag("Portal");
        foreach (GameObject p in oldPortals) Destroy(p);
    }

    private void ShufflePositions(List<Vector3Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            Vector3Int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}