using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [Header("Relay Buttons")]
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;
    
    [Header("Joining Code")]
    [SerializeField] TMP_InputField joinInputCode;
    
    [Header("Generated Host Code")]
    [SerializeField] TextMeshProUGUI hostCodeText;
    [SerializeField] TextMeshProUGUI joinCodeText;
    
    [Header("UI Groups")]
    [SerializeField] private GameObject joinMenu;
    [SerializeField] private GameObject gaming;
    [SerializeField] private GameObject menu;
    
    [Header("Text Change")]
    [SerializeField] TextMeshProUGUI leaveButtonText;
    [SerializeField] TextMeshProUGUI titleText;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(joinInputCode.text));
        leaveButton.onClick.AddListener(LeaveGame);
    }

    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
       
        hostCodeText.text = "Code: " + joinCode;

        titleText.text = "Host";

        leaveButtonText.text = "Stop Game";
        
        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
        
        leaveButton.gameObject.SetActive(true);
        
        joinMenu.SetActive(false);
        gaming.SetActive(true);
    }

    async void JoinRelay(string joinCode)
    {

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogWarning("JoinCode is empty");
            return;
        }
        
        
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
        
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
            NetworkManager.Singleton.StartClient();
            
            joinCodeText.text = "Room Code: " + joinCode;
            
            titleText.text = "Player";
            
            leaveButtonText.text = "Leave Game";
            
            leaveButton.gameObject.SetActive(true);

            joinMenu.SetActive(false);
            gaming.SetActive(true);
            

        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Joining Failed: {e.Message}");
        }
    }

    public void LeaveGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        leaveButton.gameObject.SetActive(false);
        
        menu.SetActive(true);
        gaming.SetActive(false);

        hostCodeText.text = "";
        joinInputCode.text = "";
        joinCodeText.text = "";
        titleText.text = "";
        leaveButtonText.text = "";
        
        Debug.Log("Leaving Game + network shutdown.");
    }
}
