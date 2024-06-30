using System.Collections;
using UnityEngine;
using UnityEngine.Events;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    public Material taskForceLineRendererMaterial;

    public Affiliation affiliation = Affiliation.None;
    public GameManager gameManager;

    public GameObject outpostsContainer;
    public GameObject taskForcesContainer;
    public GameObject unitsContainer;
    public GameObject projectilesContainer;

    public GameObject taskForceHUD;
    public GameObject outpostHUD;
    public GameObject indicatorTF;
    public GameObject inidcatorOutp;


    private int unitCount = 0;
    private int outpostCount = 0;
    private int taskForceCount = 0;


    public UnityEvent<AiController> onUnitSpawned = new();
    public UnityEvent<Outpost> onOutpostSpawned = new();
    public UnityEvent<TaskForceController> onTaskForceSpawned = new();

    private IEnumerator SpawnOverFrames(GameObject prefab, Vector3 position, int count, TaskForceController taskForce, System.Action callback)
    {
        if (count < 1 || prefab == null)
        {
            callback.Invoke();
            yield break;
        }

        while (true)
        {
            AiController member = AiController.Create(position, prefab, taskForce, projectilesContainer, affiliation);
            member.transform.SetParent(unitsContainer.transform, true);
            member.gameObject.name += unitCount;
            taskForce.AddUnit(member);
            onUnitSpawned.Invoke(member);
            unitCount++;

            float spawnRange = 0;
            spawnRange = count;
            spawnRange = Mathf.Sqrt(spawnRange);
            spawnRange *= member.Values.size;

            yield return null;

            for (int i = 1; i < count; i++)
            {
                member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, spawnRange), prefab, taskForce, projectilesContainer, affiliation);
                member.transform.SetParent(unitsContainer.transform, true);
                member.gameObject.name += unitCount;
                taskForce.AddUnit(member);
                onUnitSpawned.Invoke(member);
                unitCount++;
                yield return null;
            }
            
            callback.Invoke();
            yield break;
        }
    }


    public virtual TaskForceController SpawnTaskForce(GameObject unitPrefab, Vector3 position, int spawnCount, string name)
    {
        if (affiliation == Affiliation.None)
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, gameManager, affiliation);
        taskForce.transform.SetParent(taskForcesContainer.transform);

        AiController commander = AiController.Create(position, unitPrefab, taskForce, projectilesContainer, affiliation);
        commander.transform.SetParent(unitsContainer.transform, true);
        commander.gameObject.name += unitCount;
        taskForce.AddUnit(commander);
        onUnitSpawned.Invoke(commander);
        unitCount++;

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (spawnCount > 1)
        {
            range = spawnCount;
            range = Mathf.Sqrt(range);
            range *= commander.Values.size;
        }

        for (int i = 1; i < spawnCount; i++)
        {
            AiController member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, range), unitPrefab, taskForce, projectilesContainer, affiliation);
            member.transform.SetParent(unitsContainer.transform, true);
            member.gameObject.name += unitCount;
            taskForce.AddUnit(member);
            onUnitSpawned.Invoke(member);
            unitCount++;
        }

        onTaskForceSpawned.Invoke(taskForce);

        HUDController.Create(taskForce, taskForceHUD, gameManager);
        return taskForce;
    }

    public virtual TaskForceController SpawnTaskForce(TaskForcePreset preset, Vector3 position, string name, bool slow=true)
    {
        void frigatesRDY(TaskForceController taskForce)
        {
            taskForce.frigatesRDY = true;
        }

        void destroyersRDY(TaskForceController taskForce)
        {
            taskForce.destroyersRDY = true;
        }

        void cruisersRDY(TaskForceController taskForce)
        {
            taskForce.cruisersRDY = true;
        }

        void battleshipsRDY(TaskForceController taskForce)
        {
            taskForce.battleshipsRDY = true;
        }

        if (affiliation == Affiliation.None)
            return null;

        if ((preset.battleshipsCount + preset.cruisersCount + preset.destroyersCount + preset.frigatesCount) < 1)
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, gameManager, affiliation);
        taskForce.transform.SetParent(taskForcesContainer.transform);

        onTaskForceSpawned.Invoke(taskForce);

        HUDController.Create(taskForce, taskForceHUD, gameManager);
        AddLineRenderer(taskForce.gameObject, taskForceLineRendererMaterial);

        // spawn over many frames, does not create lag, but is slow
        // once given type of unit is spawned, callback is invoked
        // in this case callback sets bool flag in TaskForceController to true
        // can introduce unexpected behaviour, since TaskForceCOntroller is returned before all units are initialized
        // use TaskForceController.CheckIfReady() to prevent it
        if (slow == true)
        {
            StartCoroutine(SpawnOverFrames(preset.battleshipPrefab, position, preset.battleshipsCount, taskForce, () => battleshipsRDY(taskForce)));
            StartCoroutine(SpawnOverFrames(preset.cruiserPrefab, position, preset.cruisersCount, taskForce, () => cruisersRDY(taskForce)));
            StartCoroutine(SpawnOverFrames(preset.destroyerPrefab, position, preset.destroyersCount, taskForce, () => destroyersRDY(taskForce)));
            StartCoroutine(SpawnOverFrames(preset.frigatePrefab, position, preset.frigatesCount, taskForce, () => frigatesRDY(taskForce)));
        }

        // spawn over one frame, creates lag
        else if (slow == false)
        {
            float spawnRange = 0;

            if (preset.battleshipsCount > 1 && preset.battleshipPrefab)
            {
                AiController member = AiController.Create(position, preset.battleshipPrefab, taskForce, projectilesContainer, affiliation);
                member.transform.SetParent(unitsContainer.transform, true);
                member.gameObject.name += unitCount;
                taskForce.AddUnit(member);
                onUnitSpawned.Invoke(member);
                unitCount++;

                spawnRange = preset.battleshipsCount;
                spawnRange = Mathf.Sqrt(spawnRange);
                spawnRange *= member.Values.size;

                for (int i = 1; i < preset.battleshipsCount; i++)
                {
                    member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, spawnRange), preset.battleshipPrefab, taskForce, projectilesContainer, affiliation);
                    member.transform.SetParent(unitsContainer.transform, true);
                    member.gameObject.name += unitCount;
                    taskForce.AddUnit(member);
                    onUnitSpawned.Invoke(member);
                    unitCount++;
                }
            }

            if (preset.cruisersCount > 1 && preset.cruiserPrefab)
            {
                AiController member = AiController.Create(position, preset.cruiserPrefab, taskForce, projectilesContainer, affiliation);
                member.transform.SetParent(unitsContainer.transform, true);
                member.gameObject.name += unitCount;
                taskForce.AddUnit(member);
                onUnitSpawned.Invoke(member);
                unitCount++;

                spawnRange = preset.cruisersCount;
                spawnRange = Mathf.Sqrt(spawnRange);
                spawnRange *= member.Values.size;

                for (int i = 1; i < preset.cruisersCount; i++)
                {
                    member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, spawnRange), preset.cruiserPrefab, taskForce, projectilesContainer, affiliation);
                    member.transform.SetParent(unitsContainer.transform, true);
                    member.gameObject.name += unitCount;
                    taskForce.AddUnit(member);
                    onUnitSpawned.Invoke(member);
                    unitCount++;
                }
            }

            if (preset.destroyersCount > 1 && preset.destroyerPrefab)
            {
                AiController member = AiController.Create(position, preset.destroyerPrefab, taskForce, projectilesContainer, affiliation);
                member.transform.SetParent(unitsContainer.transform, true);
                member.gameObject.name += unitCount;
                taskForce.AddUnit(member);
                onUnitSpawned.Invoke(member);
                unitCount++;

                spawnRange = preset.destroyersCount;
                spawnRange = Mathf.Sqrt(spawnRange);
                spawnRange *= member.Values.size;

                for (int i = 1; i < preset.destroyersCount; i++)
                {
                    member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, spawnRange), preset.destroyerPrefab, taskForce, projectilesContainer, affiliation);
                    member.transform.SetParent(unitsContainer.transform, true);
                    member.gameObject.name += unitCount;
                    taskForce.AddUnit(member);
                    onUnitSpawned.Invoke(member);
                    unitCount++;
                }
            }

            if (preset.frigatesCount > 1 && preset.frigatePrefab)
            {
                AiController member = AiController.Create(position, preset.frigatePrefab, taskForce, projectilesContainer, affiliation);
                member.transform.SetParent(unitsContainer.transform, true);
                member.gameObject.name += unitCount;
                taskForce.AddUnit(member);
                onUnitSpawned.Invoke(member);
                unitCount++;

                spawnRange = preset.frigatesCount;
                spawnRange = Mathf.Sqrt(spawnRange);
                spawnRange *= member.Values.size;

                for (int i = 1; i < preset.frigatesCount; i++)
                {
                    member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, spawnRange), preset.frigatePrefab, taskForce, projectilesContainer, affiliation);
                    member.transform.SetParent(unitsContainer.transform, true);
                    member.gameObject.name += unitCount;
                    taskForce.AddUnit(member);
                    onUnitSpawned.Invoke(member);
                    unitCount++;
                }
            }
        }

        MinimapIndicator.Create(indicatorTF, taskForce);

        return taskForce;
    }

    public virtual Outpost SpawnOutpost(GameObject outpostPrefab, Vector3 position, string name)
    {
        if (affiliation == Affiliation.None)
            return null;

        Outpost outpost = Outpost.Create(position, outpostPrefab, affiliation, gameManager).GetComponent<Outpost>();
        outpost.gameObject.name = name + outpostCount;
        outpost.transform.SetParent(outpostsContainer.transform, true);
        HUDController.Create(outpost, outpostHUD, gameManager);

        MinimapIndicator.Create(inidcatorOutp, outpost);
        onOutpostSpawned.Invoke(outpost);

        return outpost;
    }

    public void AddLineRenderer(GameObject obj, Material rendererMaterial)
    {
        if (!obj.TryGetComponent(out LineRenderer lineRenderer))
        {
            lineRenderer = obj.AddComponent<LineRenderer>();
            lineRenderer.material = rendererMaterial;
        }  

    }
}
