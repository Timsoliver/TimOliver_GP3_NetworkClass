using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ServerRelay : MonoBehaviour
{
    [SerializeField] private int maxNumberOfPlayers;
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed In {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    

    public async void CreateRelay()
    {
        try
        {
           Allocation allocationHolder = await RelayService.Instance.CreateAllocationAsync(maxNumberOfPlayers - 1);
           string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocationHolder.AllocationId);

           Debug.Log(joinCode);
           
           NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
               allocationHolder.RelayServer.IpV4,
               (ushort) allocationHolder.RelayServer.Port,
               allocationHolder.AllocationIdBytes,
               allocationHolder.Key,
               allocationHolder.ConnectionData
           );
           NetworkManager.Singleton.StartHost();
        }
        
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    public async void JoinRelay(string _joinCode)
    {
        try
        {
            Debug.Log($"Joining relay with code {_joinCode}");
            JoinAllocation joinAllocationHolder = await RelayService.Instance.JoinAllocationAsync(_joinCode);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                
                joinAllocationHolder.RelayServer.IpV4,
                (ushort) joinAllocationHolder.RelayServer.Port,
                joinAllocationHolder.AllocationIdBytes,
                joinAllocationHolder.Key,
                joinAllocationHolder.ConnectionData,
                joinAllocationHolder.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
