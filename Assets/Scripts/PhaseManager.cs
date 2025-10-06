using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PhaseManager : NetworkBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [SerializeField] private int expectedPlayers = 2;
    [SerializeField] private float startTime = 15f;
    [SerializeField] private float shortenedTime = 1f;
    [SerializeField] private TextMeshProUGUI prepareTimerText;
    [SerializeField] private GameObject combatManagerPrefab;
    private readonly List<Character> undecidedCharacters = new();
    private bool winCondition = false;
    private NetworkVariable<float> timer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Phase> CurrentPhase = new(Phase.Load, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null && Instance != this) Destroy(gameObject);
        timer.OnValueChanged += (oldVal, newVal) => {prepareTimerText.text = Mathf.CeilToInt(newVal).ToString("00");};
        Instance = this;
        if (IsServer) StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        yield return LoadPhase();
        while (!winCondition)
        {
            yield return PreparePhase();
            yield return ActionPhase();
            yield return ResolvePhase();
        }
        yield return EndPhase();
    }

    private IEnumerator LoadPhase()
    {
        Debug.Log("Phase: Load phase has begun.");
        CurrentPhase.Value = Phase.Load;
        Instantiate(combatManagerPrefab);
        while (PairManager.Instance == null) yield return new WaitForSeconds(1f);
        while (!PairManager.Instance.AllPlayersConnected(expectedPlayers)) yield return new WaitForSeconds(3f);
        PairManager.Instance.TryPairing();
        while (CombatManager.Instance == null) yield return new WaitForSeconds(1f);
    }
    private IEnumerator PreparePhase()
    {
        Debug.Log("Phase: Prepare phase has begun.");
        PairManager.Instance.GetCombatants();
        CurrentPhase.Value = Phase.Prepare;
        timer.Value = startTime;
        SetPrepareTimerActiveClientRpc(true);
        while (timer.Value > 0f)
        {
            if (undecidedCharacters.Count == 0 && timer.Value > shortenedTime) timer.Value = shortenedTime;
            timer.Value = Mathf.Clamp(timer.Value - Time.deltaTime, 0f, startTime);
            yield return null;
        }
        DefaultChoice(undecidedCharacters);
        SetPrepareTimerActiveClientRpc(false);
    }
    private IEnumerator ActionPhase()
    {
        Debug.Log("Phase: Action phase has begun.");
        CurrentPhase.Value = Phase.Action;
        CombatManager.Instance.ResolveAllPairs();
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator ResolvePhase()
    {
        Debug.Log("Phase: Resolve phase has begun.");
        CurrentPhase.Value = Phase.Resolve;
        PairManager.Instance.TryPairing();

        //if theres are no active players we need to find the winner
        if (PairManager.Instance.GetPairCount() == 0)
        {
            //if theres more than 1 idle player need to confirm that all players are on the same team
            if (PairManager.Instance.GetIdlePlayerCount() > 1)
            {
                bool opponentsFound = false;
                List<Character> idlePlayers = PairManager.Instance.GetIdlePlayers();
                if (idlePlayers[0].GetTeam() != -1)
                {
                    for (int i = 0; i < idlePlayers.Count - 1 && !opponentsFound; i++)
                    {
                        for (int j = i + 1; j < idlePlayers.Count && !opponentsFound; j++)
                        {
                            if (idlePlayers[i].GetTeam() != idlePlayers[j].GetTeam())
                            {
                                opponentsFound = true;
                            }
                        }
                    }
                    if (!opponentsFound) winCondition = true;
                }
            }
            else winCondition = true;
            if (!winCondition) PairManager.Instance.TryPairing();
        }
        yield return null;
    }
    private IEnumerator EndPhase()
    {
        Debug.Log("Phase: End phase has begun.");
        CurrentPhase.Value = Phase.End;
        yield return new WaitForSeconds(1f);
    }

    private void DefaultChoice(List<Character> characters){
        foreach (Character character in new List<Character>(characters)) character.SetChoiceServerRpc(Choices.Attack);
    }
    public void AddUndecided (Character player){
        if (!undecidedCharacters.Contains(player)) undecidedCharacters.Add(player);
    }
    public void RemoveUndecided (Character player){
        if (undecidedCharacters.Contains(player)) undecidedCharacters.Remove(player);
    }

    [ClientRpc]
    private void SetPrepareTimerActiveClientRpc (bool isActive){prepareTimerText?.transform.parent.gameObject.SetActive(isActive);}
}