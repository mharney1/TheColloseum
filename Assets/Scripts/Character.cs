using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Represents a player character in the game. Handles core stats, opponent reference,
/// and subscribes to PhaseManager events. Movement, UI, and actions are added later.
/// </summary>
public class Character : NetworkBehaviour
{
    // Core player properties
    public int health = 550;
    public int exhaustion = 0;
    public int team = -1;
    public int pair = -1;

    // Reference to the opponent character (assigned by PairManager)
    public Character opponent;

    // Networked camera for local player
    private new Camera camera;

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

    /// <summary>
    /// Assigns the opponent and pair key to this character.
    /// Called by PairManager when creating a pair.
    /// </summary>
    /// <param name="newOpponent">The opponent Character</param>
    /// <param name="pairKey">The pair identifier</param>
    public void SetOpponent(Character newOpponent, int pairKey)
    {
        opponent = newOpponent;
        pair = pairKey;

        if (opponent != null)
            Debug.Log($"{name} paired with {opponent.name} (Pair {pair})");
        else
            Debug.Log($"{name} has no opponent assigned.");
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
                break;
            case Phase.Action:
                Debug.Log($"{name} entering Action phase.");
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
            Debug.Log("Waiting for managers...");
            yield return null;

    // Subscribe all characters (server + clients)
    PhaseManager.Instance.CurrentPhase.OnValueChanged += HandlePhaseChanged;

        // Server-only: add to idle players
        if (IsServer)
        {
            PairManager.Instance.AddIdlePlayer(this);
        }

    Debug.Log($"{name} subscribed to phase changes{(IsServer ? " and added to idle players" : "")}.");
}

}