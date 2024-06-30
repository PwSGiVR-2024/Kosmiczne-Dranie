using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    private bool initialized = false;
    private SpriteRenderer spriteRenderer;

    public bool isStatic = false;
    public MonoBehaviour owner;
    public Transform follow;

    public Vector3 offset = new Vector3(0, 450, 0);

    [Header("Site settings")]
    public Color defaultColor;
    public Color allyColor;
    public Color enemyColor;

    public static MinimapIndicator Create(GameObject prefab, MonoBehaviour owner)
    {
        MinimapIndicator indicator = Instantiate(prefab).GetComponent<MinimapIndicator>();
        indicator.Init(owner);

        return indicator;
    }

    private void ChangeSiteColor(SiteController site)
    {
        Debug.Log("change");

        if (site.currentController == Affiliation.Blue)
            spriteRenderer.color = allyColor;

        else if (site.currentController == Affiliation.Red)
            spriteRenderer.color = enemyColor;

        else spriteRenderer.color = defaultColor;
    }

    private void Start()
    {
        if (owner && !initialized) Init(owner);
    }

    public void Init(MonoBehaviour owner)
    {
        this.owner = owner;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (owner is TaskForceController taskForce)
        {
            follow = taskForce.Commander.transform;
            taskForce.onTaskForceDestroyed.AddListener((_) => Destroy(gameObject));
            isStatic = false;
        }

        else if (owner is Outpost outpost)
        {
            follow = outpost.transform;
            outpost.onOutpostDestroy.AddListener(() => Destroy(gameObject));
            isStatic = false;
        }

        else if (owner is SiteController site)
        {
            follow = site.transform;
            site.onCaptured.AddListener(() => ChangeSiteColor(site));
            isStatic = true;
        }
            
        else if (owner is Headquarters head)
        {
            follow = head.transform;
            isStatic = true;
        }

        else if (owner is MapMovement movement)
        {
            follow = movement.transform;
            isStatic = false;
        }

        initialized = true;
    }

    private void Update()
    {
        if (isStatic)
            return;

        if (follow == null && owner is TaskForceController taskForce && taskForce.Commander != null)
            follow = taskForce.Commander.transform;

        else if (follow == null)
            return;

        transform.position = follow.position + offset;
    }
}
