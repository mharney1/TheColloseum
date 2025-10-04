using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    [Header("Spawner Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    private void Awake()
    {
        
        // Singleton enforcement (server only)
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate SpawnerManager found on {gameObject.name}. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
        else
        {
            Debug.LogError("SpawnerManager: NetworkManager not found!");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Client {clientId} connected. Queuing spawn...");
        StartCoroutine(SpawnPlayerWithDelay(clientId));
    }

    private IEnumerator SpawnPlayerWithDelay(ulong clientId)
    {
        // Small delay to avoid race conditions
        yield return new WaitForSeconds(0.25f);

        if (playerPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnerManager: Missing prefab or spawn points.");
            yield break;
        }

        var spawnPoint = spawnPoints[nextSpawnIndex % spawnPoints.Length];
        nextSpawnIndex++;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        playerInstance.name = $"Player_{clientId}";

        var netObj = playerInstance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError($"Player prefab is missing a NetworkObject! Cannot spawn {playerInstance.name}.");
            Destroy(playerInstance);
            yield break;
        }

        if (!netObj.IsSpawned)
        {
            netObj.SpawnAsPlayerObject(clientId);
        }
        else
        {
            Debug.LogWarning($"Tried to spawn Player_{clientId}, but object was already spawned.");
        }
    }
}