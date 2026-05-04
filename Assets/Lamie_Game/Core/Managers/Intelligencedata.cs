using UnityEngine;

[CreateAssetMenu(fileName = "NewIntelligence", menuName = "Lamie/Intelligence Data")]
public class IntelligenceData : ScriptableObject
{
    [Header("Identity")]
    public string intelligenceId;
    public string intelligenceName;

    [Header("Games")]
    public string[] gameSceneNames;


}