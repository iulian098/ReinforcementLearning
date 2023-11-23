using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;

[System.Serializable]
public class InternalAgent : BaseAgent
{

    public List<float> rewardList;

    public float previousDistance;
    public bool finished;
    [SerializeField]bool useLastActionSetIfFinished;

    public PlayerController player;
    public Transform playerTransform;


    public override void SendParameters(EnvironmentParams env)
    {

        q_table = new float[env.state_size][];
        action = 0;
        for (int i = 0; i < env.state_size; i++)
        {
            q_table[i] = new float[env.action_size];
            for (int j = 0; j < env.action_size; j++)
            {
                q_table[i][j] = 0.0f;
            }
        }
    }

    /// <summary>
    /// Substract value from e
    /// </summary>
    public override void SetEValue(float val) {
        if (e - val > eMin)
            e = val;
    }

    /// <summary>
    /// Picks an action to take from its current state.
    /// </summary>
    /// <returns>The action choosen by the agent's policy</returns>
    public override float[] GetAction()
    {
        float maxValue = float.MinValue;
        int maxValueIndex = -1;

        for (int i = 0; i < q_table[lastState].Length; i++) {
            if (q_table[lastState][i] > maxValue) {
                maxValue = q_table[lastState][i];
                maxValueIndex = i;
            }
        }

        //action = q_table[lastState].ToList().IndexOf(q_table[lastState].Max());
        action = maxValueIndex;

        if(useLastActionSetIfFinished && finished && e <= eMin)
            return new float[1] { action };

        if (!loadData) {
            if (Random.Range(0f, 1f) < e) {
                action = Random.Range(0, 4);
            }

            if (e > eMin) {
                e = e - ((1f - eMin) / annealingSteps);
            }
        }

        return new float[1] { action };
    }

    /// <summary>
    /// Gets the values stored within the Q table.
    /// </summary>
    /// <returns>The average Q-values per state.</returns>
	public override float[] GetValue()
    {
        float[] value_table = new float[q_table.Length];
        for (int i = 0; i < q_table.Length; i++)
        {
            value_table[i] = q_table[i].Average();
        }
        return value_table;
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="reward">The reward recieved by the agent from the environment for it's action.</param>
    /// <param name="done">Whether the episode has ended</param>
    public override void SendState(List<float> state, bool done)
    {
        int nextState = Mathf.FloorToInt(state[0]);
        float nextStateMax = float.MinValue;

        if (useLastActionSetIfFinished && finished && e <= eMin) {
            lastState = nextState;
            return;
        }


        foreach (var item in q_table[nextState])
            if(item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData)
        {
            if (done == true)
                q_table[lastState][action] += learning_rate * (reward - q_table[lastState][action]);
            else
                q_table[lastState][action] += learning_rate * (reward + gamma * nextStateMax - q_table[lastState][action]);
        }
        lastState = nextState;
    }

    public void SendState(float state, bool done) {

        int nextState = Mathf.FloorToInt(state);
        float nextStateMax = float.MinValue;
        if (useLastActionSetIfFinished && finished) {
            lastState = nextState;
            return;
        }

        foreach (var item in q_table[nextState])
            if (item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData) {
            if (done == true)
                q_table[lastState][action] += learning_rate * (reward - q_table[lastState][action]);
            else
                q_table[lastState][action] += learning_rate * (reward + gamma * nextStateMax - q_table[lastState][action]);
        }
        lastState = nextState;
    }

    /// <summary>
    /// Reset agent data
    /// </summary>
    public override void ResetAgent() {
        rewardList.Add(episodeReward);
        reward = 0;
        currentStep = 0;
        done = false;
        player.SendInput();
    }

    /// <summary>
    /// Save data to text
    /// </summary>
    public void SaveData(int d)
    {
        float val;
        string text = "";
        for(int i = 0; i < q_table.Length; i++)
        {
            for(int j =0; j < q_table[i].Length; j++)
            {
                val = q_table[i][j];
                if (j != q_table[i].Length - 1)
                    text += val.ToString() + "|";
                else
                    text += val.ToString();
            }
            text += "\n";
        }

        File.WriteAllText("Data/Data" + d + ".txt", text);
        Debug.Log("<color=yellow>Data Saved</color>");
    }

    /// <summary>
    /// Load data
    /// </summary>
    /// <param name="nr"></param>
    /// <returns></returns>
    public float[][] LoadData(int nr) {
        float[][] loadedQTable = q_table;
        try {
            string[] vals = File.ReadAllLines("Data/Data" + nr + ".txt");
            for (int i = 0; i < vals.Length; i++) {
                string[] newVal = vals[i].Split('|');
                for (int j = 0; j < newVal.Length; j++) {
                    loadedQTable[i][j] = float.Parse(newVal[j]);
                }
            }
        }
        catch (IOException e) {
            Debug.LogError($"Data cannot be loaded. Exception : {e.Message}");
            return loadedQTable;
        }
        Debug.Log("<color=yellow>Data Loaded</color>");
        return loadedQTable;
    }

    public override void SetAnnealingSteps(int val)
    {
        annealingSteps = val;
    }
}
