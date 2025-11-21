using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class Lava : NetworkBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (!IsServer) return;
      
      if (!other.CompareTag("GroundCheck")) return;
      
      Player player = other.GetComponentInParent<Player>();
      if (player == null) return;
      
      player.KillPlayerClientRpc();
   }
}
