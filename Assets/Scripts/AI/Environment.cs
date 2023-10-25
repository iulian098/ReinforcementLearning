using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnvironmentParams
{
	public int state_size;
	public int action_size;
	public List<string> action_descriptions;
	public int num_agents;
	public Vector2Int[] grid;
}

[System.Serializable]
public class DefaultAgent {
	public float learningRate;
	public float gamma;
	public float eMin;
}

public class Environment : MonoBehaviour
{

	public bool done;
	public int maxSteps;
	public bool begun;
	public bool acceptingSteps;

	public int numberOfAgents;
	public DefaultAgent agentSettings;
	public List<InternalAgent> agentsList;
	public List<PlayerController> visualAgentsList;

	public int frameToSkip;
	public int framesSinceAction;
	public bool skippingFrames;
	public float[] actions;
	public float waitTime;
	public int episodeCount;

	public List<float> actionListTest;

	public EnvironmentParams envParameters;

	public virtual void SetUp()
    {
		envParameters = new EnvironmentParams()
		{
			state_size = 0,
			action_descriptions = new List<string>(),
			action_size = 0,
			num_agents = 1
		};

		begun = false;
		acceptingSteps = true;
    }

	public virtual List<float> collectState(InternalAgent agent)
    {
		List<float> state = new List<float>();
		return state;
    }

	public virtual void Step(InternalAgent agent, bool isLast)
    {
		if(isLast)
			acceptingSteps = false;
		agent.currentStep += 1;
		
		if(agent.currentStep >= maxSteps-1)
        {
			agent.done = true;
			agent.player.SendInput();
			//return;
        }

		agent.reward = 0;
		actions = agent.GetAction();
		framesSinceAction = 0;

		int sendAction = Mathf.FloorToInt(actions[0]);
		MiddleStep(sendAction, agent);

		StartCoroutine(WaitStep());
    }

	public void SetEValue() {
		foreach (InternalAgent agent in agentsList)
			agent.SetEValue(0.1f);
    }
	public virtual void MiddleStep(int action, InternalAgent agent) { }

	public virtual void MiddleStep(float[] action) { }

	public IEnumerator WaitStep()
	{
		yield return new WaitForSeconds(waitTime);
        foreach (InternalAgent a in agentsList) {
			EndStep(a);
        }
	}

	public virtual void EndStep(InternalAgent agent)
	{
		agent.SendState(collectState(agent), done);
		skippingFrames = false;
		acceptingSteps = true;
	}

	public virtual void Reset(InternalAgent agent)
	{
		agent.reward = 0;
		agent.currentStep = 0;
		episodeCount++;
		agent.done = false;
		acceptingSteps = false;
	}

	public virtual void Reset(bool firstTime = false) {
		//Debug.LogError("Reset");
        /*foreach (InternalAgent agent in agentsList) {
			agent.reward = 0;
			agent.currentStep = 0;
			agent.done = false;
        }*/
		acceptingSteps = true;
		episodeCount++;
	}

	public virtual void EndReset(InternalAgent agent)
	{
		agent.SendState(collectState(agent), done);
		skippingFrames = false;
		acceptingSteps = true;
		begun = true;
		framesSinceAction = 0;
	}

	public virtual void Run(InternalAgent agent, bool isLast = false)
	{
		if (acceptingSteps == true)
		{
			if (agent.done == false)
				Step(agent, isLast);
			else
				Reset();
		}
	}

}
