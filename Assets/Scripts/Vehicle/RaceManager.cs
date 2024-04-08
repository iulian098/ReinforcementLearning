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
        End
    }

    public bool enableLearning;

    [SerializeField] RaceData currentRaceData;
    [SerializeField] Track trackData;
    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] VehicleManager playerVehicle;
    [SerializeField] CinemachineStateDrivenCamera cam;
    [SerializeField] UIManager uiManager;
    [SerializeField] UILeaderboard leaderboard;
    [SerializeField] VehicleCheckpointsContainer vehicleCheckpoints;
    [SerializeField] Animator cinemachineAnimator;
    [SerializeField] bool initOnStart;

    List<VehicleManager> vehicles = new List<VehicleManager>();
    Coroutine changingStateCoroutine;
    State currentState;
    int currentStartingTime;
    float[] distances;
    float raceTimeSeconds;
    bool stopUpdate;
    bool initialized = false;

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
        if (initOnStart)
            Init();
    }

    public void Init() {
        Debug.Log("[RaceManager] Init");
        if(vehicleCheckpoints == null)
            vehicleCheckpoints = VehicleCheckpointsContainer.Instance;

        trackData = FindFirstObjectByType<Track>();

        if (!enableLearning)
            SpawnVehicles();
        else
            vehicles = FindObjectsByType<VehicleManager>(FindObjectsSortMode.None).ToList();

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

        if (enableLearning) {
            foreach (var vehicle in vehicles) {
                if (!vehicle.IsPlayer) {
                    int vehicleIndex = Array.IndexOf(vehiclesContainer.Vehicles, vehicle.vehicleConfig);
                    VehicleSaveData randSaveData = new VehicleSaveData(vehicleIndex);
                    randSaveData.Randomize(vehicle.vehicleConfig);
                    vehicle.Init(randSaveData);
                }
                else {
                    int vehicleConfigIndex = Array.IndexOf(vehiclesContainer.Vehicles, vehicle.vehicleConfig);
                    vehicle.Init(vehiclesContainer.GetSaveData(vehicleConfigIndex));
                }
            }
        }

        leaderboard.Init(vehicles);
        uiManager.Init();
        if (enableLearning)
            ChangeState(State.Playing);
        else
            ChangeState(State.Init);

        if (playerVehicle != null)
            cam.LookAt = cam.Follow = playerVehicle.transform;
        else
            cam.LookAt = cam.Follow = vehicles[0].transform;

        initialized = true;
    }

    private void FixedUpdate() {
        if (!initialized) return;

        if (currentState == State.Playing)
            raceTimeSeconds += Time.deltaTime;
        if (stopUpdate) return;
        UpdateVehiclesPlacements(true);
    }

    void SpawnVehicles() {
        vehicles.Clear();
        List<Transform> spawnPoints = trackData.GetSpawnPoints();

        if(spawnPoints.Count < currentRaceData.MaxPlayers) {
            Debug.LogError("Not enough spawn points, max players will be : " + spawnPoints.Count);
        }

        for (int i = 0; i < currentRaceData.MaxPlayers; i++) {
            if (i > spawnPoints.Count - 1) break;

            if (i == spawnPoints.Count - 1 || i == currentRaceData.MaxPlayers - 1) {
                //Spawn Player
                VehicleManager vehicle = Instantiate(vehiclesContainer.GetEquippedVehicle().Prefab, spawnPoints[i].position, spawnPoints[i].rotation);
                VehicleSaveData saveData = vehiclesContainer.vehicleSaveDatas[vehiclesContainer.selectedVehicle];
                vehicles.Add(vehicle);
                vehicle.Init(vehiclesContainer.GetEquippedVehicle(), saveData, true);
                playerVehicle = vehicle;
                uiManager.SetVehicle(vehicle);
            }
            else {
                //Spawn opponents
                int vehicleIndex = UnityEngine.Random.Range(0, vehiclesContainer.Vehicles.Length);
                VehicleManager vehicle = Instantiate(vehiclesContainer.Vehicles[vehicleIndex].Prefab, spawnPoints[i].position, spawnPoints[i].rotation);
                VehicleSaveData saveData = new VehicleSaveData(vehicleIndex);
                saveData.Randomize(vehiclesContainer.Vehicles[vehicleIndex]);
                vehicles.Add(vehicle);
                vehicle.Init(vehiclesContainer.Vehicles[vehicleIndex], saveData);
            }
        }
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
                cinemachineAnimator.Play("Finish");
                break;
            default:
                break;
        }
    }

    private void OnVehicleFinish(VehicleManager vehicleManager) {
        if (vehicleManager.IsPlayer) {
            ChangeState(State.End);
            OnPlayerFinishedRace(vehicleManager.currentPlacement);
        }
        vehicleManager.vehicleData.finished = true;
        uiManager.RaceFinished(vehicleManager, vehicleManager.IsPlayer);
    }

    private void OnPlayerFinishedRace(int placement) {
        if (placement < RaceData.CoinsRewards.Length)
            UserManager.playerData.SetInt(PlayerPrefsStrings.CASH, UserManager.playerData.GetInt(PlayerPrefsStrings.CASH) + RaceData.CoinsRewards[placement]);
        if(placement < 3) {
            RaceData.saveData.placement = placement;
        }
    }

    private void OnDrawGizmos() {
        if (vehicleCheckpoints == null) return;
        Gizmos.color = Color.white;
        int checkpointsCount = vehicleCheckpoints.Checkpoints.Length;
        for (int i = 0; i < checkpointsCount; i++)
            Gizmos.DrawLine(vehicleCheckpoints.Checkpoints[i].position, vehicleCheckpoints.Checkpoints[i + 1 > checkpointsCount - 1 ? 0 : i + 1].position);
    }
}
