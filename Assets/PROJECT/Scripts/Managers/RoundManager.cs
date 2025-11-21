using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("resetButton")]
    [SerializeField] private GameObject resetButton;
    
    private readonly List<Player> players = new List<Player>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this; 
        
    }

    public void RegisterPlayer(Player player)
    {
        if (NetworkManager.Singleton == null|| !NetworkManager.Singleton.IsServer) 
            return;
        
        if (!players.Contains(player))
            players.Add(player);
    }

    public void UnregisterPlayer(Player player)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        
        if (players.Contains(player)) 
            players.Remove(player);
    }

    public int GetPlayerIndex(Player player)
    {
        return players.IndexOf(player);
    }

    public Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        if (index < 0) index = 0;
        return spawnPoints[index % spawnPoints.Length].position;
    }

    public void OnPlayerDied(Player deadplayer)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) 
            return;

        int aliveCount = 0;
        Player lastAlive = null;

        foreach (var p in players)
        {
            if (p == null) continue;

            if (!p.IsDead)
            {
                aliveCount++;
                lastAlive = p;
            }
        }

        if (aliveCount == 1 && lastAlive != null)
        {
            lastAlive.ShowWinClientRpc();
            
            if (resetButton != null) 
                resetButton.SetActive(true);
        }
    }

    public void ResetRound()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var p in players)
        {
            if (p == null) continue;
            p.EnterPreRoundStateClientRpc();
        }
    }
}
