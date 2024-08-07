using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Playables;

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
    [SerializeField] RacersNames racersNames;
    [SerializeField] Track trackData;
    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] VehicleManager playerVehicle;
    [SerializeField] CinemachineStateDrivenCamera cam;
    [SerializeField] CinemachineVirtualCamera vehicleCamera;
    [SerializeField] UIManager uiManager;
    [SerializeField] UILeaderboard leaderboard;
    [SerializeField] VehicleCheckpointsContainer vehicleCheckpoints;
    [SerializeField] Animator cinemachineAnimator;
    [SerializeField] ParticleSystem speed_vfx;
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
        if (!enableLearning) {
            foreach (var vehicle in vehicles)
                vehicle.OnRaceFinished -= OnVehicleFinish;
        }
    }

    private void Start() {
        if (initOnStart)
            Init();
    }

    public void Init() {
        Debug.Log("[RaceManager] Init");
        if (GlobalData.selectedRaceData != null)
            currentRaceData = GlobalData.selectedRaceData;

        if(vehicleCheckpoints == null)
            vehicleCheckpoints = VehicleCheckpointsContainer.Instance;

        trackData = FindFirstObjectByType<Track>();

        if (!enableLearning) {
            SpawnVehicles();
            foreach (var vehicle in vehicles)
                vehicle.OnRaceFinished += OnVehicleFinish;

        }
        else
            vehicles = FindObjectsByType<VehicleManager>(FindObjectsSortMode.None).ToList();


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

        foreach (var vehicle in vehicles) {
            if (!vehicle.IsPlayer) {
                int nameIndex = UnityEngine.Random.Range(0, racersNames.Names.Length);
                vehicle.SetPlayerName(racersNames.Names[nameIndex]);
            }
            else {
                vehicle.SetPlayerName(UserManager.playerData.PlayerName);
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

    private void Update() {
        if (playerVehicle == null) return;

        float fovFraction = Mathf.Max(playerVehicle.Vehicle.Kmph - 50, 0) / 50f;

        if (playerVehicle.Vehicle.NOSActive) {
            fovFraction = 1;
            if (speed_vfx != null) speed_vfx.Play();
        }
        else 
            if (speed_vfx != null) speed_vfx.Stop();

        float targetFov = Mathf.Lerp(70f, 90f, fovFraction);

        vehicleCamera.m_Lens.FieldOfView = Mathf.Lerp(vehicleCamera.m_Lens.FieldOfView, targetFov, Time.deltaTime * 2);
    }

    private void FixedUpdate() {
        if (!initialized) return;

        if (currentState == State.Playing)
            raceTimeSeconds += Time.deltaTime;

        if(playerVehicle != null) {
            UpdateVehicleCamera();
        }

        if (stopUpdate) return;
        UpdateVehiclesPlacements(true);
    }

    void UpdateVehicleCamera() {
        //vehicleCamera.m_Lens.FieldOfView = Mathf.Lerp(70f, 80f, Mathf.Max(playerVehicle.Vehicle.Kmph - 50, 0) / 20f);
    }

    void SpawnVehicles() {
        vehicles.Clear();
        List<Transform> spawnPoints = trackData.GetSpawnPoints();

        if(spawnPoints.Count < currentRaceData.MaxPlayers) {
            Debug.LogError("Not enough spawn points, max players will be : " + spawnPoints.Count);
        }

        for (int i = 0; i < currentRaceData.MaxPlayers; i++) {
            if (i > spawnPoints.Count - 1) break;

            VehicleManager vehicle;

            if (i == spawnPoints.Count - 1 || i == currentRaceData.MaxPlayers - 1) {
                //Spawn Player
                vehicle = Instantiate(vehiclesContainer.GetEquippedVehicle().Prefab, spawnPoints[i].position, spawnPoints[i].rotation);
                DestroyImmediate(vehicle.GetComponent<DecisionRequester>());
                DestroyImmediate(vehicle.GetComponent<Vehicle_Agent>());
                DestroyImmediate(vehicle.GetComponent<BehaviorParameters>());

                VehicleSaveData saveData = vehiclesContainer.vehicleSaveDatas[UserManager.playerData.GetInt(PlayerPrefsStrings.SELECTED_VEHICLE)];
                vehicle.Init(vehiclesContainer.GetEquippedVehicle(), saveData, true);
                playerVehicle = vehicle;
                uiManager.SetVehicle(vehicle);
            }
            else {
                //Spawn opponents
                int vehicleIndex = UnityEngine.Random.Range(0, vehiclesContainer.Vehicles.Length);
                vehicle = Instantiate(vehiclesContainer.Vehicles[vehicleIndex].Prefab, spawnPoints[i].position, spawnPoints[i].rotation);
                VehicleSaveData saveData = new VehicleSaveData(vehicleIndex);
                saveData.Randomize(vehiclesContainer.Vehicles[vehicleIndex]);
                vehicle.Init(vehiclesContainer.Vehicles[vehicleIndex], saveData);
            }

            vehicles.Add(vehicle);
            vehicle.transform.position = spawnPoints[i].position;
            vehicle.transform.rotation = spawnPoints[i].rotation;
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
            UserManager.playerData.AddInt(PlayerPrefsStrings.CASH, RaceData.CoinsRewards[placement] * (GlobalData.enableSpecialEventBonus && RaceData.UseEvent ? 2 : 1));
        if(placement < RaceData.ExpReward.Length)
            LevelSystem.Instance.AddExp(RaceData.ExpReward[placement] * (GlobalData.enableSpecialEventBonus && RaceData.UseEvent ? 2 : 1));
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
