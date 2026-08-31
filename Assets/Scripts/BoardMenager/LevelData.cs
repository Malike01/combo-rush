using UnityEngine;

public enum ExtraTaskType
{
    None,
    ReachComboCount,        // Exp:  Make x5 Combo 
    IncludeMysteryCoin,     // Exp:  Include Mystery Coin in Combo
    MatchSpecificVariant,   // Exp:  Match Specific Variant
    SpecificColorCombo,     // Exp:  Match Specific Color Combo
    ComboAndVariant,        // Exp:  Make x5 Combo with Specific Variant
    MysteryAndClock         // Exp:  Include Mystery Coin and Clock in Combo
}

[CreateAssetMenu(fileName = "Level_", menuName = "JellyMatch/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Basic Settings")]
    public int levelNumber;
    public int targetScore; // Target score to win the level

    [Header("Extra Task (Ek Görev)")]
    public string taskDescription = "-"; 
    public ExtraTaskType taskType = ExtraTaskType.None;
    public int taskTargetValue = 0;    // Exp:  Target value for the extra task
    public int taskSecondaryValue = 0; // Exp:  Secondary value for the extra task

    [Header("Grid Design")]
    public int gridWidth = 6;
    public int gridHeight = 8;
    [TextArea(3, 5)]
    public string gridLayout;
}