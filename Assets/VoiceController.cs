// Assets/VoiceController.cs
using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using System.Text;

#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceController : MonoBehaviour
{
    [Header("Configuration des mots-clés (dire en français)")]
    [Tooltip("Mot-clé pour démarrer la dictée vocale")]
    public string motCleDemarrer = "bible";
    [Tooltip("Mot-clé pour arrêter la dictée vocale")]
    public string motCleArreter = "terminé";

    [Header("Paramètres de dictée")]
    [Tooltip("Silence (en secondes) avant arrêt automatique de la dictée")]
    public float delaiSilenceAuto = 5f;
    [Tooltip("Silence initial max (en secondes) au début de la dictée")]
    public float delaiSilenceInitial = 3f;

    [Header("Sortie")]
    [TextArea(3, 6)]
    [Tooltip("Texte final reconnu (sans le mot d’arrêt)")]
    public string texteCapture;

    // Événement à exposer pour ta logique (UI, etc.)
    public event Action<string> QuandDictéeTerminée;

    private readonly StringBuilder tampon = new StringBuilder();
    private readonly CultureInfo cultureFR = new CultureInfo("fr-FR");

    [Header("Référence recherche")]
    [SerializeField] private SearchGrid searchGrid; // glisse ton SearchGrid ici dans l’Inspector

    [Header("UI (statut voix)")]
    [SerializeField] private VoiceStatusUI statusUI;
    [SerializeField] private bool ttsEnabled = false; // juste pour afficher l'état TTS, à lier si besoin

#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
    // --------- CHAMP ETAT WINDOWS.SPEECH UNIQUEMENT SUR WINDOWS ---------
    private KeywordRecognizer recoMotsCles;
    private DictationRecognizer recoDictée;

    // Évite les doubles bascules simultanées
    private bool basculeEnCours = false;
    private bool arretDicteeDemande = false;
#endif

    // ---------------- Cycle de vie ----------------
    void Start()
    {
#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
        // --------- MODE WINDOWS / EDITOR : VOIX ACTIVE ---------
        // Normalise le mot-clé (minuscules sans accents)
        var listeMots = new[] { Normaliser(motCleDemarrer) };

        // Instancie le reconnaisseur de mots-clés
        recoMotsCles = new KeywordRecognizer(listeMots, ConfidenceLevel.Low);
        recoMotsCles.OnPhraseRecognized += QuandMotCleReconnu;
        recoMotsCles.Start();

        Debug.Log($"[Voix] Prêt. Dites \"{motCleDemarrer}\" pour commencer la dictée.");

        if (statusUI != null)
        {
            statusUI.SetListeningMode(false);   // mode mot-clé
            statusUI.SetTranscription("");
            statusUI.SetTtsEnabled(ttsEnabled);
        }
#else
        // --------- MODE WEBGL : VOIX DESACTIVEE ---------
        Debug.LogWarning("[Voix] Reconnaissance vocale non disponible sur WebGL.");

        if (statusUI != null)
        {
            statusUI.SetListeningMode(false);
            statusUI.SetTranscription("Reconnaissance vocale non disponible dans la version WebGL.");
            statusUI.SetTtsEnabled(false);
        }
#endif
    }

    void OnDestroy()
    {
#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
        Nettoyer();
#endif
    }

    void OnApplicationQuit()
    {
#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
        Nettoyer();
#endif
    }

#if !UNITY_WEBGL && UNITY_STANDALONE_WIN
    // ================== PARTIE WINDOWS.SPEECH UNIQUEMENT ==================

    private void Nettoyer()
    {
        try
        {
            if (recoMotsCles != null)
            {
                if (recoMotsCles.IsRunning) recoMotsCles.Stop();
                recoMotsCles.OnPhraseRecognized -= QuandMotCleReconnu;
                recoMotsCles.Dispose();
                recoMotsCles = null;
            }

            if (recoDictée != null)
            {
                recoDictée.DictationHypothesis -= QuandHypothese;
                recoDictée.DictationResult     -= QuandResultat;
                recoDictée.DictationComplete   -= QuandComplete;
                recoDictée.DictationError      -= QuandErreur;

                if (recoDictée.Status == SpeechSystemStatus.Running)
                    recoDictée.Stop();

                recoDictée.Dispose();
                recoDictée = null;
            }

            // Remet le système global dans un état neutre
            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
                PhraseRecognitionSystem.Shutdown();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Voix] Nettoyage: {e.Message}");
        }
    }

    // ---------------- Reconnaissance de mots-clés ----------------
    private void QuandMotCleReconnu(PhraseRecognizedEventArgs args)
    {
        if (basculeEnCours) return;

        var dit = Normaliser(args.text);
        if (dit == Normaliser(motCleDemarrer))
        {
            StartCoroutine(BasculerVersDictee());
        }
    }

    // Arrête proprement le système de mots-clés, puis démarre la dictée
    private IEnumerator BasculerVersDictee()
    {
        basculeEnCours = true;

        // 1) Stopper l’objet KeywordRecognizer si besoin
        if (recoMotsCles != null && recoMotsCles.IsRunning)
            recoMotsCles.Stop();

        // 2) Arrêter le système global de reconnaissance de phrases
        if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            PhraseRecognitionSystem.Shutdown();
        }

        // 3) Attendre l’arrêt effectif
        while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
            yield return null; // attendre une frame

        // 4) Préparer la dictée
        if (recoDictée == null)
        {
            recoDictée = new DictationRecognizer(ConfidenceLevel.Low);
            recoDictée.DictationHypothesis += QuandHypothese;
            recoDictée.DictationResult     += QuandResultat;
            recoDictée.DictationComplete   += QuandComplete;
            recoDictée.DictationError      += QuandErreur;
        }

        recoDictée.AutoSilenceTimeoutSeconds    = Mathf.Max(1f, delaiSilenceAuto);
        recoDictée.InitialSilenceTimeoutSeconds = Mathf.Max(1f, delaiSilenceInitial);

        tampon.Clear();
        texteCapture = string.Empty;
        arretDicteeDemande = false;

        // 5) Démarrer la dictée
        recoDictée.Start();
        Debug.Log($"[Voix] Dictée démarrée. Parlez, puis dites \"{motCleArreter}\" pour arrêter.");

        if (statusUI != null)
        {
            statusUI.SetListeningMode(true); // dictée active
            statusUI.SetTranscription("");   // on repart sur du propre
        }

        basculeEnCours = false;
    }

    // ---------------- Reconnaissance de dictée ----------------
    private void QuandHypothese(string texte)
    {
        // Optionnel : aperçu live
        // Debug.Log($"[Voix] Hypothèse: {texte}");
    }

    private void QuandResultat(string texte, ConfidenceLevel niveau)
    {
        if (arretDicteeDemande) return; // on ignore les segments après demande d’arrêt

        var normalise = Normaliser(texte);
        if (ContientMotArret(normalise, out string avantArret))
        {
            if (!string.IsNullOrWhiteSpace(avantArret))
                tampon.Append(avantArret).Append(' ');

            // Demande un arrêt unique + bascule (coroutine)
            StartCoroutine(BasculerVersMotsCles());
            arretDicteeDemande = true;
        }
        else
        {
            tampon.Append(texte).Append(' ');
        }
    }

    private void QuandComplete(DictationCompletionCause cause)
    {
        Debug.Log($"[Voix] Dictée terminée: {cause}");

        // Si la dictée s'arrête par silence / timeout / autre et
        // qu'on n'a pas déjà demandé de retour via "terminé"
        if (!arretDicteeDemande)
        {
            StartCoroutine(BasculerVersMotsCles());
            arretDicteeDemande = true;
        }
    }

    private void QuandErreur(string message, int hresult)
    {
        Debug.LogError($"[Voix] Erreur dictée: {message} (0x{hresult:X})");

        // En cas d'erreur, on tente aussi de revenir proprement au mode mot-clé
        if (!arretDicteeDemande)
        {
            StartCoroutine(BasculerVersMotsCles());
            arretDicteeDemande = true;
        }
    }

    // ----- La coroutine de retour vers le mode mot-clé -----
    private IEnumerator BasculerVersMotsCles()
    {
        if (basculeEnCours) yield break;
        basculeEnCours = true;

        // 1) Stopper la dictée si elle tourne
        if (recoDictée != null && recoDictée.Status == SpeechSystemStatus.Running)
        {
            recoDictée.Stop();

            // ATTENDRE la fin effective (Stop est async)
            while (recoDictée.Status == SpeechSystemStatus.Running)
                yield return null;
        }

        // 2) Finaliser le texte + event utilisateur
        texteCapture = tampon.ToString().Trim();
        QuandDictéeTerminée?.Invoke(texteCapture);
        Debug.Log($"[Voix] Texte final: {texteCapture}");

        // Met à jour l'UI avec le texte final + retourne en mode mot-clé
        if (statusUI != null)
        {
            statusUI.SetTranscription(texteCapture);
            statusUI.SetListeningMode(false);
        }

        // >>>>>>>>>>>>>>>>>>>> APPEL RECHERCHE ICI <<<<<<<<<<<<<<<<<<<<
        if (searchGrid != null && !string.IsNullOrWhiteSpace(texteCapture))
        {
            // Lance la recherche avec la transcription comme query
            searchGrid.FetchAndPopulate(texteCapture);
        }
        // >>>>>>>>>>>>>>>>>>>> -------------------- <<<<<<<<<<<<<<<<<<<<

        // Reset pour la prochaine session
        tampon.Clear();
        arretDicteeDemande = false;

        // 3) Redémarrer le système global de reconnaissance de phrases
        // (à ce stade, il n’y a plus de dictée en cours)
        if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Running)
            PhraseRecognitionSystem.Restart();

        // 4) Attendre qu’il soit prêt
        while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Running)
            yield return null;

        // 5) Relancer l’écoute du mot-clé
        if (recoMotsCles != null && !recoMotsCles.IsRunning)
            recoMotsCles.Start();

        Debug.Log($"[Voix] En attente du mot-clé \"{motCleDemarrer}\".");
        basculeEnCours = false;
    }
#endif // !UNITY_WEBGL && UNITY_STANDALONE_WIN

    // ---------------- Utilitaires communs ----------------
    private string Normaliser(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        // 1) minuscules FR
        string bas = s.ToLower(cultureFR);

        // 2) suppression des accents
        string formD = bas.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var c in formD)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    private bool ContientMotArret(string normalise, out string avant)
    {
        var stop = Normaliser(motCleArreter);
        int idx = normalise.IndexOf(stop, StringComparison.Ordinal);
        if (idx >= 0)
        {
            avant = normalise.Substring(0, idx);
            return true;
        }
        avant = normalise;
        return false;
    }
}
