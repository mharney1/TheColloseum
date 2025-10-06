using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Character : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 550;
    [SerializeField] private int minHealth = 2;
    private int team = -1;
    [SerializeField]private int pair = -1;
    private bool dizzy = false;
    [SerializeField] private int health;
    private float exhaustion;
    private Choices currentChoice = Choices.None;
    [SerializeField] private List<Choices> choices;
    private Character opponent = null;
    private CombatUI UI;
    private CharacterSliderUI sliders;
    private Movement movement;
    [SerializeField]private bool defeated = false;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health = maxHealth;
        exhaustion = 0;

        RequestPlayerNameServerRPC();
        if (IsOwner)
        {
            Camera cam = GetComponentInChildren<Camera>(true);
            cam?.gameObject.SetActive(true);
        }

        StartCoroutine(Setup());
    }
    private void Update()
    {
        if (IsServer && health == minHealth && pair != -1 && defeated) Defeated();
        else if (health == minHealth && !defeated) defeated = true;
    }
    private void OnDisable() { if (PhaseManager.Instance != null) PhaseManager.Instance.CurrentPhase.OnValueChanged -= HandlePhaseChanged; }
    private void HandlePhaseChanged(Phase previous, Phase current)
    {
        switch (current)
        {
            case Phase.Prepare:
                if (IsServer) movement.SetCycling(opponent != null);
                if (IsOwner) UI.ShowUI();
                if (isDizzy() && IsServer) SetChoiceServerRpc(Choices.None);
                break;
            case Phase.Action:
                if (IsServer) movement.SetCycling(false);
                if (IsOwner) UI.HideUI();
                break;
            case Phase.Resolve:
                StoreChoice();
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
        if (IsOwner)
        {
            while (UI == null)
            {
                UI = transform.Find("Scripts").GetComponent<CombatUI>();
                yield return null;
            }
            UI.Setup(this);
        }
        if (IsServer)
        {
            while (movement == null)
            {
                movement = transform.Find("Scripts").GetComponent<Movement>();
                yield return null;
            }
            movement.Setup(this);
            PairManager.Instance.AddIdlePlayer(this);
        }
    }

    public int GetHealth() => health;
    [ClientRpc]
    public void ModifyHealthClientRPC(int difference) => health = Mathf.Clamp(health + difference, minHealth, maxHealth);
    public int GetMaxHealth() => maxHealth;
    public float GetExhaustion() => exhaustion;
    [ClientRpc]
    public void ModifyExhaustionClientRPC(float difference) => exhaustion = Mathf.Clamp01(exhaustion + difference);

    public int GetTeam() => team;
    public void SetTeam(int newTeam) => team = newTeam;

    public int GetPair() => pair;
    public void SetPair(int newPair) => pair = newPair;

    public Choices GetChoice() => currentChoice;
    [ServerRpc(RequireOwnership = false)]
    public void SetChoiceServerRpc(Choices newChoice)
    {
        currentChoice = newChoice;
        PhaseManager.Instance.RemoveUndecided(this);
    }
    public void StoreChoice()
    {
        if (currentChoice != Choices.None)
        {
            if (choices.Count == 3) choices.RemoveAt(0);
            choices.Add(currentChoice);
        }
        else if (isDizzy()) dizzy = false;
        currentChoice = Choices.None;

    }
    public List<Choices> GetChoices() => choices;
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
    }
    [ClientRpc]
    public void ClearOpponentClientRpc()
    {
        opponent = null;
        pair = -1;
    }
    public bool isDizzy() => dizzy;
    [ClientRpc]
    public void SetDizzyClientRpc(bool state) => dizzy = state;
    [ClientRpc]
    private void SendPlayerNameClientRpc(string newName) => gameObject.name = newName;
    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerNameServerRPC() => SendPlayerNameClientRpc(name);
    public void Defeated()
    {
        PairManager.Instance.RemovePair(pair);
    }
    public bool GetDefeated() => defeated;
}