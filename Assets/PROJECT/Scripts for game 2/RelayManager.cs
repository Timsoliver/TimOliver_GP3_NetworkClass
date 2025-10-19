using System;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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
    
    [Header("Joining Code")]
    
    [SerializeField] TMP_InputField joinInputCode;
    
    [Header("Generated Host Code")]
    
    [SerializeField] TextMeshProUGUI hostCodeText;

    [Header("UI Groups")]
    
    [SerializeField] private GameObject joinMenu;
    [SerializeField] private GameObject gaming;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(joinInputCode.text));
    }

    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        hostCodeText.text = "Code: " + joinCode;
        
        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
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

            joinMenu.SetActive(false);
            gaming.SetActive(true);

        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Joining Failed: {e.Message}");
        }
    }
}
