using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    // generalnie to jest przyczepione do prefaba
    // mo¿liwe ¿e to bêdzie musia³o byæ childem jakiegoœ canvas, jeœli tak daj znaæ

    public bool isStatic = false;
    public MonoBehaviour owner;
    public Transform follow;

    // mo¿na daæ offset ¿eby jak bêdzie aktualizowaæ pozycjê to wy¿ej ni¿ to co followuje
    public Vector3 offset = new Vector3(0, 250, 0);

    // to jest inicjalizowane kiedy task force albo coœ siê spawnuje
    public static MinimapIndicator Create(GameObject prefab, MonoBehaviour owner)
    {
        MinimapIndicator indicator = Instantiate(prefab).GetComponent<MinimapIndicator>();
        indicator.Init(owner);

        return indicator;
    }
    
    public void Init(MonoBehaviour owner)
    {
        this.owner = owner;

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
            isStatic = true;
        }
            

        else if (owner is Headquarters head)
        {
            follow = head.transform;
            isStatic = true;
        }
            

    }

    private void Update()
    {
        // niektóre obiekty takie jak site s¹ statyczne
        if (isStatic)
            return;

        if (follow == null)
            return;

        // trzeba dodaæ warunek, bo jak rozwali dowódcê to mo¿e w jakiejœ klatce nie mieæ czego followowaæ
        if (follow == null && owner is TaskForceController taskForce && taskForce.Commander != null)
            follow = taskForce.Commander.transform;




        // tutaj to coœ musi mieæ aktualizowan¹ pozycjê w ka¿dej klatce, na podstawie Transform follow
        transform.position = follow.position + offset;

    }
}
