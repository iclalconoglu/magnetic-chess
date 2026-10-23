using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GameObject magnetPrefab;

    [HideInInspector] public bool isOfflineMode = false;
    [HideInInspector] public int offlineOyuncuSayisi = 2;

    [Header("UI Elemanlarý")]
    public TextMeshProUGUI sureYazisi;
    public TextMeshProUGUI siraYazisi;
    public GameObject winPanel;

    [Header("Bekleme Odasý UI")]
    public TextMeshProUGUI beklemeYazisi;

    [Header("2 Farklý Saha Screen Ögeleri")]
    public GameObject saha4Kisilik;
    public GameObject saha2Kisilik;

    [Header("2 Kiþilik Modun Kutularý")]
    public Transform ikiKisilikP1Kutu;
    public Transform ikiKisilikP2Kutu;

    [Header("4 Kiþilik Modun Kutularý")]
    public Transform dortKisilikP1Kutu;
    public Transform dortKisilikP2Kutu;
    public Transform dortKisilikP3Kutu;
    public Transform dortKisilikP4Kutu;

    [Header("Oyuncu Taþ Listeleri")]
    public List<GameObject> p1Stones = new List<GameObject>();
    public List<GameObject> p2Stones = new List<GameObject>();
    public List<GameObject> p3Stones = new List<GameObject>();
    public List<GameObject> p4Stones = new List<GameObject>();

    [Header("Ceza Taþlarý")]
    public List<GameObject> p1KirmiziToplar = new List<GameObject>();
    public List<GameObject> p2KirmiziToplar = new List<GameObject>();
    public List<GameObject> p3KirmiziToplar = new List<GameObject>();
    public List<GameObject> p4KirmiziToplar = new List<GameObject>();

    private int p1AktifCezaSayisi = 0;
    private int p2AktifCezaSayisi = 0;
    private int p3AktifCezaSayisi = 0;
    private int p4AktifCezaSayisi = 0;

    private int mevcutSira = 1;
    private float mevcutSure = 10f;
    private bool oyunBitti = false;
    private bool oyunBasladi = false;

    private float sonCezaZamani = 0f;
    private const float CEZA_BEKLEME_SURESI = 0.1f;

    // Orijinal font boyutunu saklamak için deðiþken
    private float orijinalSureFontBoyutu = 36f;

    private NetworkVariable<int> toplamOyuncuSayisiAð = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        Instance = this;
        isOfflineMode = LobbyManager.IsOfflineMode;
        offlineOyuncuSayisi = LobbyManager.SecilenOyuncuSayisi;
    }

    void Start()
    {
        if (siraYazisi != null) siraYazisi.text = "PLAYER " + mevcutSira + " oynuyor";
        if (beklemeYazisi != null) beklemeYazisi.gameObject.SetActive(false);

        // Oyun ilk açýldýðýnda Inspector'daki orijinal font büyüklüðünü hafýzaya kaydet
        if (sureYazisi != null)
        {
            orijinalSureFontBoyutu = sureYazisi.fontSize;
        }

        if (isOfflineMode)
        {
            SahalariSec(offlineOyuncuSayisi);
            oyunBasladi = true;
        }
        else
        {
            SahalariSec(toplamOyuncuSayisiAð.Value);
            oyunBasladi = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (isOfflineMode) return;

        if (IsServer)
        {
            toplamOyuncuSayisiAð.Value = LobbyManager.SecilenOyuncuSayisi;
            SahalariSec(toplamOyuncuSayisiAð.Value);
        }
    }

    void SahalariSec(int oyuncuSayisi)
    {
        p1Stones.Clear(); p2Stones.Clear(); p3Stones.Clear(); p4Stones.Clear();
        p1KirmiziToplar.Clear(); p2KirmiziToplar.Clear(); p3KirmiziToplar.Clear(); p4KirmiziToplar.Clear();

        if (oyuncuSayisi == 4)
        {
            if (saha4Kisilik != null) saha4Kisilik.SetActive(true);
            if (saha2Kisilik != null) saha2Kisilik.SetActive(false);

            if (dortKisilikP1Kutu != null) { foreach (Transform t in dortKisilikP1Kutu) { if (t.gameObject.activeSelf) p1Stones.Add(t.gameObject); else p1KirmiziToplar.Add(t.gameObject); } }
            if (dortKisilikP2Kutu != null) { foreach (Transform t in dortKisilikP2Kutu) { if (t.gameObject.activeSelf) p2Stones.Add(t.gameObject); else p2KirmiziToplar.Add(t.gameObject); } }
            if (dortKisilikP3Kutu != null) { foreach (Transform t in dortKisilikP3Kutu) { if (t.gameObject.activeSelf) p3Stones.Add(t.gameObject); else p3KirmiziToplar.Add(t.gameObject); } }
            if (dortKisilikP4Kutu != null) { foreach (Transform t in dortKisilikP4Kutu) { if (t.gameObject.activeSelf) p4Stones.Add(t.gameObject); else p4KirmiziToplar.Add(t.gameObject); } }
        }
        else
        {
            if (saha4Kisilik != null) saha4Kisilik.SetActive(false);
            if (saha2Kisilik != null) saha2Kisilik.SetActive(true); // Olasý isim çakýþmasý düzeltildi

            if (ikiKisilikP1Kutu != null) { foreach (Transform t in ikiKisilikP1Kutu) { if (t.gameObject.activeSelf) p1Stones.Add(t.gameObject); else p1KirmiziToplar.Add(t.gameObject); } }
            if (ikiKisilikP2Kutu != null) { foreach (Transform t in ikiKisilikP2Kutu) { if (t.gameObject.activeSelf) p2Stones.Add(t.gameObject); else p2KirmiziToplar.Add(t.gameObject); } }
        }
    }

    void Update()
    {
        if (oyunBitti) return;

        if (!isOfflineMode)
        {
            int bagliOyuncuSayisi = NetworkManager.Singleton.ConnectedClients.Count;
            int hedefOyuncuSayisi = toplamOyuncuSayisiAð.Value;

            if (!oyunBasladi)
            {
                if (bagliOyuncuSayisi >= hedefOyuncuSayisi)
                {
                    oyunBasladi = true;
                    if (beklemeYazisi != null) beklemeYazisi.gameObject.SetActive(false);

                    // Herkes geldiðinde font boyutunu tekrar eski orijinal büyük haline getir cnm
                    if (sureYazisi != null) sureYazisi.fontSize = orijinalSureFontBoyutu;
                }
                else
                {
                    if (beklemeYazisi != null)
                    {
                        beklemeYazisi.gameObject.SetActive(true);
                        beklemeYazisi.text = "OYUNCULAR BEKLENÝYOR (" + bagliOyuncuSayisi + "/" + hedefOyuncuSayisi + ")";
                    }

                    // AÞKIM BAK BURASI DÜZELDÝ: Yazý sýðsýn diye boyutu küçültüyoruz (Örn: 24f yaptýk, istersen deðiþtirebilirsin)
                    if (sureYazisi != null)
                    {
                        sureYazisi.fontSize = orijinalSureFontBoyutu * 0.65f;
                        sureYazisi.text = "BEKLENÝYOR...";
                    }
                    return;
                }
            }
        }

        if (isOfflineMode)
        {
            if (mevcutSure > 0)
            {
                mevcutSure -= Time.deltaTime;
                if (sureYazisi != null) sureYazisi.text = "SÜRE: " + mevcutSure.ToString("0");
            }
            else { SiraDegistir(); }
        }
        else
        {
            if (!IsServer) return;

            if (mevcutSure > 0)
            {
                mevcutSure -= Time.deltaTime;
                UpdateTimerClientRpc(mevcutSure);
            }
            else { SiraDegistir(); }
        }
    }

    [ClientRpc]
    void UpdateTimerClientRpc(float sure)
    {
        if (isOfflineMode) return;
        if (sureYazisi != null)
        {
            sureYazisi.fontSize = orijinalSureFontBoyutu; // Oyun esnasýnda kesinlikle büyük font kalmasýný garanti ediyoruz
            sureYazisi.text = "SÜRE: " + sure.ToString("0");
        }
    }

    public void PlaceMagnet(Vector3 position)
    {
        if (!oyunBasladi || oyunBitti) return;

        if (isOfflineMode)
        {
            SpawnLokalMagnet(position);
            TasEksilt(mevcutSira);
            SiraDegistir();
            GecikmeliWinKontrolLokal();
        }
        else
        {
            if (NetworkManager.Singleton.LocalClientId != (ulong)(mevcutSira - 1)) return;
            PlaceMagnetServerRpc(position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlaceMagnetServerRpc(Vector3 position)
    {
        if (!oyunBasladi || oyunBitti) return;

        bool hamleYapabilir = false;

        if (mevcutSira == 1 && (p1Stones.Count > 0 || p1AktifCezaSayisi > 0)) hamleYapabilir = true;
        else if (mevcutSira == 2 && (p2Stones.Count > 0 || p2AktifCezaSayisi > 0)) hamleYapabilir = true;
        else if (mevcutSira == 3 && (p3Stones.Count > 0 || p3AktifCezaSayisi > 0)) hamleYapabilir = true;
        else if (mevcutSira == 4 && (p4Stones.Count > 0 || p4AktifCezaSayisi > 0)) hamleYapabilir = true;

        if (hamleYapabilir)
        {
            SpawnAndSyncMagnet(position);
            TasEksilt(mevcutSira);
            SiraDegistir();
            StartCoroutine(GecikmeliWinKontrol());
        }
    }

    void SpawnAndSyncMagnet(Vector3 position)
    {
        GameObject go = Instantiate(magnetPrefab, new Vector3(position.x, 0.2f, position.z), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }

    void SpawnLokalMagnet(Vector3 position)
    {
        Instantiate(magnetPrefab, new Vector3(position.x, 0.2f, position.z), Quaternion.identity);
    }

    void TasEksilt(int oyuncuNo)
    {
        int stoneCount = 0;
        int cezaCount = 0;

        if (oyuncuNo == 1) { stoneCount = p1Stones.Count; cezaCount = p1AktifCezaSayisi; }
        else if (oyuncuNo == 2) { stoneCount = p2Stones.Count; cezaCount = p2AktifCezaSayisi; }
        else if (oyuncuNo == 3) { stoneCount = p3Stones.Count; cezaCount = p3AktifCezaSayisi; }
        else if (oyuncuNo == 4) { stoneCount = p4Stones.Count; cezaCount = p4AktifCezaSayisi; }

        if (isOfflineMode)
        {
            if (oyuncuNo == 1) { if (stoneCount > 0) Destroy(p1Stones[stoneCount - 1]); else if (cezaCount > 0) p1KirmiziToplar[cezaCount - 1].SetActive(false); }
            else if (oyuncuNo == 2) { if (stoneCount > 0) Destroy(p2Stones[stoneCount - 1]); else if (cezaCount > 0) p2KirmiziToplar[cezaCount - 1].SetActive(false); }
            else if (oyuncuNo == 3) { if (stoneCount > 0) Destroy(p3Stones[stoneCount - 1]); else if (cezaCount > 0) p3KirmiziToplar[cezaCount - 1].SetActive(false); }
            else if (oyuncuNo == 4) { if (stoneCount > 0) Destroy(p4Stones[stoneCount - 1]); else if (cezaCount > 0) p4KirmiziToplar[cezaCount - 1].SetActive(false); }
        }
        else
        {
            TasSilGorselClientRpc(oyuncuNo, stoneCount, cezaCount);
        }

        if (oyuncuNo == 1) { if (p1Stones.Count > 0) p1Stones.RemoveAt(p1Stones.Count - 1); else p1AktifCezaSayisi--; }
        else if (oyuncuNo == 2) { if (p2Stones.Count > 0) p2Stones.RemoveAt(p2Stones.Count - 1); else p2AktifCezaSayisi--; }
        else if (oyuncuNo == 3) { if (p3Stones.Count > 0) p3Stones.RemoveAt(p3Stones.Count - 1); else p3AktifCezaSayisi--; }
        else if (oyuncuNo == 4) { if (p4Stones.Count > 0) p4Stones.RemoveAt(p4Stones.Count - 1); else p4AktifCezaSayisi--; }

        if (!isOfflineMode)
        {
            SyncGameStateClientRpc(mevcutSira, p1AktifCezaSayisi, p2AktifCezaSayisi, p3AktifCezaSayisi, p4AktifCezaSayisi);
        }
    }

    [ClientRpc]
    void TasSilGorselClientRpc(int oyuncuNo, int stoneCount, int cezaCount)
    {
        if (isOfflineMode) return;
        if (oyuncuNo == 1) { if (stoneCount > 0) Destroy(p1Stones[stoneCount - 1]); else if (cezaCount > 0) p1KirmiziToplar[cezaCount - 1].SetActive(false); }
        else if (oyuncuNo == 2) { if (stoneCount > 0) Destroy(p2Stones[stoneCount - 1]); else if (cezaCount > 0) p2KirmiziToplar[cezaCount - 1].SetActive(false); }
        else if (oyuncuNo == 3) { if (stoneCount > 0) Destroy(p3Stones[stoneCount - 1]); else if (cezaCount > 0) p3KirmiziToplar[cezaCount - 1].SetActive(false); }
        else if (oyuncuNo == 4) { if (stoneCount > 0) Destroy(p4Stones[stoneCount - 1]); else if (cezaCount > 0) p4KirmiziToplar[cezaCount - 1].SetActive(false); }
    }

    public void StonlariTopla(int adet)
    {
        if (Time.time - sonCezaZamani < CEZA_BEKLEME_SURESI) return;
        sonCezaZamani = Time.time;

        int limit = isOfflineMode ? offlineOyuncuSayisi : toplamOyuncuSayisiAð.Value;
        int cezaAlacakOyuncu = mevcutSira - 1;
        if (cezaAlacakOyuncu < 1) cezaAlacakOyuncu = limit;

        HatalýOyuncuyaCezaEkle(cezaAlacakOyuncu, adet);
    }

    void HatalýOyuncuyaCezaEkle(int oyuncuNo, int adet)
    {
        for (int i = 0; i < adet; i++)
        {
            if (oyunBitti) return;
            if (oyuncuNo == 1 && p1AktifCezaSayisi < p1KirmiziToplar.Count) p1AktifCezaSayisi++;
            else if (oyuncuNo == 2 && p2AktifCezaSayisi < p2KirmiziToplar.Count) p2AktifCezaSayisi++;
            else if (oyuncuNo == 3 && p3AktifCezaSayisi < p3KirmiziToplar.Count) p3AktifCezaSayisi++;
            else if (oyuncuNo == 4 && p4AktifCezaSayisi < p4KirmiziToplar.Count) p4AktifCezaSayisi++;
        }

        if (isOfflineMode)
        {
            for (int i = 0; i < p1KirmiziToplar.Count; i++) if (p1KirmiziToplar[i] != null) p1KirmiziToplar[i].SetActive(i < p1AktifCezaSayisi);
            for (int i = 0; i < p2KirmiziToplar.Count; i++) if (p2KirmiziToplar[i] != null) p2KirmiziToplar[i].SetActive(i < p2AktifCezaSayisi);
            for (int i = 0; i < p3KirmiziToplar.Count; i++) if (p3KirmiziToplar[i] != null) p3KirmiziToplar[i].SetActive(i < p3AktifCezaSayisi);
            for (int i = 0; i < p4KirmiziToplar.Count; i++) if (p4KirmiziToplar[i] != null) p4KirmiziToplar[i].SetActive(i < p4AktifCezaSayisi);
        }
        else
        {
            SyncGameStateClientRpc(mevcutSira, p1AktifCezaSayisi, p2AktifCezaSayisi, p3AktifCezaSayisi, p4AktifCezaSayisi);
        }
    }

    public void SiraDegistir()
    {
        int limit = isOfflineMode ? offlineOyuncuSayisi : toplamOyuncuSayisiAð.Value;

        mevcutSira++;
        if (mevcutSira > limit) mevcutSira = 1;

        mevcutSure = 10f;
        GorselGuncelle();

        if (!isOfflineMode)
        {
            SyncGameStateClientRpc(mevcutSira, p1AktifCezaSayisi, p2AktifCezaSayisi, p3AktifCezaSayisi, p4AktifCezaSayisi);
        }
    }

    void GorselGuncelle()
    {
        if (siraYazisi != null) siraYazisi.text = "PLAYER " + mevcutSira + " oynuyor";
    }

    [ClientRpc]
    void SyncGameStateClientRpc(int sira, int p1Ceza, int p2Ceza, int p3Ceza, int p4Ceza)
    {
        if (isOfflineMode) return;
        mevcutSira = sira;
        p1AktifCezaSayisi = p1Ceza; p2AktifCezaSayisi = p2Ceza;
        p3AktifCezaSayisi = p3Ceza; p4AktifCezaSayisi = p4Ceza;

        for (int i = 0; i < p1KirmiziToplar.Count; i++) if (p1KirmiziToplar[i] != null) p1KirmiziToplar[i].SetActive(i < p1AktifCezaSayisi);
        for (int i = 0; i < p2KirmiziToplar.Count; i++) if (p2KirmiziToplar[i] != null) p2KirmiziToplar[i].SetActive(i < p2AktifCezaSayisi);
        for (int i = 0; i < p3KirmiziToplar.Count; i++) if (p3KirmiziToplar[i] != null) p3KirmiziToplar[i].SetActive(i < p3AktifCezaSayisi);
        for (int i = 0; i < p4KirmiziToplar.Count; i++) if (p4KirmiziToplar[i] != null) p4KirmiziToplar[i].SetActive(i < p4AktifCezaSayisi);

        GorselGuncelle();
    }

    void GecikmeliWinKontrolLokal()
    {
        int limit = offlineOyuncuSayisi;
        if (p1Stones.Count == 0 && p1AktifCezaSayisi == 0) LokalOyunBitir("PLAYER 1 KAZANDI!");
        else if (p2Stones.Count == 0 && p2AktifCezaSayisi == 0) LokalOyunBitir("PLAYER 2 KAZANDI!");
        else if (limit == 4 && p3Stones.Count == 0 && p3AktifCezaSayisi == 0) LokalOyunBitir("PLAYER 3 KAZANDI!");
        else if (limit == 4 && p4Stones.Count == 0 && p4AktifCezaSayisi == 0) LokalOyunBitir("PLAYER 4 KAZANDI!");
    }

    void LokalOyunBitir(string mesaj)
    {
        oyunBitti = true;
        if (siraYazisi != null) siraYazisi.text = mesaj;
        if (winPanel != null) winPanel.SetActive(true);
    }

    IEnumerator GecikmeliWinKontrol()
    {
        yield return new WaitForSeconds(1.0f);
        if (isOfflineMode) yield break;

        if (p1Stones.Count == 0 && p1AktifCezaSayisi == 0) OyunBittiSyncClientRpc("PLAYER 1 KAZANDI!");
        else if (p2Stones.Count == 0 && p2AktifCezaSayisi == 0) OyunBittiSyncClientRpc("PLAYER 2 KAZANDI!");
        else if (toplamOyuncuSayisiAð.Value == 4 && p3Stones.Count == 0 && p3AktifCezaSayisi == 0) OyunBittiSyncClientRpc("PLAYER 3 KAZANDI!");
        else if (toplamOyuncuSayisiAð.Value == 4 && p4Stones.Count == 0 && p4AktifCezaSayisi == 0) OyunBittiSyncClientRpc("PLAYER 4 KAZANDI!");
    }

    [ClientRpc]
    void OyunBittiSyncClientRpc(string mesaj)
    {
        if (isOfflineMode) return;
        oyunBitti = true;
        if (siraYazisi != null) siraYazisi.text = mesaj;
        if (winPanel != null) winPanel.SetActive(true);
    }
}