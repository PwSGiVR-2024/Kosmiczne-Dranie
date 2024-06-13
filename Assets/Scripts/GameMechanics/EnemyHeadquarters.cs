using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadquarters : Headquarters
{
    public Spawner spawner;

    public class Target
    {
        public MonoBehaviour target;
        public TargetPriority priority;

        public Target(MonoBehaviour target, TargetPriority priority)
        {
            this.target = target;
            this.priority = priority;
        }
    }

    public GameManager manager;
    public SiteController[] sites;

    public enum EventType { SiteCaptured, SiteAttacked }
    public enum TargetPriority { Low, Medium, High }
    public enum Objective { Defend, Attack }
    public enum Behaviour { Defensive, Offensive }

    public Objective currentObjective;
    public Behaviour currentBehaviour;

    public List<TaskForceController> ownTaskForces = new();
    public HashSet<TaskForceController> detectedTaskForces = new();
    public List<TaskForceController> enemyTaskForces = new();
    public HashSet<SiteController> siteTargets = new();
    public HashSet<Target> targets = new();

    private void Start()
    {
        sites = FindObjectsOfType<SiteController>();

        //spawner.onEnemyOutpostSpawned.AddListener((outpost) => outpostNetwork.Add(outpost));

        foreach (var site in sites)
        {
            site.onCaptured.AddListener(() => EvaluateSite(site, EventType.SiteCaptured));
            site.onAttacked.AddListener(() => EvaluateSite(site, EventType.SiteAttacked));
        }

        spawner.onTaskForceSpawned.AddListener(RegisterTaskForce);
    }

    private void Update()
    {
        EvaluateStatus();
    }

    private void EvaluateStatus()
    {
        currentObjective = Objective.Attack;
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
                    siteTargets.Add(site);
                    return;
                }

                else if (site.currentController == Affiliation.Red)
                {
                    siteTargets.Remove(site);
                }

                break;
            }

            case EventType.SiteAttacked:
            {
                if (site.attacker != Affiliation.Red)
                {
                    siteTargets.Add(site);
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

    }

    private void RegisterTaskForce(TaskForceController taskForce)
    {
        taskForce.onTaskForceSpotted.AddListener((detected) => {
            detectedTaskForces.Add(detected);
            detected.onTaskForceDestroyed.AddListener((destroyed) => detectedTaskForces.Remove(destroyed));
            });
    }
}
