using Runner.RL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    [SerializeField] RunnerAgent[] agents;
    [SerializeField] RunnerEnvironment environment;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject trainingMenuPanel;

    int selectedAgent;

    public void BeginTraining() {
        environment.SetAgent(agents[selectedAgent]);
        environment.BeginNewGame();
        mainMenuPanel.SetActive(false);
        trainingMenuPanel.SetActive(true);
    }

    public void StopTraining() {
        environment.StopTraining();
        mainMenuPanel.SetActive(true);
        trainingMenuPanel.SetActive(false);
    }

    public void ChangeAgent(int val) {
        selectedAgent = val;
    }
}
