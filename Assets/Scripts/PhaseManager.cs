using Unity.Netcode;
using UnityEngine;
using System.Collections;
using NUnit.Framework;

/// <summary>
/// Manages the game phases for a match.
/// </summary>
public class PhaseManager : NetworkBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int expectedPlayers = 2;

    public NetworkVariable<Phase> CurrentPhase = new(
        Phase.Load,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Destroy duplicates on the same client
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("XXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX");
            return;
        }

        Instance = this;

        Debug.Log(IsServer ? "Server instance of PhaseManager is now ready." : "Client instance of PhaseManager is now ready.");

        if (IsServer)
            StartCoroutine(GameLoop());
    }

    /// <summary>
    /// Main game loop coroutine. Handles each phase in order.
    /// </summary>
    private IEnumerator GameLoop()
    {
        // ---- LOADING PHASE ----
        yield return LoadPhase();

        // ---- PHASE LOOP ----
        while (true)
        {
            // Prepare Phase
            yield return PreparePhase();

            // Action Phase
            yield return ActionPhase();

            // Resolve Phase
            yield return ResolvePhase();

            // End Phase
            yield return EndPhase();
        }
    }

    #region Phase Coroutines

    /// Waits for the PairManager Instance to be created and all players to register in it
    private IEnumerator LoadPhase()
    {
        Debug.Log("Phase: Load phase has begun.");
        CurrentPhase.Value = Phase.Load;

        // Wait until the PairManager exists
        while (PairManager.Instance == null)
        {
            Debug.Log("Waiting for PairManager...");
            yield return new WaitForSeconds(1f);
        }

        // Wait until all expected players are connected
        while (!PairManager.Instance.AllPlayersConnected(expectedPlayers))
        {
            Debug.Log($"Waiting for players... ({NetworkManager.Singleton.ConnectedClientsList.Count}/{expectedPlayers})");
            yield return new WaitForSeconds(3f);
        }

        Debug.Log("All players connected. Proceeding to next phase.");
    }

    /// <summary>
    /// Placeholder for the Prepare Phase logic.
    /// </summary>
    private IEnumerator PreparePhase()
    {
        Debug.Log("Phase: Prepare phase has begun.");
        CurrentPhase.Value = Phase.Prepare;
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// Placeholder for the Action Phase logic.
    /// </summary>
    private IEnumerator ActionPhase()
    {
        Debug.Log("Phase: Action phase has begun.");
        CurrentPhase.Value = Phase.Action;
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// Placeholder for the Resolve Phase logic.
    /// </summary>
    private IEnumerator ResolvePhase()
    {
        Debug.Log("Phase: Resolve phase has begun.");
        CurrentPhase.Value = Phase.Resolve;
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// Placeholder for the End Phase logic.
    /// </summary>
    private IEnumerator EndPhase()
    {
        Debug.Log("Phase: End phase has begun.");
        CurrentPhase.Value = Phase.End;
        yield return new WaitForSeconds(3f);
    }

    #endregion
}
