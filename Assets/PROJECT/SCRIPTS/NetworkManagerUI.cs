using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{

    [SerializeField] private Button clientBtn, serverBtn, hostBtn;
    [SerializeField] private ServerRelay serverRelay;

    private void Awake()
    {
        clientBtn.onClick.AddListener( () =>
        {
            NetworkManager.Singleton.StartClient();
        });
        
        serverBtn.onClick.AddListener( () =>
        {
            NetworkManager.Singleton.StartServer();
        });
        
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
    }

    private void AssignClientNetworkButton()
    {
        NetworkManager.Singleton.StartClient();
    }
}
