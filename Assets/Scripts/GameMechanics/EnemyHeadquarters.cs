using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadquarters : Headquarters
{
    private Dictionary<string, int> taskForcesCounts = new();

    private TaskForcePreset scoutPreset;
    private TaskForcePreset earlyPreset;
    private TaskForcePreset midPreset;
    private TaskForcePreset latePreset;

    public EnemyFleetManager fleetManager;
    public PlayerHeadquarters playerHeadquarters;

    public Spawner spawner;

    public int spawnCooldown = 30;

    public class Target
    {
        public MonoBehaviour target;
        public TargetPriority priority;
        public int weight;

        public Target(MonoBehaviour target, TargetPriority priority)
        {
            this.target = target;
            this.priority = priority;
        }

        private int CalculateWeight()
        {
            return 0;
        } 
    }

    public GameManager manager;
    public SiteController[] sites;
    public List<SiteController> capturedSites = new();
    public List<SiteController> hostileSites = new();
    public List<SiteController> siegedSites = new();

    public enum EventType { SiteCaptured, SiteAttacked }
    public enum TargetPriority { Low, Medium, High }
    public enum Objective { Defend, Attack }
    public enum Behaviour { Defensive, Cautious, Offensive }

    public Objective currentObjective;
    public Behaviour currentBehaviour;

    public List<TaskForceController> ownTaskForces = new();
    public List<TaskForceController> detectedTaskForces = new();
    public List<TaskForceController> enemyTaskForces = new();
    public List<Target> targets = new();

    private void Awake()
    {
        scoutPreset = new TaskForcePreset(
            frigatePrefab: fleetManager.frigatePrefab,
            destroyerPrefab: null,
            cruiserPrefab: null,
            battleshipPrefab: null,
            frigatesCount: 1,
            destroyersCount: 0,
            cruisersCount: 0,
            battleshipsCount: 0);

        earlyPreset = new TaskForcePreset(
            frigatePrefab: fleetManager.frigatePrefab,
            destroyerPrefab: fleetManager.destroyerPrefab,
            cruiserPrefab: null,
            battleshipPrefab: null,
            frigatesCount: 4,
            destroyersCount: 1,
            cruisersCount: 0,
            battleshipsCount: 0);

        midPreset = new TaskForcePreset(
            frigatePrefab: fleetManager.frigatePrefab,
            destroyerPrefab: fleetManager.destroyerPrefab,
            cruiserPrefab: fleetManager.cruiserPrefab,
            battleshipPrefab: null,
            frigatesCount: 8,
            destroyersCount: 4,
            cruisersCount: 1,
            battleshipsCount: 0);

        latePreset = new TaskForcePreset(
            frigatePrefab: fleetManager.frigatePrefab,
            destroyerPrefab: fleetManager.destroyerPrefab,
            cruiserPrefab: fleetManager.cruiserPrefab,
            battleshipPrefab: fleetManager.battleshipPrefab,
            frigatesCount: 16,
            destroyersCount: 8,
            cruisersCount: 4,
            battleshipsCount: 1);

        
    }



    private void Start()
    {
        scoutPreset.MultiplyCounts(24);
        scoutPreset.DebugLogValues(label: "Scout preset");

        earlyPreset.MultiplyCounts(12);
        earlyPreset.DebugLogValues(label: "Early preset");

        midPreset.MultiplyCounts(6);
        midPreset.DebugLogValues(label: "Mid preset");

        latePreset.MultiplyCounts(4);
        latePreset.DebugLogValues(label: "Late preset");



        sites = FindObjectsOfType<SiteController>();

        //spawner.onEnemyOutpostSpawned.AddListener((outpost) => outpostNetwork.Add(outpost));

        foreach (var site in sites)
        {
            site.onCaptured.AddListener(() => EvaluateSite(site, EventType.SiteCaptured));
            site.onAttacked.AddListener(() => EvaluateSite(site, EventType.SiteAttacked));
        }

        spawner.onTaskForceSpawned.AddListener(RegisterTaskForce);

        StartCoroutine(CreateTaskForceInInterval());
        StartCoroutine(OrderTaskForcesInInterval(5));
    }

    private void Update()
    {
        EvaluateStatus();
    }

    private void EvaluateStatus()
    {
        if (capturedSites.Count < hostileSites.Count)
            currentBehaviour = Behaviour.Defensive;

        else if (capturedSites.Count > hostileSites.Count)
            currentBehaviour = Behaviour.Offensive;
    }

    private void EvaluateSite(SiteController site, EventType eventType)
    {
        switch (eventType)
        {
            case EventType.SiteCaptured:
            {
                if (site.currentController != Affiliation.Red)
                {
                    hostileSites.Add(site);
                    capturedSites.Remove(site);
                    siegedSites.Remove(site);
                    return;
                }

                else if (site.currentController == Affiliation.Red)
                {
                    hostileSites.Remove(site);
                    capturedSites.Add(site);
                    siegedSites.Remove(site);
                }

                break;
            }

            case EventType.SiteAttacked:
            {
                if (site.attacker != Affiliation.Red && site.currentController == Affiliation.Red)
                {
                    siegedSites.Add(site);
                    return;
                }

                else if (site.attacker == Affiliation.Red)
                {
                    siegedSites.Remove(site);
                    return;
                }
                break;
            }
        }
    }

    private void EvaluateTargets()
    {

    }

    private void AssignTargets()
    {
        if (capturedSites.Count == sites.Length)
        {
            foreach (var taskForce in fleetManager.taskForces)
            {
                taskForce.SetDestination(playerHeadquarters.transform.position);
            }

            return;
        }

        foreach (var taskForce in fleetManager.taskForces)
        {
            if (taskForce.CurrentState != State.Idle)
                continue;

            Vector3 closestTarget = Vector3.positiveInfinity;

            for (int i = 0; sites.Length > i; i++)
            {
                if (sites[i].currentController == Affiliation.Red)
                    continue;

                if (Vector3.Distance(taskForce.Commander.transform.position, sites[i].transform.position) < Vector3.Distance(taskForce.Commander.transform.position, closestTarget))
                    closestTarget = sites[i].transform.position;
            }

            if (closestTarget != Vector3.positiveInfinity)
                taskForce.SetDestination(closestTarget);
        }
    }

    private void RegisterTaskForce(TaskForceController taskForce)
    {
        taskForce.onTaskForceSpotted.AddListener((detected) => {
            detectedTaskForces.Add(detected);
            detected.onTaskForceDestroyed.AddListener((destroyed) => detectedTaskForces.Remove(destroyed));
            });
    }

    private IEnumerator CreateTaskForceInInterval()
    {
        WaitForSeconds cooldown = new WaitForSeconds(spawnCooldown);
        TaskForceController taskForce = null;

        while (true)
        {
            taskForce = fleetManager.ProcureTaskForce(latePreset, transform.position, "enemy task force");
            if (taskForce) yield return cooldown;

            taskForce = fleetManager.ProcureTaskForce(midPreset, transform.position, "enemy task force");
            if (taskForce) yield return cooldown;

            taskForce = fleetManager.ProcureTaskForce(earlyPreset, transform.position, "enemy task force");
            if (taskForce) yield return cooldown;

            taskForce = fleetManager.ProcureTaskForce(scoutPreset, transform.position, "enemy task force");
            if (taskForce) yield return cooldown;


            //if (taskForce == null)
            //    taskForce = fleetManager.ProcureTaskForce(latePreset, transform.position, "enemy task force");

            //else if (taskForce == null)
            //    taskForce = fleetManager.ProcureTaskForce(midPreset, transform.position, "enemy task force");

            //else if (taskForce == null)
            //    taskForce = fleetManager.ProcureTaskForce(earlyPreset, transform.position, "enemy task force");

            //else if (taskForce == null)
            //    taskForce = fleetManager.ProcureTaskForce(scoutPreset, transform.position, "enemy task force");


            Debug.Log("mewing");
            yield return null;
        }
    }

    private IEnumerator OrderTaskForcesInInterval(float seconds)
    {
        WaitForSeconds interval = new WaitForSeconds(seconds);

        while (true)
        {
            AssignTargets();

            yield return interval;
        }
    }
}
