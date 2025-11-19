using UnityEngine;
using TMPro;

public class VoiceStatusUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text listeningText;
    [SerializeField] private TMP_Text transcriptionText;
    [SerializeField] private TMP_Text ttsText;

    [Header("Couleurs")]
    [SerializeField] private Color onColor  = new Color(0.4f, 1f, 0.4f); // vert
    [SerializeField] private Color offColor = new Color(1f, 0.4f, 0.4f); // rouge

    void Awake()
    {
        SetListeningMode(false);   // mode mot-clé au démarrage
        SetTranscription("");
        SetTtsEnabled(false);
    }

    /// <summary>
    /// true = dictée active, false = mode mot-clé
    /// </summary>
    public void SetListeningMode(bool dictationActive)
    {
        if (!listeningText) return;

        if (dictationActive)
        {
            listeningText.text  = "Mode : Dictée (parlez, puis dites \"terminé\")";
            listeningText.color = onColor;
        }
        else
        {
            listeningText.text  = "Mode : Mot-clé (en attente de \"bible\")";
            listeningText.color = offColor;
        }
    }

    public void SetTranscription(string text)
    {
        if (!transcriptionText) return;

        if (string.IsNullOrWhiteSpace(text))
            transcriptionText.text = "Transcription : ...";
        else
            transcriptionText.text = "Transcription : " + text;
    }

    public void SetTtsEnabled(bool enabled)
    {
        if (!ttsText) return;

        ttsText.text  = enabled ? "Synthèse : ON" : "Synthèse : OFF";
        ttsText.color = enabled ? onColor : offColor;
    }
}
