using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "ComboRush/Yeni Seviye (Level Data)")]
public class LevelData : ScriptableObject
{
    [Header("Seviye Temel Bilgileri")]
    public int levelNumber = 1;
    public int targetScore = 1000; 

    [Header("Grid Boyutları")]
    public int gridWidth = 6;
    public int gridHeight = 8;

    [Header("Özel Tahta Şekli (Harita)")]
    [Tooltip("1: Hücre var (Coin düşer), 0: Hücre yok (Boşluk/Engel). Aşağıdan yukarıya doğru çalışır.")]
    [TextArea(3, 15)]
    public string gridLayout; 
}