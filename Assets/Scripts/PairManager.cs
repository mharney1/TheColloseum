using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PairManager : NetworkBehaviour
{
    public static PairManager Instance { get; private set; }
    private readonly List<Character> idlePlayers = new();
    private readonly Dictionary<int, (Character, Character)> pairs = new();
    public int nextKey = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public bool AllPlayersConnected(int expectedPlayers) => idlePlayers.Count >= expectedPlayers;
    public void AddIdlePlayer(Character player) { if (IsServer && !idlePlayers.Contains(player)) idlePlayers.Add(player); }
    public void RemoveIdlePlayer(Character player) { if (IsServer && idlePlayers.Contains(player)) idlePlayers.Remove(player); }
    public int GetIdlePlayers() => idlePlayers.Count;
    public Dictionary<int, (Character, Character)> GetAllPairs() => pairs;
    public void TryPairing()
    {
        while (idlePlayers.Count >= 2)
        {
            bool pairMade = false;
            for (int i = 0; i < idlePlayers.Count - 1; i++)
            {
                Character player1 = idlePlayers[i];
                for (int j = i + 1; j < idlePlayers.Count; j++)
                {
                    Character player2 = idlePlayers[j];
                    if (player1.GetTeam() == -1 || player1.GetTeam() != player2.GetTeam())
                    {
                        int key = nextKey++;
                        pairs[key] = (player1, player2);
                        player1.SetOpponentClientRPC(player2.OwnerClientId, key);
                        player2.SetOpponentClientRPC(player1.OwnerClientId, key);
                        RemoveIdlePlayer(player1);
                        RemoveIdlePlayer(player2);
                        pairMade = true;
                        break;
                    }
                }
                if (pairMade) { i = -1; break; }
            }
            if (!pairMade) break;
        }
    }
    public void removePair(int key)
    {
        (Character c1, Character c2) = pairs[key];

        foreach (Character player in new[] { c1, c2 })
        {
            player.SetOpponentClientRPC(111, -1);

            if (player.GetHealth() != 1)
            {
                AddIdlePlayer(player);
            }
        }
        pairs.Remove(key);
    }
    public void GetCombatants()
    {
        foreach (var pair in pairs.Values)
        {
            PhaseManager.Instance.AddUndecided(pair.Item1);
            PhaseManager.Instance.AddUndecided(pair.Item2);
        }
    }
}