using System.Collections.Generic;
using UnityEngine;

public abstract class AgentBase : MonoBehaviour {
	[SerializeField] protected PlayerController player;
	[SerializeField] protected bool useLastActionSetIfFinished;
	[SerializeField] protected Transform playerTransform;

	public bool loadData;
	public int dataNr;

	public bool acceptingSteps;
	public int annealingSteps = 2000; // Number of steps to lower e to eMin.
	public bool done;
	public int currentStep;
	public float reward;
	public float episodeReward;

	protected float learning_rate = 0.05f; //Default: 0.5f
	protected float gamma = 0.99f; //Defualt = 0.99f
	protected float eMin = 0.1f; // Lower bound of epsilon.
	protected float e = 1;  // Initial epsilon value for random action selection.
	protected int action = -1;
	protected int lastState;
	protected bool finished;


	public float E => e;

	public virtual void Init(EnvironmentParams env, DefaultAgent agentSettings) { }

	public virtual object GetAction() {
		return 0.0f;
	}

	public virtual object GetValue() {
		return 0.0f;
	}

	public virtual void SendState (Vector2Int state, bool done){}

	public virtual void SetAnnealingSteps(int val) { }

	public virtual void SetEValue(float val) { }

	public virtual void ResetAgent() { }

	protected string VectorToString(Vector2Int vect) {
		return vect.x + "|" + vect.y;
    }
}