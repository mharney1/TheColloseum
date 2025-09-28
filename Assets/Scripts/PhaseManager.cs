using Unity.Netcode;
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;

/// <summary>
/// Manages the game phases for a match.
/// </summary>
public class PhaseManager : NetworkBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int expectedPlayers = 2;

    [Header("Prepare Phase Timing")]
    [SerializeField] private float startTime = 15f;  // Starting timer
    [SerializeField] private float shortenedTime = 5f; // Reduced time when all ready
    private NetworkVariable<float> timer = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private List<Character> undecidedCharacters;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI prepareTimerText;

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

        timer.OnValueChanged += (oldVal, newVal) =>
        {
            if (prepareTimerText != null)
                prepareTimerText.text = Mathf.CeilToInt(newVal).ToString("00");
        };

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
            Debug.Log($"Waiting for players... {PairManager.Instance.GetIdlePlayers()}/{expectedPlayers})");
            yield return new WaitForSeconds(3f);
        }

        Debug.Log("All players connected attemping to Pair players.");
        PairManager.Instance.TryPairing();
    }

    /// <summary>
    /// Placeholder for the Prepare Phase logic.
    /// </summary>
    private IEnumerator PreparePhase()
    {
        Debug.Log("Phase: Prepare phase has begun.");
        CurrentPhase.Value = Phase.Prepare;

        timer.Value = startTime;
        undecidedCharacters = new List<Character>(PairManager.Instance.GetCombatants());

        SetPrepareTimerActiveClientRpc(true);

        while (timer.Value > 0f)
            {
                if (undecidedCharacters.Count == 0 && timer.Value > shortenedTime)
                    timer.Value = shortenedTime;

                if (IsServer)
                {
                    timer.Value -= Time.deltaTime;
                    timer.Value = Mathf.Clamp(timer.Value, 0f, startTime); // Clamp to avoid negative
                }

                yield return null;
            }

        DefaultChoice(undecidedCharacters);

        SetPrepareTimerActiveClientRpc(false);

        Debug.Log($"{(undecidedCharacters.Count == 0 ? "Everyone has a choice." : "Someone hasn't chosen.")}");
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
        Debug.Log("All players connected attemping to Pair players.");
        PairManager.Instance.TryPairing();
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

    private void DefaultChoice(List<Character> characters)
    {
        foreach (Character character in new List<Character>(characters))
        {
            character.SetCurrentChoice(Choices.Attack);
        }
    }
    public void RemoveUndecided(Character player)
    {
        undecidedCharacters.Remove(player);
    }

    [ClientRpc]
    private void SetPrepareTimerActiveClientRpc(bool isActive)
    {
        if (prepareTimerText != null)
        {
            prepareTimerText.transform.parent.gameObject.SetActive(isActive);
            Debug.Log($"{name} is attempting to {(isActive ? "enable" : "disable")} the prepare timer.");
        }
        else
        {
            Debug.LogWarning($"{name} cannot set prepare timer active: TextMeshProUGUI is null!");
        }
    }
}