using UnityEngine;

public class BoardClick : MonoBehaviour
{
    void Update()
    {
        // Fare sol týklandýðýnda
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Kameradan farenin olduðu yere bir ýþýn gönderiyoruz
            if (Physics.Raycast(ray, out hit))
            {
                // EÐER ÇARPTIÐIMIZ OBJENÝN TAG'Ý "Board" ÝSE
                if (hit.collider.CompareTag("Board"))
                {
                    if (GameManager.Instance != null)
                    {
                        // Sadece Board üzerine týklandýysa taþ koy
                        GameManager.Instance.PlaceMagnet(hit.point);
                    }
                }
                else
                {
                    // Board dýþýnda bir yere (Plane vb.) týklandýðýnda hiçbir þey yapmaz
                    Debug.Log("Board dýþýna týklandý: " + hit.collider.name);
                }
            }
        }
    }
}
