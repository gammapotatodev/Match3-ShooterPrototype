using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Level Configuration")]
    [SerializeField] private LevelPattern levelPattern;
    public List<List<BlockStack>> gridCells = new List<List<BlockStack>>();
    public bool IsBusy { get; private set; }

    public void Lock()
    {
        IsBusy = true;
    }

    public void Unlock()
    {
        IsBusy = false;
    }


    public event Action OnAllColumnsShiftedComplete;
    public Action OnGridChanged;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        gridCells.Clear();

        width = levelPattern.width;
        height = levelPattern.height;

        for (int x = 0; x < width; x++)
        {
            gridCells.Add(new List<BlockStack>());

            for (int y = 0; y < height; y++)
            {
                var stack = new BlockStack();
                Vector3 basePos = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);

                // Базовый слой (нижний) — красный
                GameObject basePrefab = levelPattern.GetPrefabFromLayer(0, x, y);
                if (basePrefab != null)
                {
                    stack.baseBlock = Instantiate(basePrefab, basePos, Quaternion.identity, transform);
                    var props = stack.baseBlock.GetComponent<BlockProperties>();
                    props.colorType = levelPattern.GetColorFromLayer(0, x, y);
                    props.layerType = BlockLayer.Base;
                }

                // Верхний слой (жёлтый) — всегда создаём на том же родителе
                GameObject topPrefab = levelPattern.GetPrefabFromLayer(1, x, y);
                if (topPrefab != null)
                {
                    // Сдвигаем по Z ближе к камере (меньше Z = ближе)
                    // Чем больше Z-координата — тем дальше объект от камеры
                    Vector3 topPos = basePos + Vector3.forward * 1f;  // ПОЗИТИВНЫЙ сдвиг

                    stack.topBlock = Instantiate(topPrefab, topPos, Quaternion.identity, transform);
                    var props = stack.topBlock.GetComponent<BlockProperties>();
                    props.colorType = levelPattern.GetColorFromLayer(1, x, y);
                    props.layerType = BlockLayer.Top;
                }

                gridCells[x].Add(stack);
            }
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Удаляет верхний блок в клетке (x,y)
    // Возвращает true, если что-то удалили
    public bool RemoveBlockAt(int x, int y)
    {
        if (!IsValidPosition(x, y)) return false;

        var stack = gridCells[x][y];
        bool removed = stack.RemoveTop();

        if (removed)
        {
            OnGridChanged?.Invoke();
        }

        return removed;
    }

    public void ShiftColumnInstant(int column)
    {
        var col = gridCells[column];

        int writeY = 0;

        for (int readY = 0; readY < height; readY++)
        {
            var stack = col[readY];
            if (!stack.IsEmpty)
            {
                // Перемещаем весь стек вниз
                if (readY != writeY)
                {
                    col[writeY] = stack;
                    col[readY] = new BlockStack(); // очищаем старую позицию
                }
                writeY++;
            }
        }

        // Очищаем верхние пустые ячейки
        for (int y = writeY; y < height; y++)
        {
            col[y] = new BlockStack();
        }

        OnGridChanged?.Invoke();
    }

    public IEnumerator AnimateAllCubes(float duration = 0.15f)
    {
        var startPositions = new Dictionary<GameObject, Vector3>();
        var targetPositions = new Dictionary<GameObject, Vector3>();

        // Собираем все существующие блоки и их целевые позиции
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                if (stack.HasBase)
                {
                    Vector3 target = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0);
                    startPositions[stack.baseBlock] = stack.baseBlock.transform.position;
                    targetPositions[stack.baseBlock] = target;
                }
                if (stack.HasTop)
                {
                    Vector3 target = gridOrigin + new Vector3(x * cellSize, y * cellSize, 0) + Vector3.forward * -0.1f;
                    startPositions[stack.topBlock] = stack.topBlock.transform.position;
                    targetPositions[stack.topBlock] = target;
                }
            }
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = Mathf.SmoothStep(0, 1, t);

            foreach (var block in startPositions.Keys)
            {
                if (block == null) continue;
                block.transform.position = Vector3.Lerp(startPositions[block], targetPositions[block], ease);
            }
            yield return null;
        }

        foreach (var block in startPositions.Keys)
        {
            if (block == null) continue;
            block.transform.position = targetPositions[block];
        }

        OnAllColumnsShiftedComplete?.Invoke();
    }

    public int GetRemainingBlocksCount()
    {
        int count = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                count += (stack.HasBase ? 1 : 0) + (stack.HasTop ? 1 : 0);
            }
        return count;
    }

    // Внутри класса GridSystem
    public bool TryRemoveBlock(GameObject block)
    {
        if (block == null) return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var stack = gridCells[x][y];
                if (stack == null || stack.IsEmpty) continue;

                if (stack.topBlock == block)
                {
                    Destroy(stack.topBlock);
                    stack.topBlock = null;
                    OnGridChanged?.Invoke();
                    return true;
                }
                else if (stack.baseBlock == block)
                {
                    // Удаляем верхний, если он есть (опционально)
                    if (stack.topBlock != null)
                    {
                        Destroy(stack.topBlock);
                        stack.topBlock = null;
                    }
                    Destroy(stack.baseBlock);
                    stack.baseBlock = null;
                    OnGridChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

}

