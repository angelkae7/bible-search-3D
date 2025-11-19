using UnityEngine;

public class CardRaycaster : MonoBehaviour
{
    public float maxDistance = 100f;
    
    void Update()
    {
        // clic gauche
        if (Input.GetMouseButtonDown(0))
            TryClickCard();
    }

    void TryClickCard()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            var card = hit.collider.GetComponent<ResultCard>();
            if (card != null)
            {
                Debug.Log("OPEN: " + card.Link);
                Application.OpenURL(card.Link);
            }
        }
    }
}
