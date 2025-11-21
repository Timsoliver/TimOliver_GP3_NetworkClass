using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class SpawnButtonUI : MonoBehaviour
{
    public void OnSpawnClicked()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            return;
        
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObj == null) return;
        
        var player = playerObj.GetComponent<Player>();
        if (player == null) return;
        
        player.RequestSpawn();
    }
}
