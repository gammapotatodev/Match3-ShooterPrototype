using System;
using System.Collections;
using UnityEngine;

public class ShootSystem : MonoBehaviour
{
    private bool waitingForTarget;
    private BlockColor currentColor;
    private int scanX;

    public GameObject whiteOrbPrefab;
    public Transform projectileMuzzle; // откуда вылетают сферы

    public GridSystem gridSystem;
    public Action<bool> onShootComplete;
    public Action<int> onBulletUsed;

    public int bulletsLeft; // храним между выстрелами

    public void Shoot(int bullets, BlockColor projectileColor)
    {
        bulletsLeft = bullets;
        currentColor = projectileColor;
        waitingForTarget = true;
        scanX = 0;

        StartCoroutine(ShootingLoop());
    }

    private IEnumerator ShootingLoop()
    {
        while (waitingForTarget && bulletsLeft > 0)
        {
            if (gridSystem.IsBusy)
            {
                yield return null;
                continue;
            }

            if (scanX >= gridSystem.width)
            {
                scanX = 0;
                yield return null;
                continue;
            }

            var stack = gridSystem.gridCells[scanX][0]; // ← BlockStack

            if (stack != null && !stack.IsEmpty)
            {
                GameObject topBlock = stack.TopMost;
                if (topBlock != null)
                {
                    var properties = topBlock.GetComponent<BlockProperties>();
                    if (properties != null && properties.colorType == currentColor)
                    {
                        yield return StartCoroutine(RemoveSingle(topBlock));
                    }
                }
            }

            scanX++;
            yield return null;
        }

        waitingForTarget = false;
        onShootComplete?.Invoke(true);
    }

    private IEnumerator RemoveSingle(GameObject targetBlock)
    {
        bool removedBase = false;
        if (targetBlock == null)
        {
            gridSystem.Unlock();
            yield break;
        }

        gridSystem.Lock();

        // Создаём сферу и отправляем к цели
        GameObject orbObj = Instantiate(
            whiteOrbPrefab,
            projectileMuzzle.position,
            Quaternion.identity
        );

        WhiteOrb orb = orbObj.GetComponent<WhiteOrb>();

        bool arrived = false;
        orb.Init(targetBlock.transform, () => arrived = true);

        // Ждём, пока сфера долетит
        yield return new WaitUntil(() => arrived);

        // Находим и удаляем блок из стека
        bool blockRemoved = false;

        for (int x = 0; x < gridSystem.width; x++)
        {
            for (int y = 0; y < gridSystem.height; y++)
            {
                var stack = gridSystem.gridCells[x][y];
                if (stack == null || stack.IsEmpty) continue;

                // Если это верхний блок
                if (stack.topBlock == targetBlock)
                {
                    if (stack.topBlock != null)
                    {
                        Destroy(stack.topBlock);
                        stack.topBlock = null;
                    }
                    blockRemoved = true;
                    break;
                }
                // Если это базовый блок (и нет верхнего)
                else if (stack.baseBlock == targetBlock && !stack.HasTop)
                {
                    if (stack.baseBlock != null)
                    {
                        Destroy(stack.baseBlock);
                        stack.baseBlock = null;
                    }
                    blockRemoved = true;
                    break;
                }
            }
            if (blockRemoved) break;
        }

        // Обновляем UI и счётчик
        bulletsLeft--;
        onBulletUsed?.Invoke(bulletsLeft);

        // Сдвигаем все столбцы и запускаем анимацию
        if (blockRemoved)
        {
            // Сдвигаем все столбцы (можно оптимизировать, если знаешь только один столбец)
            for (int x = 0; x < gridSystem.width; x++)
            {
                gridSystem.ShiftColumnInstant(x);
            }

            yield return StartCoroutine(gridSystem.AnimateAllCubes(0.05f));
        }

        gridSystem.Unlock();
    }
    private void RemoveFromGrid(GameObject target)
    {
        gridSystem.TryRemoveBlock(target);
    }
}
