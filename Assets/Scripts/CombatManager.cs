using System.Collections;
using System.Collections.Generic;
using Unity.Services.Multiplay.Authoring.Core.MultiplayApi;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat Multipliers")]
    public float maxBlockMultiplier = 0.75f;
    public float maxCounterMultiplier = 1f;
    public float exposedMultiplier = 1.25f;
    public float blockExhaustionMultiplier = 0.5f;
    public float counterExhaustionMultiplier = 0.3f;
    public float exStep = 0.25f;
    public int restHealAmount = 75;

    private void Awake () {Instance = this;}
    public void ResolveAllPairs()
    {
        foreach (var pair in PairManager.Instance.GetAllPairs())
        {
            Character p1 = pair.Value.Item1;
            Character p2 = pair.Value.Item2;
            HandleCombatOutcome(p1, p1.GetChoice(), p2, p2.GetChoice());
        }
    }
    private void HandleCombatOutcome(Character p1, Choices c1, Character p2, Choices c2)
    {
        float baseAttack = 100f;
        float p1Damage = 0f;
        float p2Damage = 0f;

        switch (c1)
        {
            case Choices.Attack:
                switch (c2)
                {
                    case Choices.Attack:
                        p1Damage = baseAttack;
                        p2Damage = baseAttack;
                        p1.ModifyExhaustionClientRPC(-exStep);
                        p2.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Block:
                        p2Damage = baseAttack * (1 - (maxBlockMultiplier - blockExhaustionMultiplier * p2.GetExhaustion()));
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Counter:
                        p1Damage = baseAttack * exposedMultiplier;
                        p2Damage = baseAttack * (1 - (maxCounterMultiplier - counterExhaustionMultiplier * p2.GetExhaustion()));
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Rest:
                        p2Damage = baseAttack * exposedMultiplier;
                        p1.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.None:
                        p2Damage = baseAttack * exposedMultiplier;
                        p1.ModifyExhaustionClientRPC(-exStep);
                        break;
                }
                break;
            case Choices.Block:
                switch (c2)
                {
                    case Choices.Attack:
                        p1Damage = baseAttack * (1 - (maxBlockMultiplier - blockExhaustionMultiplier * p1.GetExhaustion()));
                        p1.ModifyExhaustionClientRPC(-exStep);
                        p2.ModifyExhaustionClientRPC(exStep);
                        break;
                    case Choices.Block:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(exStep);
                        break;
                    case Choices.Counter:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(exStep);
                        p2.SetDizzyClientRpc(true);
                        break;
                    case Choices.Rest:
                        p1.ModifyExhaustionClientRPC(exStep);
                        Rest(p2);
                        break;
                    case Choices.None:
                        p1.ModifyExhaustionClientRPC(exStep);
                        break;
                }
                break;
            case Choices.Counter:
                switch (c2)
                {
                    case Choices.Attack:
                        p2Damage = baseAttack * exposedMultiplier;
                        p1Damage = baseAttack * (1 - (maxCounterMultiplier - counterExhaustionMultiplier * p1.GetExhaustion()));
                        p2.ModifyExhaustionClientRPC(exStep);
                        p1.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Block:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(exStep);
                        p1.SetDizzyClientRpc(true);
                        break;
                    case Choices.Counter:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p2.ModifyExhaustionClientRPC(exStep);
                        p1.SetDizzyClientRpc(true);
                        p2.SetDizzyClientRpc(true);
                        break;
                    case Choices.Rest:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p1.SetDizzyClientRpc(true);
                        Rest(p2);
                        break;
                    case Choices.None:
                        p1.ModifyExhaustionClientRPC(exStep);
                        p1.SetDizzyClientRpc(true);
                        break;
                }
                break;
            case Choices.Rest:
                switch (c2)
                {
                    case Choices.Attack:
                        p1Damage = baseAttack * exposedMultiplier;
                        p2.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Block:
                        Rest(p1);
                        p2.ModifyExhaustionClientRPC(exStep);
                        break;
                    case Choices.Counter:
                        Rest(p1);
                        p2.ModifyExhaustionClientRPC(exStep);
                        p2.SetDizzyClientRpc(true);
                        break;
                    case Choices.Rest:
                        Rest(p1);
                        Rest(p2);
                        break;
                    case Choices.None:
                        Rest(p1);
                        break;
                }
                break;
            case Choices.None:
                switch (c2)
                {
                    case Choices.Attack:
                        p1Damage = baseAttack * exposedMultiplier;
                        p2.ModifyExhaustionClientRPC(-exStep);
                        break;
                    case Choices.Block:
                        p2.ModifyExhaustionClientRPC(exStep);
                        break;
                    case Choices.Counter:
                        p2.ModifyExhaustionClientRPC(exStep);
                        p2.SetDizzyClientRpc(true);
                        break;
                    case Choices.Rest:
                        Rest(p2);
                        break;
                    case Choices.None:
                        break;
                }
                break;
        }
        if (p1Damage > 0) p1.ModifyHealthClientRPC(-Mathf.RoundToInt(p1Damage));
        if (p2Damage > 0) p2.ModifyHealthClientRPC(-Mathf.RoundToInt(p2Damage));
        Debug.Log($"{p1.name} now has {p1.GetHealth()} Health and {p1.GetExhaustion()} exhaustion.\n{p2.name} now has {p2.GetHealth()} Health and {p2.GetExhaustion()} exhaustion.");
    }
    private void Rest(Character c)
    {
        c.ModifyExhaustionClientRPC(-c.GetExhaustion());
        c.ModifyHealthClientRPC(restHealAmount);
    }
}