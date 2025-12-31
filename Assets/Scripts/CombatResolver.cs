using System;
using UnityEngine;

public static class CombatResolver
{
	private static float S_MAX_BLOCK_MULTIPLIER = 0.75f;
	private static float S_BLOCK_MODIFIER = 0.5f;
	private static float S_MAX_COUNTER_MULTIPLIER = 1f;
	private static float S_COUNTER_MODIFIER = 0.3f;
	private static float S_EXPOSED_MULTIPLIER = 1.25f;
	private static float S_EX_STEP = 0.25f;
	private static float S_BASE_ATTACK = 100;
	private static float S_BASE_HEAL = 75;
	private static CombatOutcome [,] S_MATRIX;

	public static void Initialize(
		)
	{
		int count = Enum.GetValues( typeof( Choices ) ).Length;
		S_MATRIX = new CombatOutcome [ count, count ];

		BuildMatrix();
	}
	private static void BuildMatrix()
	{
		S_MATRIX [ (int) Choices.None, (int) Choices.None ] = new CombatOutcome
		{
			exhaustionDeltaP1 = -S_EX_STEP,
			exhaustionDeltaP2 = -S_EX_STEP
		};
		S_MATRIX [ (int) Choices.None, (int) Choices.Attack ] = new CombatOutcome
		{
			healthDeltaP1 = -S_BASE_ATTACK,
			exhaustionDeltaP2 = -S_EX_STEP,
			multiplierP1 = DmgMultiplier.Exposed
		};
		S_MATRIX [ (int) Choices.None, (int) Choices.Block ] = new CombatOutcome
		{
			exhaustionDeltaP1 = -S_EX_STEP,
			exhaustionDeltaP2 = S_EX_STEP,
			multiplierP2 = DmgMultiplier.Block
		};
		S_MATRIX [ (int) Choices.None, (int) Choices.Counter ] = new CombatOutcome
		{
			exhaustionDeltaP1 = -S_EX_STEP,
			exhaustionDeltaP2 = S_EX_STEP,
			multiplierP2 = DmgMultiplier.Counter,
			dizzyP2 = true
		};
		S_MATRIX [ (int) Choices.None, (int) Choices.Rest ] = new CombatOutcome
		{
			healthDeltaP2 = S_BASE_HEAL,
			exhaustionDeltaP1 = -S_EX_STEP,
			exhaustionDeltaP2 = (-S_EX_STEP) * 4
		};
		S_MATRIX [ (int) Choices.Attack, (int) Choices.Attack ] = new CombatOutcome
		{
			healthDeltaP1 = -S_BASE_ATTACK,
			healthDeltaP2 = -S_BASE_ATTACK,
			exhaustionDeltaP1 = -S_EX_STEP,
			exhaustionDeltaP2 = -S_EX_STEP,
		};
		S_MATRIX [ (int) Choices.Attack, (int) Choices.Block ] = new CombatOutcome
		{
			healthDeltaP2 = -S_BASE_ATTACK,
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = -S_EX_STEP,
			multiplierP2 = DmgMultiplier.Block
		};
		S_MATRIX [ (int) Choices.Attack, (int) Choices.Counter ] = new CombatOutcome
		{
			healthDeltaP1 = -S_BASE_ATTACK,
			healthDeltaP2 = -S_BASE_ATTACK,
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = -S_EX_STEP,
			multiplierP1 = DmgMultiplier.Exposed,
			multiplierP2 = DmgMultiplier.Counter
		};
		S_MATRIX [ (int) Choices.Attack, (int) Choices.Rest ] = new CombatOutcome
		{
			healthDeltaP2 = -S_BASE_ATTACK,
			exhaustionDeltaP1 = -S_EX_STEP,
			multiplierP2 = DmgMultiplier.Exposed
		};
		S_MATRIX [ (int) Choices.Block, (int) Choices.Block ] = new CombatOutcome
		{
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = S_EX_STEP,
			multiplierP1 = DmgMultiplier.Block,
			multiplierP2 = DmgMultiplier.Block
		};
		S_MATRIX [ (int) Choices.Block, (int) Choices.Counter ] = new CombatOutcome
		{
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = S_EX_STEP,
			multiplierP1 = DmgMultiplier.Block,
			multiplierP2 = DmgMultiplier.Counter,
			dizzyP2 = true
		};
		S_MATRIX [ (int) Choices.Block, (int) Choices.Rest ] = new CombatOutcome
		{
			healthDeltaP2 = S_BASE_HEAL,
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = (-S_EX_STEP) * 4,
			multiplierP1 = DmgMultiplier.Block
		};
		S_MATRIX [ (int) Choices.Counter, (int) Choices.Counter ] = new CombatOutcome
		{
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = S_EX_STEP,
			multiplierP1 = DmgMultiplier.Counter,
			multiplierP2 = DmgMultiplier.Counter,
			dizzyP1 = true,
			dizzyP2 = true
		};
		S_MATRIX [ (int) Choices.Counter, (int) Choices.Rest ] = new CombatOutcome
		{
			healthDeltaP2 = S_BASE_HEAL,
			exhaustionDeltaP1 = S_EX_STEP,
			exhaustionDeltaP2 = (-S_EX_STEP) * 4,
			multiplierP1 = DmgMultiplier.Counter,
			dizzyP1 = true
		};
		S_MATRIX [ (int) Choices.Rest, (int) Choices.Rest ] = new CombatOutcome
		{
			healthDeltaP1 = S_BASE_HEAL,
			healthDeltaP2 = S_BASE_HEAL,
			exhaustionDeltaP1 = (-S_EX_STEP) * 4,
			exhaustionDeltaP2 = (-S_EX_STEP) * 4
		};
	}
	public static CombatOutcome Resolve(
		Choices c1,
		Choices c2,
		float exhaustionP1,
		float exhaustionP2)
	{
		bool swapped = false;

		if ((int) c1 > (int) c2)
		{
			(c1, c2) = (c2, c1);
			swapped = true;
		}

		CombatOutcome outcome = S_MATRIX [ (int) c1, (int) c2 ];

		if (swapped)
		{
			outcome = Mirror( outcome );
		}

		outcome = ApplyModifiers( outcome, exhaustionP1, exhaustionP2 );

		return outcome;
	}
	private static CombatOutcome Mirror(CombatOutcome outcome)
	{
		return new CombatOutcome
		{
			healthDeltaP1 = outcome.healthDeltaP2,
			healthDeltaP2 = outcome.healthDeltaP1,

			exhaustionDeltaP1 = outcome.exhaustionDeltaP2,
			exhaustionDeltaP2 = outcome.exhaustionDeltaP1,

			multiplierP1 = outcome.multiplierP2,
			multiplierP2 = outcome.multiplierP1,

			dizzyP1 = outcome.dizzyP2,
			dizzyP2 = outcome.dizzyP1
		};
	}
	private static CombatOutcome ApplyModifiers(CombatOutcome outcome, float exhaustionP1, float exhaustionP2)
	{
		float healthDeltaP1 = outcome.healthDeltaP1;
		float healthDeltaP2 = outcome.healthDeltaP2;

		if (outcome.multiplierP1 == DmgMultiplier.Exposed)
		{
			healthDeltaP1 *= S_EXPOSED_MULTIPLIER;
		}
		if (outcome.multiplierP1 == DmgMultiplier.Counter)
		{
			healthDeltaP1 *= (1 - (S_MAX_COUNTER_MULTIPLIER - S_COUNTER_MODIFIER * exhaustionP1));
		}
		if (outcome.multiplierP1 == DmgMultiplier.Block)
		{
			healthDeltaP1 *= (1 - (S_MAX_BLOCK_MULTIPLIER - S_BLOCK_MODIFIER * exhaustionP1));
		}

		if (outcome.multiplierP2 == DmgMultiplier.Exposed)
		{
			healthDeltaP2 *= S_EXPOSED_MULTIPLIER;
		}
		if (outcome.multiplierP2 == DmgMultiplier.Counter)
		{
			healthDeltaP2 *= (1 - (S_MAX_COUNTER_MULTIPLIER - S_COUNTER_MODIFIER * exhaustionP2));
		}
		if (outcome.multiplierP2 == DmgMultiplier.Block)
		{
			healthDeltaP2 *= (1 - (S_MAX_BLOCK_MULTIPLIER - S_BLOCK_MODIFIER * exhaustionP2));
		}

		return new CombatOutcome
		{
			healthDeltaP1 = healthDeltaP1,
			healthDeltaP2 = healthDeltaP2,

			exhaustionDeltaP1 = outcome.exhaustionDeltaP1,
			exhaustionDeltaP2 = outcome.exhaustionDeltaP2,

			dizzyP1 = outcome.dizzyP1,
			dizzyP2 = outcome.dizzyP2,
		};
	}

#if UNITY_EDITOR
	[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded )]
	private static void RunAllDebugTests()
	{
		Initialize();
		ChoiceOrderTest();
		CaseCoverageTest();
		CaseSymmetryTest();

		Debug.Log( "CombatResolver debug tests complete." );
	}

	private static void ChoiceOrderTest()
	{
		Debug.Assert( (int) Choices.None == 0, "Choices.None changed from 0" );
		Debug.Assert( (int) Choices.Attack == 1, "Choices.Attack changed from 1" );
		Debug.Assert( (int) Choices.Block == 2, "Choices.Block changed from 2" );
		Debug.Assert( (int) Choices.Counter == 3, "Choices.Counter changed from 3" );
		Debug.Assert( (int) Choices.Rest == 4, "Choices.Rest changed from 4" );

		Debug.Log( "CombatResolver choice order test complete." );
	}

	private static void CaseCoverageTest()
	{
		foreach (Choices a in Enum.GetValues( typeof( Choices ) ))
			foreach (Choices b in Enum.GetValues( typeof( Choices ) ))
			{
				Choices c1 = a;
				Choices c2 = b;

				if ((int) c1 > (int) c2)
					(c1, c2) = (c2, c1);
				Debug.Assert( !S_MATRIX [ (int) c1, (int) c2 ].Equals( default( CombatOutcome ) ),
					$"Combat matrix missing canonical entry for {c1} vs {c2}" );
			}
		Debug.Log( "CombatResolver case coverage test complete." );
	}
	private static void CaseSymmetryTest()
	{
		float e1 = 0.5f;
		float e2 = 1f;

		foreach (Choices c1 in Enum.GetValues( typeof( Choices ) ))
			foreach (Choices c2 in Enum.GetValues( typeof( Choices ) ))
			{

				CombatOutcome a = Resolve( c1, c2, e1, e2 );
				CombatOutcome b = Resolve( c2, c1, e2, e1 );

				Debug.Assert( Mathf.Approximately( a.healthDeltaP1, b.healthDeltaP2 ),
					$"Health P1 mismatch: {(int) c1} {c1} vs {(int) c2}{c2}" );

				Debug.Assert( Mathf.Approximately( b.healthDeltaP1, a.healthDeltaP2 ),
					$"Health P2 mismatch: {(int) c1} {c1} vs {(int) c2} {c2}" );

				Debug.Assert( Mathf.Approximately( a.exhaustionDeltaP1, b.exhaustionDeltaP2 ),
					$"Exhaustion P1 mismatch: {(int) c1} {c1} vs {(int) c2} {c2}" );

				Debug.Assert( Mathf.Approximately( b.exhaustionDeltaP1, a.exhaustionDeltaP2 ),
					$"Exhaustion P2 mismatch: {(int) c1} {c1} vs {(int) c2} {c2}" );

				Debug.Assert( a.dizzyP1 == b.dizzyP2,
					$"Dizzy P1 mismatch: {(int) c1} {c1} vs {(int) c2} {c2}" );

				Debug.Assert( b.dizzyP1 == a.dizzyP2,
					$"Dizzy P2 mismatch: {(int) c1} {c1} vs {(int) c2} {c2}" );
			}
		Debug.Log( "CombatResolver case symmetry test complete." );
	}
#endif
}
