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
        // Keep trying to form pairs while there are at least 2 idle players
        while (idlePlayers.Count >= 2)
        {
            bool pairMade = false;

            // Outer loop: iterate over idle players
            for (int i = 0; i < idlePlayers.Count - 1; i++)
            {
                Character player1 = idlePlayers[i];

                // Inner loop: try to find a valid partner
                for (int j = i + 1; j < idlePlayers.Count; j++)
                {
                    Character player2 = idlePlayers[j];

                    bool sameTeam = (player1.GetTeam() != -1 && player1.GetTeam() == player2.GetTeam());

                    if (!sameTeam)
                    {
                        // Form the pair
                        int key = nextPairKey++;
                        pairs[key] = (player1, player2);

                        player1.SetOpponent(player2, key);
                        player2.SetOpponent(player1, key);

                        // Remove both from idle list
                        idlePlayers.Remove(player1);
                        idlePlayers.Remove(player2);

                        Debug.Log($"A pair has been created between {player1.name} and {player2.name} with the pair key of {key}");

                        pairMade = true;
                        break; // exit inner loop to restart outer loop
                    }
                }

                if (pairMade)
                {
                    i = -1; // reset outer loop to start from index 0
                    break;
                }
            }

            // If no pair was made during the full iteration, break the while loop
            if (!pairMade)
                break;
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
    public int GetIdlePlayers() => idlePlayers.Count;
}
