using System.Collections;
using UnityEngine;

public class ResultCardFocus : MonoBehaviour
{
    [Header("Point de focus devant la caméra (optionnel)")]
    public Transform focusPoint;      // Peut rester vide, le script le trouve/crée

    public float moveSpeed = 5f;
    public float scaleMultiplier = 1.5f;

    Vector3 originalPos;
    Quaternion originalRot;
    Vector3 originalScale;
    bool originalSaved = false;

    public bool IsFocused { get; private set; }

    void Awake()
    {
        // Si aucun focusPoint n'est assigné dans l'Inspector,
        // le script va le chercher ou le créer.
        if (focusPoint == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                // Cherche un enfant "FocusPoint"
                Transform existing = cam.transform.Find("FocusPoint");
                if (existing != null)
                {
                    focusPoint = existing;
                }
                else
                {
                    // Le crée automatiquement devant la caméra
                    GameObject go = new GameObject("FocusPoint");
                    go.transform.SetParent(cam.transform);
                    go.transform.localPosition = new Vector3(0f, 0f, 1.8f); // distance devant la caméra
                    go.transform.localRotation = Quaternion.identity;
                    focusPoint = go.transform;
                }
            }
        }
    }

    void Start()
    {
        SaveOriginalTransform();
    }

    void SaveOriginalTransform()
    {
        if (originalSaved) return;
        originalPos = transform.position;
        originalRot = transform.rotation;
        originalScale = transform.localScale;
        originalSaved = true;
    }

    void OnMouseDown()
    {
        // PC / éditeur : clic souris sur la carte
        if (!IsFocused)
            Focus();
        else
            Unfocus();
    }

    public void Focus()
    {
        if (focusPoint == null) return;

        SaveOriginalTransform();
        StopAllCoroutines();
        StartCoroutine(MoveTo(
            focusPoint.position,
            focusPoint.rotation,
            originalScale * scaleMultiplier,
            true
        ));
    }

    public void Unfocus()
    {
        StopAllCoroutines();
        StartCoroutine(MoveTo(
            originalPos,
            originalRot,
            originalScale,
            false
        ));
    }

    IEnumerator MoveTo(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, bool focusState)
    {
        IsFocused = focusState;

        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            float f = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, f);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, f);
            transform.localScale = Vector3.Lerp(startScale, targetScale, f);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.localScale = targetScale;
    }
}
