using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CenterUIClicker : MonoBehaviour
{
    void Update()
    {
        // clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            TryClickCenterCard();
        }
    }

    void TryClickCenterCard()
    {
        if (EventSystem.current == null) return;

        // pointer virtuel au CENTRE de l'écran
        var data = new PointerEventData(EventSystem.current);
        data.position = new Vector2(Screen.width / 2f, Screen.height / 2f);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            var card = r.gameObject.GetComponentInParent<ResultCard>();
            if (card != null)
            {
                card.OpenLink();   // 👈 utilise ta méthode
                break;
            }
        }
    }
}
