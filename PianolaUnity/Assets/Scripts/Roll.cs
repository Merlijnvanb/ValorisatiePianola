using PrimeTween;
using UnityEngine;
using TMPro;

public class Roll : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    
    public TextMeshProUGUI PieceText;
    public TextMeshProUGUI ComposerText;
    public TextMeshProUGUI PerformerText;

    public RollData Data;

    void Start()
    {
        CanvasGroup.alpha = 0;
    }

    public void Initialize(RollData data)
    {
        Data = data;
        
        PieceText.SetText(Data.PieceTitle);
        ComposerText.SetText(Data.ComposerName);
        PerformerText.SetText(Data.PerformerName);
    }

    public void Enable()
    {
        Tween.Alpha(CanvasGroup, 1, .2f);
    }

    public void Disable()
    {
        Tween.Alpha(CanvasGroup, 0, .2f);
    }
}
