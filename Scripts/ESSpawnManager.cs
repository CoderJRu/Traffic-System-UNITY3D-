﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESSpawnManager : MonoBehaviour
{
    //gateway
    public enum SpawnType
    {
        AutoGenerated,
        ManuallyGenerated
    }
    [Tooltip("Please Do Not Used 'LOOKAT' as your as your start rotation method when this is set to autogenerated")]
    public SpawnType _spawntype = SpawnType.ManuallyGenerated;
    [HideInInspector]
    public ESGateWaySpawnSetup[] spawnpoints;
    [HideInInspector]
    public GameObject[] Spawnobjects;
    //public float checkspawndist = 10;
    public Transform[] vehicles;
    public Transform[] MotorBikes;
    //public float Delaytime = 5f;
    [Tooltip("dont use above 100 vehicles if this for mobile")]
    [Range(1, 300)]
    [Header("Make Sure this is equal or greater than human spawncount ;)")]
    public int MaxAllowedVehicles = 10;
    [Tooltip("Will not Spawn if Camera Is Veiwing")] public bool BasedOnVisibleSpawns = true;
    [Range(0, 10)]
    public int MaxNumberOfSpwanedBikes = 10;
    [Range(200, 90000)]
    public float BikeDistPoolDistance = 500f;
    [Range(200, 90000)]
    public float NearestSpawnDistance = 300f;
    public enum StartRotation
    {
        LookAt,
        CopyRotation
    }
    //
    public StartRotation _startrotation = StartRotation.LookAt;
    [Tooltip("Dont mess around with this ;}{")]
    [Range(0.02f, 15)]
    public float SpawnRate = 0.19f;
    [Tooltip("Dont mess around with this ;}{")]
    [Range(1, 15)]
    public float SpawnStartTime = 1.5f;
    public int Iteration = 5;
    public float LastvehDispart = 30f;
    public Vector3 SpawnOffset = new Vector3(0, 1.3f, 0);
    //
    [Header("AutoGeneratedSettings")]
    [Tooltip("CageSettings/useful for only Sphere cage")]
    public float CageSize = 5.0f;
    [Tooltip("Use if vehicles spawn on each other")] public bool AdvancedObstacleDetection = true;
    [Tooltip("CubeSettings")]
    public Vector3 BoxSize = new Vector3(1, 1, 1);
    public enum CageType
    {
        sphere,
        cube
    }
    public CageType GetCageType = CageType.cube;
    //private
    [SerializeField] private int spawnindex;
    private int vehicleindex;
    private float counter;
    private float starter;
    [Header("DEBUG")]
    public Transform player;
    [HideInInspector]
    public List<GameObject> SpawnedVeh = new List<GameObject>();
    [HideInInspector] public List<GameObject> SpawnedBikes = new List<GameObject>();
    //
    [HideInInspector]
    public Transform veh;
    [HideInInspector]
    public bool parentspawnedvehicles = true;
    private GameObject Pvehicles;
    [SerializeField] private int numofveh, numofbikes;
    //[HideInInspector]
    public List<ESGateWaySpawnSetup> nearestspawn;
    //public ESPathGenerator generator;
    [HideInInspector] public bool IntegratedEasyRoad3;
    //
    [HideInInspector] public ESGateWay[] gateWays;
    private void Awake()
    {
        ESTrafficLghtCtrl[] lights = GameObject.FindObjectsOfType<ESTrafficLghtCtrl>();
        if (lights.Length > 0)
        {
            foreach (ESTrafficLghtCtrl mylight in lights)
            {
                mylight.DistApartLastVeh = LastvehDispart;
            }
        }
        if (_spawntype == SpawnType.ManuallyGenerated)
        {
            Spawn();
            SpawnBikes();
            spawnpoints = GameObject.FindObjectsOfType<ESGateWaySpawnSetup>();
        }
        else
        {
            Spawn();
            SpawnBikes();
            //generator = GameObject.FindObjectOfType<ESPathGenerator>();
            if (IntegratedEasyRoad3 == false)
            {
                Spawnobjects = GameObject.FindGameObjectsWithTag("Nodes");
            }
            else
            {
                gateWays = GameObject.FindObjectsOfType<ESGateWay>();
                if (gateWays.Length > 0)
                {
                    System.Array.Resize(ref Spawnobjects, gateWays.Length);
                    for (int i = 0; i < gateWays.Length; ++i)
                    {
                        Spawnobjects[i] = gateWays[i].gameObject;
                    }
                }
            }
            if (Spawnobjects.Length > 0)
            {
                for (int i = 0; i < Spawnobjects.Length; ++i)
                {
                    Spawnobjects[i].layer = 2;
                    if (Spawnobjects[i].GetComponent<ESNodeManager>().NextNode != null &&
                      Spawnobjects[i].GetComponent<ESNodeManager>().ConnectedNode.Count == 0
                        && Spawnobjects[i].GetComponent<ESNodeManager>().NextNode.GetComponent<ESNodeManager>().NextNode != null)
                    {
                        if (i % Iteration == 0)
                        {
                            Spawnobjects[i].gameObject.AddComponent<ESGateWaySpawnSetup>();
                            Spawnobjects[i].GetComponent<ESGateWaySpawnSetup>().UseQuickTool = false;
                            Spawnobjects[i].GetComponent<ESGateWaySpawnSetup>().TargetNode = Spawnobjects[i].GetComponent<ESNodeManager>().NextNode;
                            Spawnobjects[i].GetComponent<ESGateWaySpawnSetup>().ConsiderVisibleMeshes = BasedOnVisibleSpawns;
                            if (GetCageType == CageType.sphere)
                            {
                                Spawnobjects[i].AddComponent<SphereCollider>();
                                Spawnobjects[i].GetComponent<SphereCollider>().isTrigger = true;
                                Spawnobjects[i].GetComponent<SphereCollider>().radius = CageSize;
                            }
                            else
                            {
                                Spawnobjects[i].AddComponent<BoxCollider>();
                                Spawnobjects[i].GetComponent<BoxCollider>().isTrigger = true;
                                Spawnobjects[i].GetComponent<BoxCollider>().size = BoxSize;
                            }
                        }
                    }
                }
            }
        }
    }
    //
    private void Start()
    {
        if (_spawntype == SpawnType.AutoGenerated)
        {
            spawnpoints = GameObject.FindObjectsOfType<ESGateWaySpawnSetup>();
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        /*
        if (spawnpoints.Length > 0)
        {
            for (int i = 0; i < spawnpoints.Length; i++)
            {
                spawnpoints[i].LineOfSight = LineOfSight;
                spawnpoints[i].m_DistApartFromPlayer = m_DistApartFromPlayer;
                spawnpoints[i].m_SpawnAngle = m_SpawnAngle;
                spawnpoints[i].distanceapart = distanceapart;
            }
        }
        */
        //
        InvokeRepeating("ManageSpawnAI", SpawnStartTime, SpawnRate);
        InvokeRepeating("ManageSpawnBikes", (SpawnStartTime), SpawnRate + 0.2f);
    }
    //
    //
    #region Manaully
    private void ManageSpawnBikes()
    {
        if (nearestspawn.Count == 0) return;
        if (SpawnedBikes.Count == 0) return;
        int bikeindex = Random.Range(0, SpawnedBikes.Count);
        int spawnpointindex = Random.Range(0, nearestspawn.Count);
        if (nearestspawn[spawnpointindex].ignorebikes) return;
        //
        Collider[] cols = new Collider[0];
        bool ObstaclesInbound = new bool();
        //
        if (AdvancedObstacleDetection)
        {
            if (GetCageType == CageType.cube)
                cols = Physics.OverlapBox(nearestspawn[spawnpointindex].transform.position, BoxSize, nearestspawn[spawnpointindex].transform.rotation);
            else
                cols = Physics.OverlapSphere(nearestspawn[spawnpointindex].transform.position, CageSize);
            //
            if (cols.Length > 0)
            {
                for (int g = 0; g < cols.Length; g++)
                {
                    if (cols[g].attachedRigidbody != null)
                    {
                        ObstaclesInbound = true;
                    }
                }
            }
            else
            {
                ObstaclesInbound = false;
            }
        }
        //
        if (SpawnedBikes[bikeindex].gameObject.activeSelf == false && ObstaclesInbound == false && nearestspawn[spawnpointindex].GetComponent<ESNodeManager>().NextNode != null)
        {
            Transform bike = SpawnedBikes[bikeindex].transform;
            if (!nearestspawn[spawnpointindex].CanSpawn)
            {
                return;
            }
            if (nearestspawn[spawnpointindex].CanSpawn && nearestspawn[spawnindex].returnGateUpdate == false && !nearestspawn[spawnindex].neverspawn)
            {
                bike.position = nearestspawn[spawnpointindex].transform.position + SpawnOffset;
                if (_startrotation == StartRotation.LookAt)
                {
                    bike.transform.LookAt(nearestspawn[spawnpointindex].TargetNode.position);
                }
                else
                {
                    bike.localRotation = nearestspawn[spawnpointindex].TargetNode.transform.localRotation;
                }
                //
                nearestspawn[spawnindex].returnGateUpdate = true;
                nearestspawn[spawnindex].StartCoroutine(nearestspawn[spawnindex].Cool(5));
                bike.GetComponent<UL_MotorCycleController>().TargetNode = nearestspawn[spawnpointindex].TargetNode;
                bike.GetComponent<UL_MotorCycleController>().distanceapartplayer = BikeDistPoolDistance;
                SpawnedBikes[bikeindex].SetActive(true);
            }
        }
        else
        {
            return;
        }
    }
    private void ManageSpawnAI()
    {
        nearestspawn = new List<ESGateWaySpawnSetup>();

        for (int i = 0; i < spawnpoints.Length; ++i)
        {
            if (UCTMath.CalculateVector3Distance(player.position, spawnpoints[i].transform.position) < NearestSpawnDistance * NearestSpawnDistance && !spawnpoints[i].neverspawn && spawnpoints[i].CanSpawn && !spawnpoints[i].returnGateUpdate)
            {
                nearestspawn.Add(spawnpoints[i]);
            }
            else
            {
                //nearestspawn.Add(spawnpoints[i]);
                if (nearestspawn.Count > 0 && i < nearestspawn.Count)
                {
                    nearestspawn[i].returnGateUpdate = true;
                    nearestspawn.RemoveAt(i);
                }
            }
        }
        if (nearestspawn.Count == 0) return;
        vehicleindex = Random.Range(0, SpawnedVeh.Count);
        spawnindex = Random.Range(0, nearestspawn.Count);
        Collider[] cols = new Collider[0];
        bool ObstaclesInbound = new bool();
        //
        if (AdvancedObstacleDetection)
        {
            if (GetCageType == CageType.cube)
                cols = Physics.OverlapBox(nearestspawn[spawnindex].transform.position, BoxSize, nearestspawn[spawnindex].transform.rotation);
            else
                cols = Physics.OverlapSphere(nearestspawn[spawnindex].transform.position, CageSize);
            //
            if (cols.Length > 0)
            {
                for (int g = 0; g < cols.Length; g++)
                {
                    if (cols[g].attachedRigidbody != null)
                    {
                        ObstaclesInbound = true;
                    }
                }
            }
            else
            {
                ObstaclesInbound = false;
            }
        }
        if (SpawnedVeh[vehicleindex].gameObject.activeSelf == false && ObstaclesInbound == false && nearestspawn[spawnindex].CanSpawn && !nearestspawn[spawnindex].neverspawn)
        {
            veh = SpawnedVeh[vehicleindex].transform;
            if (!nearestspawn[spawnindex].CanSpawn)
            {
                return;
            }
            if (nearestspawn[spawnindex].CanSpawn)
            {
                //set vehicles position
                veh.localPosition = nearestspawn[spawnindex].transform.position + SpawnOffset;
                if (_startrotation == StartRotation.LookAt)
                {
                    veh.transform.LookAt(nearestspawn[spawnindex].TargetNode.position);
                }
                else
                {
                    veh.localRotation = nearestspawn[spawnindex].TargetNode.transform.localRotation;
                }
                //
                nearestspawn[spawnindex].returnGateUpdate = true;
                nearestspawn[spawnindex].StartCoroutine(nearestspawn[spawnindex].Cool(5));
                //
                veh.GetComponent<ESVehicleAI>().playerdist = 0.0f;
                veh.GetComponent<ESVehicleAI>().TargetNode = nearestspawn[spawnindex].TargetNode;
                veh.GetComponent<ESVehicleAI>().trafficlightctrl = null;
                veh.GetComponent<ESVehicleAI>().TriggerObject = null;
                veh.GetComponent<ESVehicleAI>().callsensor = false;
                veh.GetComponent<ESVehicleAI>().Stop = false;
                veh.GetComponent<ESVehicleAI>().Trafficlightbraking = false;
                veh.GetComponent<ESVehicleAI>().AngleBraking = false;
                veh.GetComponent<ESVehicleAI>().topspeed = veh.GetComponent<ESVehicleAI>().backuptopspeed;
                veh.GetComponent<ESVehicleAI>().Brakemul = 0.0f;
                veh.GetComponent<ESVehicleAI>().returntriggerstay = false;
                veh.GetComponent<ESVehicleAI>().EnableAvoid = false;
                veh.GetComponent<ESVehicleAI>().CopySpeed = false;
                veh.GetComponent<ESVehicleAI>().alignwithroad = false;
                if (veh.GetComponent<ESVehicleAI>().CarRb != null)
                    veh.GetComponent<ESVehicleAI>().CarRb.angularDrag = 5f;
                //if (veh.GetComponent<ESVehicleAI>().playerdist > veh.GetComponent<ESVehicleAI>().SpawnDistance)
                SpawnedVeh[vehicleindex].SetActive(true);
            }
        }
        else
        {
            return;
        }
        //
    }
    //
    private void SpawnBikes()
    {
        if (MotorBikes.Length == 0) return;
        if (Pvehicles == null)
        {
            GameObject parentveh = new GameObject("AI");
            Pvehicles = parentveh;
        }
        //
        SpawnedBikes = new List<GameObject>();
        for (int i = 0; i < MaxNumberOfSpwanedBikes; ++i)
        {
            int bikeindex = Random.Range(0, MotorBikes.Length);
            GameObject myspawned = Instantiate(MotorBikes[bikeindex].gameObject, Vector3.zero, Quaternion.identity, Pvehicles.transform);
            SpawnedBikes.Add(myspawned);
            myspawned.SetActive(false);
        }
        numofbikes = SpawnedBikes.Count;
    }
    //
    private void Spawn()
    {
        if (Pvehicles == null)
        {
            GameObject parentveh = new GameObject("AI");
            Pvehicles = parentveh;
        }
        SpawnedVeh = new List<GameObject>();
        //
        for (int i = 0; i < MaxAllowedVehicles; ++i)
        {
            vehicleindex = Random.Range(0, vehicles.Length);
            GameObject spawned = Instantiate(vehicles[vehicleindex].gameObject, Vector3.zero, Quaternion.identity, Pvehicles.transform);
            SpawnedVeh.Add(spawned);
            spawned.SetActive(false);
        }
        numofveh = SpawnedVeh.Count;
    }
    #endregion
    //
}
//
