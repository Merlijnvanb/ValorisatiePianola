using UnityEngine;

[CreateAssetMenu(fileName = "RollData", menuName = "Scriptable Objects/RollData")]
public class RollData : ScriptableObject
{
    public string MidiPath;
    public string PieceTitle;
    public string ComposerName;
    public string PerformerName;
}
