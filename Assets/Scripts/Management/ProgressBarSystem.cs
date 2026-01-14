using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarSystem : MonoBehaviour
{
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private Image progressFillImage;

    public event Action OnProgressComplete;

    private int totalBlocks;
    private int currentBlocks;
    private bool isLevelCompleted = false;

    private void Awake()
    {
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        if (progressFillImage == null)
            Debug.LogError("ProgressBarSystem: progressFillImage не назначен!");
    }

    private void Start()
    {
        if (gridSystem != null)
    {
        totalBlocks = gridSystem.width * gridSystem.height;
        gridSystem.OnAllColumnsShiftedComplete += OnGridUpdated;
    }
        UpdateProgressImmediate();
    }

    private void OnGridUpdated()
    {
        UpdateProgressImmediate();
    }

    private void UpdateProgressImmediate()
    {
        if (gridSystem == null || gridSystem.gridCells == null || gridSystem.gridCells.Count == 0)
            return; // сетка ещё не готова — выходим
        currentBlocks = gridSystem.GetRemainingBlocksCount();

        float progress = totalBlocks > 0 ? 1f - (float)currentBlocks / totalBlocks: 0f;

        progressFillImage.fillAmount = progress;

        if(progress >= 1f && !isLevelCompleted)
        {
            isLevelCompleted = true;
            OnProgressComplete?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (gridSystem != null)
            gridSystem.OnAllColumnsShiftedComplete -= OnGridUpdated;
    }
}
