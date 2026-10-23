using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.IO; // Sabit diske doðrudan yazýp okumak için bu kütüphane þart cnm

public class LobbyManager : MonoBehaviour
{
    public static int SecilenOyuncuSayisi = 2;
    public static bool IsOfflineMode = false;
    public static string SecilenOdaAdi = "";
    public static ushort AktifPort = 7777;

    [Header("Paneller")]
    public GameObject girisPaneli;
    public GameObject secimPaneli;
    public GameObject odaListesiPaneli;

    [Header("Oda Listesi Elemanlarý")]
    public Transform odaListesiIcerikKutusu;
    public GameObject odaButonPrefab;

    private string dosyaYolu;

    void Awake()
    {
        // Önbelleðe takýlmamak için bilgisayarýn geçici diski üzerinde ortak bir .txt dosyasý oluþturuyoruz
        dosyaYolu = Path.Combine(Application.persistentDataPath, "canli_oda_listesi_paylasimi.txt");

        // AKILLI SIFIRLAMA: Eðer bu dosya eski bir testten kaldýysa (1 dakikadan eskiyse) otomatik temizle
        if (File.Exists(dosyaYolu))
        {
            System.DateTime sonYazilmaZamani = File.GetLastWriteTime(dosyaYolu);
            if ((System.DateTime.Now - sonYazilmaZamani).TotalSeconds > 60)
            {
                File.Delete(dosyaYolu);
            }
        }
    }

    void Start()
    {
        if (girisPaneli != null) girisPaneli.SetActive(true);
        if (secimPaneli != null) secimPaneli.SetActive(false);
        if (odaListesiPaneli != null) odaListesiPaneli.SetActive(false);
    }

    void Update()
    {
        // Katýl paneli açýk olduðu sürece diski her 30 karede bir doðrudan sorgular (Önbelleðe takýlmaz!)
        if (odaListesiPaneli != null && odaListesiPaneli.activeSelf)
        {
            if (Time.frameCount % 30 == 0)
            {
                OdaListesiniGuncelle();
            }
        }
    }

    public void OyunKurButon()
    {
        IsOfflineMode = false;
        if (girisPaneli != null) girisPaneli.SetActive(false);
        if (secimPaneli != null) secimPaneli.SetActive(true);
    }

    public void CevrimdisiOynaButon()
    {
        IsOfflineMode = true;
        if (girisPaneli != null) girisPaneli.SetActive(false);
        if (secimPaneli != null) secimPaneli.SetActive(true);
    }

    public void OyunaKatilButon()
    {
        IsOfflineMode = false;
        if (girisPaneli != null) girisPaneli.SetActive(false);
        if (odaListesiPaneli != null) odaListesiPaneli.SetActive(true);

        OdaListesiniGuncelle();
    }

    void OdaListesiniGuncelle()
    {
        // Eðer henüz hiçbir pencere dosya oluþturmadýysa lobi tamamen boþ kalýr
        if (!File.Exists(dosyaYolu)) return;

        // Dosyayý doðrudan diskten taze taze okuyoruz
        string kayitliOdalar = File.ReadAllText(dosyaYolu);
        string[] odalar = kayitliOdalar.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Eðer ekrandaki buton sayýsý dosyadaki oda sayýsýyla aynýysa boþuna yeniden üretme
        if (odalar.Length == odaListesiIcerikKutusu.childCount) return;

        // Eski butonlarý tertemiz et
        foreach (Transform child in odaListesiIcerikKutusu)
        {
            Destroy(child.gameObject);
        }

        // Tüm odalarý sýrayla ekrana diz
        foreach (string odaData in odalar)
        {
            string[] parcalar = odaData.Split('|');
            if (parcalar.Length < 2) continue;

            string odaIsmi = parcalar[0];
            ushort odaPortu = ushort.Parse(parcalar[1]);

            GameObject yeniButon = Instantiate(odaButonPrefab, odaListesiIcerikKutusu);

            TextMeshProUGUI tmpText = yeniButon.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = odaIsmi;
            else yeniButon.GetComponentInChildren<Text>().text = odaIsmi;

            yeniButon.GetComponent<Button>().onClick.AddListener(() => OdayaBaglan(odaPortu));
        }
    }

    void OdayaBaglan(ushort port)
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("127.0.0.1", port);
            }
            NetworkManager.Singleton.StartClient();
        }
    }

    public void IkiKisilikSec()
    {
        SecilenOyuncuSayisi = 2;
        OdaKurulumunuTamamla();
    }

    public void DortKisilikSec()
    {
        SecilenOyuncuSayisi = 4;
        OdaKurulumunuTamamla();
    }

    void OdaKurulumunuTamamla()
    {
        if (!IsOfflineMode)
        {
            int mevcutOdaSayisi = 0;
            string eskiOdalarZinciri = "";

            // Dosya varsa önce içindeki mevcut kurulmuþ odalarý oku ve sayýsýný al
            if (File.Exists(dosyaYolu))
            {
                eskiOdalarZinciri = File.ReadAllText(dosyaYolu);
                string[] odalar = eskiOdalarZinciri.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                mevcutOdaSayisi = odalar.Length;
            }

            int yeniOdaNo = mevcutOdaSayisi + 1;
            SecilenOdaAdi = "Oda " + yeniOdaNo + " (" + SecilenOyuncuSayisi + " Kiþilik)";
            AktifPort = (ushort)(7777 + mevcutOdaSayisi); // Oda 1 -> 7777, Oda 2 -> 7778, Oda 3 -> 7779...

            // Yeni odayý eskisinin altýna ekleyip doðrudan diske kazýyoruz!
            string yeniOdalarZinciri = eskiOdalarZinciri + SecilenOdaAdi + "|" + AktifPort + ";";
            File.WriteAllText(dosyaYolu, yeniOdalarZinciri);
        }

        if (IsOfflineMode)
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            AgiBaslatVeSahneAc();
        }
    }

    private void AgiBaslatVeSahneAc()
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("127.0.0.1", AktifPort);
            }
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}