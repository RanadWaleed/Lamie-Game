using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MatchPair
{
    public string pairID;
    public Sprite draggableSprite;
    public Sprite targetSprite;
}

[CreateAssetMenu(fileName = "NewStageData", menuName = "Lamie/Stage Data")]
public class StageData : ScriptableObject
{
    public string stageName;
    public float standardTime = 12f;
    public List<MatchPair> matchPairs = new List<MatchPair>();

    public List<Sprite> distractors = new List<Sprite>();
}