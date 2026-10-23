using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobiSistemi : MonoBehaviour
{
    public void HostOl()
    {
        // Bu buton hem sunucuyu açar hem de sizi oyuncu olarak içine sokar
        NetworkManager.Singleton.StartHost();
        Debug.Log("Sunucu kuruldu, oyuncu bekleniyor...");
    }

    public void KatiliciOl()
    {
        // Bu buton var olan bir sunucuya (IP adresi üzerinden) baðlanmanýzý saðlar
        NetworkManager.Singleton.StartClient();
        Debug.Log("Sunucuya baðlanýlýyor...");
    }
}