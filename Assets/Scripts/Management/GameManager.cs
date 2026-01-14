using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ProgressBarSystem progressBarSystem;
    
    [SerializeField] private GameObject levelCompletePanel; // ссылка на панель завершения
    [SerializeField] private GameObject levelFailedPanel; // ссылка на панель провала
    [SerializeField] private GameObject surprisePanel; // ссылка на панель сюрприза
    [SerializeField] private TextMeshProUGUI lvlText;
    [SerializeField] private string nextSceneName = "Level2"; // имя следующей сцены

    [SerializeField] private List<GameObject> projectilePrefabs;
    
    [SerializeField] private List<Vector3> spawnPositions = new List<Vector3>
    {
        new Vector3(3.5f, 3f, 0f),
        new Vector3(-3.5f, 3f, 0f),
        new Vector3(0.5f, 3f, 0f)// пример второй позиции
    };

    [SerializeField] private int shootCount = 20;
    [SerializeField] private float spawnCheckRadius = 0.5f;

    private void Start()
    {
        lvlText.text = SceneManager.GetActiveScene().name;
        
        if (progressBarSystem != null)
        {
            progressBarSystem.OnProgressComplete += OnLevelCompleted;
        }
    }

    public void OnLevelCompleted()
    {
        if(surprisePanel != null)
        {
            surprisePanel.SetActive(false);
        }
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Панель завершения уровня не назначена!");
        }
    }

    public void OnLose()
    {
        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(true);
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Имя следующей сцены не указано!");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        PickUpProjectilesSystem[] existing =
            Object.FindObjectsByType<PickUpProjectilesSystem>(FindObjectsSortMode.None);

        for (int prefabIndex = 0; prefabIndex < projectilePrefabs.Count; prefabIndex++)
        {
            if (prefabIndex >= spawnPositions.Count)
                continue;

            Vector3 spawnPos = spawnPositions[prefabIndex];

            bool spawnPointOccupied = false;

            foreach (var pickup in existing)
            {
                if (Vector3.Distance(pickup.transform.position, spawnPos) < spawnCheckRadius)
                {
                    spawnPointOccupied = true;
                    break;
                }
            }

            if (!spawnPointOccupied)
            {
                SpawnProjectileOfType(prefabIndex);
            }
        }
    }


    private void SpawnProjectileOfType(int prefabIndex)
    {
        GridSystem gridSystem = Object.FindAnyObjectByType<GridSystem>();
        if (gridSystem == null)
        {
            Debug.LogWarning("GridSystem не найден!");
            return;
        }

        GameObject prefab = projectilePrefabs[prefabIndex];
        Vector3 pos = spawnPositions[prefabIndex];

        GameObject newProj = Instantiate(prefab, pos, Quaternion.identity);

        if (newProj.TryGetComponent<PickUpProjectilesSystem>(out var pickupSystem))
        {
            pickupSystem.shootCount = shootCount; // используем поле shootCount из GameManager
        }

        if (newProj.TryGetComponent<ShootSystem>(out var shootSystem))
        {
            shootSystem.gridSystem = gridSystem;
        }
        else
        {
            Debug.LogWarning($"У префаба {prefab.name} нет ShootSystem!");
        }

        Debug.Log($"Создан {newProj.name} на позиции {pos}");
    }
}