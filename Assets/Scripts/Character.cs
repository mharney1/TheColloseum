using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Character : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 550;
    private int team = -1;
    private int pair = -1;
    private bool Dizzy = false;
    [SerializeField] private int health;
    private float exhaustion;
    private Choices currentChoice = Choices.None;
    private Character opponent = null;
    private CombatUI UI;
    private CharacterSliderUI sliders;
    private Movement movement;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health = maxHealth;
        exhaustion = 0;

        if (IsServer) SendPlayerNameClientRpc(name);
        if (!IsServer) RequestPlayerNameServerRPC();
        if (IsOwner)
        {
            Camera cam = GetComponentInChildren<Camera>(true);
            cam?.gameObject.SetActive(true);
        }        
        
        StartCoroutine(Setup());
    }
    private void OnDisable() { if (PhaseManager.Instance != null) PhaseManager.Instance.CurrentPhase.OnValueChanged -= HandlePhaseChanged; }
    private void HandlePhaseChanged(Phase previous, Phase current)
    {
        switch (current)
        {
            case Phase.Prepare:
                if(IsServer)movement.SetCycling(opponent != null);
                if (IsOwner) UI.ShowUI();
                if (isDizzy() && IsServer) SetChoiceServerRpc(Choices.None);
                break;
            case Phase.Action:
                if (IsServer) movement.SetCycling(false);
                if (IsOwner) UI.HideUI();
                SetDizzyClientRpc(false);
                
                break;
            case Phase.Resolve:
                break;
            case Phase.End:
                break;
        }
    }
    private IEnumerator Setup()
    {
        while (PairManager.Instance == null || PhaseManager.Instance == null) yield return null;
        PhaseManager.Instance.CurrentPhase.OnValueChanged += HandlePhaseChanged;

        while (sliders == null)
        {
            sliders = transform.Find("Player Sliders").GetComponent<CharacterSliderUI>();
            yield return null;
        }
        sliders.Setup(this);
        Debug.Log($"{name} found the sliders.");
        if (IsOwner)
        {
            while (UI == null)
            {
                UI = transform.Find("Scripts").GetComponent<CombatUI>();
                yield return null;
            }
            UI.Setup(this);
            Debug.Log($"{name} found the UI.");
        }
        if (IsServer)
        {
            while (movement == null)
            {
                movement = transform.Find("Scripts").GetComponent<Movement>();
                yield return null;
            }
            movement.Setup(this);
            Debug.Log($"{name} found the UI.");
            PairManager.Instance.AddIdlePlayer(this);
        }
    }

    public int GetHealth() => health;
    [ClientRpc]
    public void ModifyHealthClientRPC(int difference) => health = Mathf.Clamp(health + difference, 1, maxHealth);
    public int GetMaxHealth() => maxHealth;
    public float GetExhaustion() => exhaustion;
    [ClientRpc]
    public void ModifyExhaustionClientRPC(float difference) => exhaustion = Mathf.Clamp01(exhaustion + difference);

    public int GetTeam() => team;
    public void SetTeam(int newTeam) => team = newTeam;

    public int GetPair() => pair;
    public void SetPair(int newPair) => pair = newPair;

    public Choices GetChoice() => currentChoice;
    [ServerRpc (RequireOwnership = false)]
    public void SetChoiceServerRpc(Choices newChoice)
    {
        currentChoice = newChoice;
        PhaseManager.Instance.RemoveUndecided(this);
    }

    public Character GetOpponent() => opponent;
    [ClientRpc]
    public void SetOpponentClientRPC(ulong ownerId, int newPair)
    {
        opponent = null;
        SetPair(newPair);
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            Character character = netObj.GetComponent<Character>();
            if (character != null && character.OwnerClientId == ownerId) opponent = character;
        }
        if (opponent == null) Debug.Log($"{name} has no opponent assigned.");
    }

    public bool isDizzy() => Dizzy;
    [ClientRpc]
    public void SetDizzyClientRpc(bool state) => Dizzy = state;
    [ClientRpc]
    private void SendPlayerNameClientRpc(string newName) => gameObject.name = newName;
    [ServerRpc (RequireOwnership = false)]
    private void RequestPlayerNameServerRPC() => SendPlayerNameClientRpc(name);
}