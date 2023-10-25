using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{

	public bool loadData;
	public int dataNr;

	public float[][] q_table;// The matrix containing the values estimates.
	public float learning_rate = 0.05f; //Default: 0.5f
	public float gamma = 0.99f; //Defualt = 0.99f
	public float eMin = 0.1f; //Lower bound of epsilon.
	public int annealingSteps = 2000; //Number of steps to lower e to eMin.
	public bool done;
	public int currentStep;
	public float reward;
	public float episodeReward;

	protected int action = -1;
	protected float e = 1;  //Initial epsilon value for random action selection.
	protected int lastState;

	public float E => e;

	public virtual void SendParameters(EnvironmentParams env) { }

	public virtual string Receive()
	{
		return "";
	}

	public virtual float[] GetAction()
	{
		return new float[1] { 0.0f };
	}

	public virtual float[] GetValue()
	{
		float[] value = new float[1];
		return value;
	}

	public virtual void SendState(List<float> state, float reward, bool done) { }
	public virtual void SendState(List<float> state, bool done) { }

	public virtual void SetAnnealingSteps(int val) { }

	public virtual void SetEValue(float val) { }

	public virtual void ResetAgent() { }
}