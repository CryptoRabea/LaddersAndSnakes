using UnityEngine;
using Unity.Netcode;
using LAS.Entities;

namespace LAS.Networking
{
    /// <summary>
    /// Network-synced player piece for online multiplayer
    /// </summary>
    public class NetworkPlayerPiece : NetworkBehaviour
    {
        [SerializeField] private PlayerPiece playerPiece;

        private NetworkVariable<int> networkCurrentIndex = new NetworkVariable<int>(1);
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero);

        public override void OnNetworkSpawn()
        {
            if (playerPiece == null)
                playerPiece = GetComponent<PlayerPiece>();

            if (!IsOwner)
            {
                // Subscribe to position changes
                networkPosition.OnValueChanged += OnPositionChanged;
                networkCurrentIndex.OnValueChanged += OnIndexChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                networkPosition.OnValueChanged -= OnPositionChanged;
                networkCurrentIndex.OnValueChanged -= OnIndexChanged;
            }
        }

        private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
        {
            transform.position = newValue;
        }

        private void OnIndexChanged(int oldValue, int newValue)
        {
            if (playerPiece != null)
                playerPiece.currentIndex = newValue;
        }

        /// <summary>
        /// Update position on server
        /// </summary>
        [ServerRpc]
        public void UpdatePositionServerRpc(Vector3 position, int index)
        {
            networkPosition.Value = position;
            networkCurrentIndex.Value = index;
        }

        /// <summary>
        /// Sync local changes to network
        /// </summary>
        public void SyncToNetwork()
        {
            if (IsOwner && playerPiece != null)
            {
                UpdatePositionServerRpc(transform.position, playerPiece.currentIndex);
            }
        }

        private void Update()
        {
            // Periodically sync position if owner
            if (IsOwner && playerPiece != null)
            {
                if (Vector3.Distance(transform.position, networkPosition.Value) > 0.01f ||
                    playerPiece.currentIndex != networkCurrentIndex.Value)
                {
                    SyncToNetwork();
                }
            }
        }
    }
}
