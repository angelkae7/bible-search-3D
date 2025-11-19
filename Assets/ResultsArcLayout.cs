using System.Collections.Generic;
using UnityEngine;

public class ResultsArcLayout : MonoBehaviour
{
    [Header("Centre = devant le joueur")]
    public Transform center;       // ex : Player ou Main Camera
    public float radius = 3f;      // distance des cartes
    public float angleRange = 60f; // ouverture de l’arc en degrés
    public float heightOffset = 0f;

    void LateUpdate()
    {
        ArrangeChildrenInArc();
    }

    void ArrangeChildrenInArc()
    {
        int count = transform.childCount;
        if (count == 0 || center == null) return;

        for (int i = 0; i < count; i++)
        {
            Transform card = transform.GetChild(i);

            // Si la carte est en focus, on ne la bouge pas
            var focus = card.GetComponent<ResultCardFocus>();
            if (focus != null && focus.IsFocused) continue;

            float t = (count == 1) ? 0.5f : (float)i / (count - 1); // 0 → 1
            float angle = Mathf.Lerp(-angleRange * 0.5f, angleRange * 0.5f, t);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = center.position + new Vector3(
                Mathf.Sin(rad) * radius,
                heightOffset,
                Mathf.Cos(rad) * radius
            );

            card.position = pos;

            // Regarde vers le centre (en gardant la même hauteur)
            Vector3 lookTarget = new Vector3(center.position.x, card.position.y, center.position.z);
            card.LookAt(lookTarget);
        }
    }
}
