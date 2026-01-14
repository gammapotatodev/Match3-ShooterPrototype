using UnityEngine;

public class ShootingSlotSystem : MonoBehaviour
{
    public bool[] isFull;
    public GameObject[] slots;

    private void Awake()
    {
        isFull = new bool[slots.Length];
        projectilesInSlots = new PickUpProjectilesSystem[slots.Length];
    }

        // НОВОЕ: что лежит в каждом слоте
    public PickUpProjectilesSystem[] projectilesInSlots;

    public void SetProjectile(int index, PickUpProjectilesSystem projectile)
    {
        projectilesInSlots[index] = projectile;
        isFull[index] = true;
    }

    public void ClearSlot(int index)
    {
        projectilesInSlots[index] = null;
        isFull[index] = false;
    }

    public void CheckTripleMatch()
    {
        for (int i = 0; i <= projectilesInSlots.Length - 3; i++)
        {
            var p1 = projectilesInSlots[i];
            var p2 = projectilesInSlots[i + 1];
            var p3 = projectilesInSlots[i + 2];

            if (p1 == null || p2 == null || p3 == null)
                continue;

            if (p1.ProjectileColor == p2.ProjectileColor &&
                p2.ProjectileColor == p3.ProjectileColor)
            {
                MergeProjectiles(i, i + 1, i + 2);
                return; // если нужно — можно убрать, чтобы чекать дальше
            }
        }
    }

    private void MergeProjectiles(int a, int b, int c)
    {
        var p1 = projectilesInSlots[a];
        var p2 = projectilesInSlots[b];
        var p3 = projectilesInSlots[c];

        int totalShots = p1.ShootCount + p2.ShootCount + p3.ShootCount;

        // уничтожаем старые
        Destroy(p2.gameObject);
        Destroy(p3.gameObject);
        ClearSlot(b);
        ClearSlot(c);

        // 🔄 сбрасываем ShootSystem
        ShootSystem shootSystem = p1.GetComponent<ShootSystem>();
        shootSystem.StopAllCoroutines();

        shootSystem.bulletsLeft = 0;

        // ✅ обновляем данные
        p1.shootCount = totalShots;
        p1.countText.text = totalShots.ToString();

        Debug.Log($"Merged projectile ready: {totalShots} bullets");
    }

    ////////////////////////////////
    public bool AreAllSlotsFull()
    {
        for (int i = 0; i < isFull.Length; i++)
        {
            if (!isFull[i])
                return false;
        }
        return true;
    }
}
