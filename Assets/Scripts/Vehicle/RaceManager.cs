using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoSingleton<RaceManager> {

    const int STARTING_TIME = 3;

    public enum State {
        Init,
        Starting,
        Playing,
        End,
        Learning = 999
    }

    public bool enableLearning;

    [SerializeField] RaceData currentRaceData;
    [SerializeField] Vehicle playerVehicle;
    [SerializeField] CinemachineVirtualCamera cam;
    [SerializeField] UIManager uiManager;
    [SerializeField] UILeaderboard leaderboard;
    [SerializeField] VehicleCheckpointsContainer vehicleCheckpoints;
    [SerializeField] Animator cinemachineAnimator;

    List<VehicleManager> vehicles;
    Coroutine changingStateCoroutine;
    State currentState;
    int currentStartingTime;
    float[] distances;
    float raceTimeSeconds;
    bool stopUpdate;

    public RaceData RaceData => currentRaceData;
    public List<VehicleManager> Vehicles => vehicles;
    public State CurrentState => currentState;
    public float[] Distances => distances;
    public float CurrentStartingTime => currentStartingTime;
    public float RaceTimeSeconds => raceTimeSeconds;
    public bool StopUpdate { get { return stopUpdate; } set {  stopUpdate = value; } }

    private void OnDestroy() {
        foreach (var vehicle in vehicles)
            vehicle.OnRaceFinished -= OnVehicleFinish;
    }

    private void Start() {
        vehicles = FindObjectsOfType<VehicleManager>().ToList();

        foreach (var vehicle in vehicles)
            vehicle.OnRaceFinished += OnVehicleFinish;

        distances = new float[vehicleCheckpoints.Checkpoints.Length + 1];

        for (int i = 1; i < distances.Length; i++) {
            if (i < vehicleCheckpoints.Checkpoints.Length)
                distances[i] = distances[i - 1] + Vector3.Distance(vehicleCheckpoints.Checkpoints[i - 1].position, vehicleCheckpoints.Checkpoints[i].position);
            else
                distances[i] = distances[i - 1] + Vector3.Distance(vehicleCheckpoints.Checkpoints[i - 1].position, vehicleCheckpoints.Checkpoints[0].position);
        }

        UpdateVehiclesPlacements(false);

        foreach (var vehicle in vehicles)
            vehicle.Init();

        leaderboard.Init(vehicles);
        uiManager.Init();
        if (enableLearning)
            ChangeState(State.Playing);
        else
            ChangeState(State.Init);

        cam.LookAt = cam.Follow = playerVehicle.transform;
    }

    private void FixedUpdate() {
        if (currentState == State.Playing)
            raceTimeSeconds += Time.deltaTime;
        if (stopUpdate) return;
        UpdateVehiclesPlacements(true);
    }

    public void UpdateVehiclesPlacements(bool sendCallback) {
        vehicles = vehicles.OrderByDescending(x => x.vehicleData.loopCount).ThenByDescending(x => x.vehicleData.totalDistance).ToList();

        for (int i = 0; i < vehicles.Count; i++) {
            if (vehicles[i].Initialized && sendCallback && (!vehicles[i].vehicleData.finished || enableLearning)) {
                if (i > vehicles[i].currentPlacement)
                    vehicles[i].OnPlacementChanged?.Invoke(false);
                else if (i < vehicles[i].currentPlacement)
                    vehicles[i].OnPlacementChanged?.Invoke(true);
            }

            vehicles[i].currentPlacement = i;
        }
    }

    public void ChangeState(State state, float delay = 0f) {
        if(changingStateCoroutine != null)
            StopCoroutine(changingStateCoroutine);
        changingStateCoroutine = StartCoroutine(ChangeStateCoroutine(state, delay));
    }

    IEnumerator ChangeStateCoroutine(State state, float delay) {
        yield return new WaitForSeconds(delay);

        currentState = state;

        switch (state) {
            case State.Init:
                currentStartingTime = STARTING_TIME;

                yield return new WaitForSeconds(1f);

                ChangeState(State.Starting);

                break;
            case State.Starting:

                while (currentStartingTime > 0) {
                    yield return new WaitForSeconds(1);
                    currentStartingTime--;
                }
                ChangeState(State.Playing);

                break;
            case State.Playing:

                break;
            case State.End:

                break;
            default:
                break;
        }
    }

    private void OnDrawGizmos() {
        if (vehicleCheckpoints == null) return;
        Gizmos.color = Color.white;
        int checkpointsCount = vehicleCheckpoints.Checkpoints.Length;
        for (int i = 0; i < checkpointsCount; i++)
            Gizmos.DrawLine(vehicleCheckpoints.Checkpoints[i].position, vehicleCheckpoints.Checkpoints[i + 1 > checkpointsCount - 1 ? 0 : i + 1].position);
    }

    private void OnVehicleFinish(VehicleManager vehicleManager) {
        if (vehicleManager.IsPlayer)
            cinemachineAnimator.Play("Finish");
        vehicleManager.vehicleData.finished = true;
        uiManager.RaceFinished(vehicleManager, vehicleManager.IsPlayer);
    }
}
