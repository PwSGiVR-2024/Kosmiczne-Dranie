using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SiteController : MonoBehaviour
{
    private Collider[] checkerArray = new Collider[1];

    public int range;
    public bool capturing = false;
    public float captureTime = 5.0f;
    public float timeLeft = 5.0f;
    public LayerMask contendersMask;
    public LayerMask attackerMask;
    public Affiliation currentController = Affiliation.None;
    public Affiliation attacker = Affiliation.None;
    public Affiliation previousControler = Affiliation.None;

    public ResourceHolder[] resources;

    public UnityEvent onCaptured = new();
    public UnityEvent onAttacked = new();

    void Start()
    {
        timeLeft = captureTime;
    }

    void Update()
    {
        GameUtils.DrawCircle(gameObject, range, transform);

        if (CheckForNewCapturers(out AiController capturer))
            StartCoroutine(StartCapturing(capturer));
    }

    private bool CheckForNewCapturers(out AiController capturer)
    {
        if (capturing)
        {
            capturer = null;
            return false;
        }

        // writes first collider that entered the range, to the array
        int colliders = Physics.OverlapSphereNonAlloc(transform.position, range, checkerArray, contendersMask);

        // returns true if written collider is AiController and is not controller of this site
        if (colliders > 0)
        {
            if (checkerArray[0].TryGetComponent(out AiController controller))
            {
                if (controller.Affiliation == currentController)
                {
                    capturer = null;
                    return false;
                }

                attacker = controller.UnitTaskForce.Affiliation;
                capturer = controller;
                return true;
            }
        }

        capturer = null;
        return false;
    }

    private IEnumerator StartCapturing(AiController capturer)
    {
        capturing = true;
        timeLeft = captureTime;
        attackerMask = LayerMask.GetMask(LayerMask.LayerToName(capturer.gameObject.layer));
        attacker = capturer.Affiliation;
        onAttacked.Invoke();

        while (true)
        {
            // checks conditions in every frame
            // if returns false, then values are reset
            // if true, continues counting
            // if timer is out, then site is captured
            if (CheckConditions())
                timeLeft -= Time.deltaTime;
            else
            {
                // reset values
                capturing = false;
                attackerMask = 0;
                attacker = Affiliation.None;
                timeLeft = captureTime;
                yield break;
            }

            if (timeLeft <= 0)
            {
                // reset values and set controller
                previousControler = currentController;
                currentController = attacker;
                capturing = false;
                attackerMask = 0;
                attacker = Affiliation.None;
                timeLeft = captureTime;
                onCaptured.Invoke();

                foreach (var resource in resources)
                {
                    resource.onCapture.Invoke(currentController);
                }

                yield break;
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    private bool CheckConditions()
    {
        // scans for colliders with layer different than current attacker
        // if at least one collider is written to checkerArray, then conditions for capturing are false
        // hence the size of 1
        int enemyColliders = Physics.OverlapSphereNonAlloc(transform.position, range, checkerArray, GameUtils.SubtractLayerMasks(contendersMask, attackerMask));

        if (enemyColliders > 0)
            return false;

        // scans for ally colliders
        // to capture the site, there must be uninterrupted presence of at least one ally collider
        int allyColliders = Physics.OverlapSphereNonAlloc(transform.position, range, checkerArray, attackerMask);

        if (allyColliders < 1)
            return false;

        return true;
    }
}
