using System;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

/// <summary>
/// Represents a player character in the game. Handles core stats, opponent reference,
/// and subscribes to PhaseManager events. Movement, UI, and actions are added later.
/// </summary>
public class Character : NetworkBehaviour
{
    // Core player properties
    private NetworkVariable<int> health = new NetworkVariable<int>(
        550, // default starting value
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<int> exhaustion = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private int team = -1;
    private int pair = -1;
    private bool isCycling = false;
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    private NetworkVariable<Choices> currentChoice = new NetworkVariable<Choices>(
    Choices.None,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
    // Reference to the opponent character (assigned by PairManager)
    private NetworkVariable<ulong> opponentId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Called when this NetworkObject spawns.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Dynamically find and enable the camera
            Camera cam = GetComponentInChildren<Camera>(true);
            if (cam != null)
                cam.gameObject.SetActive(true);
            else
                Debug.LogWarning("No camera found on player prefab!");
        }

        // Only the server should handle both registration and phase subscription
        StartCoroutine(RegisterAndSubscribe());
    }
    private void OnDisable()
    {
        // Unsubscribe from PhaseManager events to avoid memory leaks
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.CurrentPhase.OnValueChanged -= HandlePhaseChanged;
        }
    }
    private void Update()
    {
        if (!IsServer) return; // server handles movement only

        if (isCycling)
        {
            // Move constantly to the right
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);

            // Face the opponent directly
            Character opponent = GetOpponent();
            if (opponent != null)
            {
                Vector3 direction = opponent.transform.position - transform.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    /// <summary>
    /// Handles actions when the phase changes.
    /// Currently just logs, movement and actions will be added later.
    /// </summary>
    private void HandlePhaseChanged(Phase previous, Phase current)
    {
        switch (current)
        {
            case Phase.Prepare:
                Debug.Log($"{name} entering Prepare phase.");
                isCycling = opponentId.Value != 0;
                break;
            case Phase.Action:
                Debug.Log($"{name} entering Action phase.");
                isCycling = false;
                break;
            case Phase.Resolve:
                Debug.Log($"{name} entering Resolve phase.");
                break;
            case Phase.End:
                Debug.Log($"{name} entering End phase.");
                break;
        }
    }
    private IEnumerator RegisterAndSubscribe()
    {
        Debug.Log($"{name} has begun the coroutine on the {(IsServer ? "server" : "client")}.");
        // Wait until both managers exist
        while (PairManager.Instance == null || PhaseManager.Instance == null)
        {
            Debug.Log("Waiting for managers...");
            yield return null;
        }

        // Subscribe all characters (server + clients)
        PhaseManager.Instance.CurrentPhase.OnValueChanged += HandlePhaseChanged;

        // Server-only: add to idle players
        if (IsServer)
        {
            PairManager.Instance.AddIdlePlayer(this);
        }

        Debug.Log($"{name} subscribed to phase changes{(IsServer ? " and added to idle players" : "")}.");
    }

    public int GetHealth() => health.Value;
    public int GetExhaustion() => exhaustion.Value;
    public int GetTeam() => team;
    public int GetPair() => pair;
    public Character GetOpponent()
    {
        if (opponentId.Value == 0) return null; // no opponent assigned yet

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(opponentId.Value, out var netObj))
        {
            return netObj.GetComponent<Character>();
        }
        return null;
    }

    public void ModifyHealth(int difference)
    {
        if (IsServer)
        {
            health.Value += difference;
        }
    }
    public void ModifyExhaustion(int difference)
    {
        if (IsServer)
            exhaustion.Value += difference;
    }
    public void SetCurrentChoice(Choices newChoice)
    {
        currentChoice.Value = newChoice;
        if (IsServer)
            PhaseManager.Instance.RemoveUndecided(this);
    }
    public void SetOpponent(Character newOpponent, int newPair)
    {
        if (IsServer)
        {
            opponentId.Value = newOpponent != null ? newOpponent.NetworkObjectId : 0;
            pair = newPair;
        }
        if (newOpponent != null)
            Debug.Log($"{name} paired with {newOpponent.name} (Pair {pair})");
        else
            Debug.Log($"{name} has no opponent assigned.");
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void SetChoiceServerRpc(Choices newChoice)
    {
        currentChoice.Value = newChoice;
        PhaseManager.Instance.RemoveUndecided(this);
    }
}