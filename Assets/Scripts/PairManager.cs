using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-authoritative manager that handles pairing players.
/// Tracks idle players who are not yet in a pair.
/// Only one instance exists on the server; all client instances are destroyed.
/// </summary>
public class PairManager : NetworkBehaviour
{
    public static PairManager Instance { get; private set; }

    // Declaration of all variables and data structures.
    private readonly List<Character> idlePlayers = new();
    private readonly Dictionary<int, (Character, Character)> pairs = new();
    private int nextPairKey = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Destroy duplicate instances on the same client
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("XXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX\nXXXXXXXXXX");
            return;
        }

        Instance = this;

        Debug.Log(IsServer ? "Server instance of PairManager is now ready." : "Clients instance of PairManager is now ready.");
    }

    #region PairManager Core
    /// <summary>
    /// This region contains the core functions of PairManager which are used to manage pairs.
    /// </summary>

    // Adds a player to the idle list. Called by Character on spawn.
    public void AddIdlePlayer(Character player)
    {
        if (!IsServer) return; // server-only
        if (!idlePlayers.Contains(player))
        {
            idlePlayers.Add(player);
            Debug.Log($"{player.name} added to idle players bringing the total idle player count to {idlePlayers.Count}.");
        }
    }
    // Removes a player from the idle list once paired.
    public void RemoveIdlePlayer(Character player)
    {
        if (!IsServer) return;
        if (idlePlayers.Contains(player))
        {
            idlePlayers.Remove(player);
            Debug.Log($"{player.name} removed from idle players bringing the total idle player count to {idlePlayers.Count}.");
        }
    }
    // Attempts to pair opponents together
    public void TryPairing()
    {
        if (!IsServer) return;

        while (idlePlayers.Count >= 2)
        {
            Character p1 = idlePlayers[0];
            Character p2 = idlePlayers[1];

            idlePlayers.RemoveRange(0, 2);

            int key = nextPairKey++;
            pairs[key] = (p1, p2);

            p1.SetOpponent(p2, key);
            p2.SetOpponent(p1, key);

            Debug.Log($"Paired {p1.name} with {p2.name} (Pair {key})");
        }
    }
    #endregion

    // Returns true if the number of idle players matches the expected number of players.
    public bool AllPlayersConnected(int expectedPlayers) => idlePlayers.Count >= expectedPlayers;

    // Returns all combatants currently paired for the next phase (server-only).
    public List<Character> GetCombatants()
    {
        List<Character> combatants = new();
        foreach (var pair in pairs.Values)
        {
            combatants.Add(pair.Item1);
            combatants.Add(pair.Item2);
        }
        return combatants;
    }
}
