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
    
    

    async void CreateRelay()
    {
        try
        {
           Allocation _allocationHolder = await RelayService.Instance.CreateAllocationAsync(maxNumberOfPlayers - 1);
           string _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocationHolder.AllocationId);

           
           
           NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
               _allocationHolder.RelayServer.IpV4,
               (ushort)_allocationHolder.RelayServer.Port,
               _allocationHolder.AllocationIdBytes,
               _allocationHolder.Key,
               _allocationHolder.ConnectionData
           );
           NetworkManager.Singleton.StartHost();
        }
        
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    async void JoinRelay(string _joinCode)
    {
        try
        {
            Debug.Log($"Joining relay with code {_joinCode}");
            JoinAllocation _joinAllocationHolder = await RelayService.Instance.JoinAllocationAsync(_joinCode);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                
                _joinAllocationHolder.RelayServer.IpV4,
                (ushort)_joinAllocationHolder.RelayServer.Port,
                _joinAllocationHolder.AllocationIdBytes,
                _joinAllocationHolder.Key,
                _joinAllocationHolder.ConnectionData,
                _joinAllocationHolder.HostConnectionData
            );
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
