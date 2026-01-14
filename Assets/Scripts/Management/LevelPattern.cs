using UnityEngine;
[CreateAssetMenu(fileName = "LevelPattern", menuName = "Grid/Level Pattern")]
public class LevelPattern : ScriptableObject
{
    public int width = 10;
    public int height = 10;

    [Header("Shared Prefabs")]
    public GameObject redPrefab;
    public GameObject yellowPrefab;
    public GameObject bluePrefab;
    public GameObject greenPrefab;
    public GameObject orangePrefab;
    public GameObject pinkPrefab;
    public GameObject lightBluePrefab;
    public GameObject purplePrefab;

    [Header("Layers")]
    [SerializeField] private int[] baseLayer;    // основной слой
    [SerializeField] private int[] secondLayer;  // второй слой (поверх)

    public GameObject GetPrefabFromLayer(int layer, int x, int y)
    {
        int index = y * width + x;

        int[] targetLayer = (layer == 0) ? baseLayer : secondLayer;

        // Если массив для этого слоя пустой или слишком короткий — возвращаем null
        if (targetLayer == null || index >= targetLayer.Length)
        {
            return null;
        }

        int type = targetLayer[index];

        return type switch
        {
            0 => null,
            1 => redPrefab,
            2 => yellowPrefab,
            3 => bluePrefab,
            4 => greenPrefab,
            5 => orangePrefab,
            6 => pinkPrefab,
            7 => lightBluePrefab,
            8 => purplePrefab,
            _ => null
        };
    }

    public BlockColor GetColorFromLayer(int layer, int x, int y)
{
    int index = y * width + x;

    int[] targetLayer = (layer == 0) ? baseLayer : secondLayer;

    int type = targetLayer[index];

    return type switch
    {
        1 => BlockColor.Red,
        2 => BlockColor.Yellow,
        3 => BlockColor.Blue,
        4 => BlockColor.Green,
        5 => BlockColor.Orange,
        6 => BlockColor.Pink,
        7 => BlockColor.LightBlue,
        8 => BlockColor.Purple,
       // _ => BlockColor.None
    };
}
}
