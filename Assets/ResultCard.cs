using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class ResultCard : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text snippetText;
    [SerializeField] private Image thumbImage;
    [SerializeField] private Sprite fallbackSprite;

    // 👉 URL de la page à ouvrir quand on clique sur la carte
    private string linkUrl;
    public string Link => linkUrl;   // utile si tu veux y accéder ailleurs

    public void Setup(string title, string snippet, string imageUrl, string link)
    {
        linkUrl = link;

        if (titleText)   titleText.text   = string.IsNullOrWhiteSpace(title)   ? "(Sans titre)" : title;
        if (snippetText) snippetText.text = string.IsNullOrWhiteSpace(snippet) ? ""            : snippet;

        if (thumbImage) thumbImage.preserveAspect = true;

        if (!string.IsNullOrEmpty(imageUrl))
            StartCoroutine(LoadImage(imageUrl));
        else if (fallbackSprite && thumbImage)
            thumbImage.sprite = fallbackSprite;
    }

    private IEnumerator LoadImage(string url)
    {
        using var req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isHttpError || req.isNetworkError)
#endif
        {
            if (fallbackSprite && thumbImage) thumbImage.sprite = fallbackSprite;
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(req);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        if (thumbImage)
        {
            thumbImage.sprite = sprite;
            thumbImage.preserveAspect = true;
        }
    }

    // 👉 méthode commune pour ouvrir le lien
    public void OpenLink()
    {
        if (!string.IsNullOrEmpty(linkUrl))
        {
            Debug.Log("Open URL: " + linkUrl);
            Application.OpenURL(linkUrl);
        }
    }

    // 👉 Clic souris direct sur la carte (UI classique)
    public void OnPointerClick(PointerEventData eventData)
    {
        OpenLink();
    }
}
