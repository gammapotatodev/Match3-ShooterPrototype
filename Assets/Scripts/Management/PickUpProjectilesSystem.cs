using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickUpProjectilesSystem : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] public int shootCount = 5;
    [SerializeField] private BlockColor projectileColor;

    private ShootSystem shootSystem;
    private ShootingSlotSystem shootingSlotSystem;
    private int index;

    public TextMeshProUGUI countText;
    
    public BlockColor ProjectileColor => projectileColor;
    public int ShootCount => shootCount;
    
    private void Start()
    {
        countText.text = shootCount.ToString();
        shootingSlotSystem = GameObject.Find("ShootingSlots").GetComponent<ShootingSlotSystem>();
        shootSystem = GetComponent<ShootSystem>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        for (int i = 0; i < shootingSlotSystem.slots.Length; i++)
        {
            if (!shootingSlotSystem.isFull[i])
            {
                index = i;

                shootingSlotSystem.SetProjectile(index, this);

                transform.position = shootingSlotSystem.slots[i].transform.position;

                shootSystem.onBulletUsed += UpdateCountText;
                shootSystem.onShootComplete += OnShootFinished;

                shootSystem.Shoot(shootCount, projectileColor);

                // Проверяем комбинации
                shootingSlotSystem.CheckTripleMatch();
                        // ПРОВЕРКА НА ПРОИГРЫШ
                if (shootingSlotSystem.AreAllSlotsFull())
                {
                    GameManager gm = Object.FindAnyObjectByType<GameManager>();
                    if (gm != null)
                        gm.OnLose();
                }


                break;
            }
        }
    }

    private void OnShootFinished(bool success)
    {
        shootSystem.onShootComplete -= OnShootFinished;
            shootSystem.onBulletUsed -= UpdateCountText;
        if (success)
        {
            shootingSlotSystem.ClearSlot(index);
            Destroy(gameObject);
        }
    }

    private void UpdateCountText(int remainingBullets)
    {
        countText.text = remainingBullets.ToString();
    }


}
