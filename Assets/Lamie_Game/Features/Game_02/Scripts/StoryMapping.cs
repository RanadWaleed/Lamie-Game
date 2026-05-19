using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStoryMapping", menuName = "Lamie/Story Mapping")]
public class StoryMapping : ScriptableObject
{
    public string storyId;
    public string storyMood;
    public string correctTitleId;

    [Header("Shapes")]
    public List<string> allowedSky;
    public List<string> allowedBuildings;
    public List<string> allowedCharacters;

    [Header("Symbols & Decorations")]
    public List<string> allowedSymbols;
    public int expectedSymbolsCount = 7;
    public int maxDecorAvailable = 8;

    [Header("Colors")]
    public List<string> paletteMood;

    [Header("Standard Times (seconds)")]
    public float standardTimeShapes = 12f;
    public float standardTimeSymbols = 15f;
    public float maxDesignTime = 180f;
    public float areaThresholdRatio = 0.6f;
}