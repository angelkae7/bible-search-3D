using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class SearchGrid : MonoBehaviour
{
    [Header("Google Custom Search")]
    [SerializeField] private string apiKey = "AIzaSyCbt_rBnRAlDIYY6ymLIdtFfjXW7Vs3ckA";
    [SerializeField] private string searchEngineId = "61231c4044eb14cc6";
    [SerializeField] private int num = 6;

    [Header("UI Refs")]
    [SerializeField] private Transform gridParent;   // WallCanvas/Grid
    [SerializeField] private ResultCard cardPrefab;  // Prefab ResultCard

    private Coroutine runningFetch;

    /// <summary>
    /// Lance une recherche avec la query passée en paramètre.
    /// Si une recherche est déjà en cours, elle est annulée proprement.
    /// </summary>
    public Coroutine FetchAndPopulate(string q)
    {
        if (runningFetch != null) StopCoroutine(runningFetch);
        runningFetch = StartCoroutine(FetchAndPopulateRoutine(q));
        return runningFetch;
    }

    private IEnumerator FetchAndPopulateRoutine(string query)
    {
        query = (query ?? "").Trim();
        if (string.IsNullOrEmpty(query))
        {
            Debug.LogWarning("[Search] Query vide, abandon.");
            runningFetch = null; yield break;
        }

        string url =
            $"https://www.googleapis.com/customsearch/v1" +
            $"?key={apiKey}&cx={searchEngineId}" +
            $"&q={UnityWebRequest.EscapeURL(query)}" +
            $"&num={num}&hl=fr&lr=lang_fr&safe=off";

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
        {
            Debug.LogError("[Search] Erreur requête: " + req.error + "\n" + req.downloadHandler.text);
            runningFetch = null; yield break;
        }

        PopulateFromJson(req.downloadHandler.text);
        runningFetch = null;
    }

    private void PopulateFromJson(string json)
    {
        ClearGrid();

        var root = JsonUtility.FromJson<GoogleRoot>(json);
        if (root?.items == null || root.items.Length == 0)
        {
            Debug.LogWarning("[Search] Aucun résultat.");
            return;
        }

        foreach (var it in root.items)
        {
            string title = WebUtility.HtmlDecode(it.title ?? "");
            string snippet = WebUtility.HtmlDecode(it.snippet ?? "");
            string img = (it.pagemap?.cse_image != null && it.pagemap.cse_image.Length > 0)
                ? it.pagemap.cse_image[0]?.src
                : null;

                string link = it.link;



            var card = Instantiate(cardPrefab, gridParent);
            card.transform.localScale = Vector3.one; // GridLayout gère la position
            card.Setup(title, snippet, img, link);
        }
    }

    private void ClearGrid()
    {
        if (!gridParent) return;
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
    }

    // ---- DTOs internes ----
    [Serializable] private class GoogleRoot { public GoogleItem[] items; }
    [Serializable] private class GoogleItem { public string title; public string snippet; public GooglePageMap pagemap; public string link;   }
    [Serializable] private class GooglePageMap { public GoogleCseImage[] cse_image; }
    [Serializable] private class GoogleCseImage { public string src; }
}
