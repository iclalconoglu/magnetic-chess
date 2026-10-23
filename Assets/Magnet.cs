using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Magnet : MonoBehaviour
{
    public float cekimGucu = 10f;
    public float cekimMesafesi = 5f;
    private Rigidbody rb;
    private bool siliniyorMu = false;
    // Bu mýknatýsý hangi oyuncunun tahtaya býraktýðýný hafýzasýnda tutacak
  
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (siliniyorMu) return;

        Magnet[] tumMiknatislar = FindObjectsByType<Magnet>(FindObjectsSortMode.None);
        foreach (Magnet diger in tumMiknatislar)
        {
            if (diger == this || diger == null || diger.siliniyorMu) continue;

            float mesafe = Vector3.Distance(transform.position, diger.transform.position);
            if (mesafe < cekimMesafesi)
            {
                Vector3 yon = (transform.position - diger.transform.position).normalized;
                Rigidbody digerRb = diger.GetComponent<Rigidbody>();
                if (digerRb != null)
                {
                    digerRb.AddForce(yon * cekimGucu);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (siliniyorMu) return;

        if (collision.gameObject.GetComponent<Magnet>())
        {
            List<GameObject> yapisanGrup = new List<GameObject>();
            ZinciriBul(gameObject, yapisanGrup);

            if (yapisanGrup.Count >= 2)
            {
                // Silme sürecini yarým saniye gecikmeli baþlat
                StartCoroutine(YarimSaniyeSonraSil(yapisanGrup));
            }
        }
    }

    IEnumerator YarimSaniyeSonraSil(List<GameObject> grup)
    {
        // Önce gruptaki tüm taþlarý iþaretle ki baþka çekimlere girmesinler
        foreach (GameObject g in grup)
        {
            if (g != null && g.GetComponent<Magnet>())
                g.GetComponent<Magnet>().siliniyorMu = true;
        }

        // Ýstediðiniz gibi yarým saniye bekle
        yield return new WaitForSeconds(0.5f);

        // GameManager'a bildir
        GameManager.Instance.StonlariTopla(grup.Count);

        // Taþlarý sahadan kaldýr
        foreach (GameObject g in grup)
        {
            if (g != null) Destroy(g);
        }
    }

    void ZinciriBul(GameObject obje, List<GameObject> liste)
    {
        if (liste.Contains(obje)) return;
        liste.Add(obje);

        Collider[] temasEdenler = Physics.OverlapSphere(obje.transform.position, 0.65f);
        foreach (var temas in temasEdenler)
        {
            if (temas.gameObject != obje && temas.gameObject.GetComponent<Magnet>())
            {
                ZinciriBul(temas.gameObject, liste);
            }
        }
    }
}