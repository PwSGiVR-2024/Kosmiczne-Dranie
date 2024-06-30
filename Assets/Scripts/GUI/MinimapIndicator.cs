using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    // generalnie to jest przyczepione do prefaba
    // mo�liwe �e to b�dzie musia�o by� childem jakiego� canvas, je�li tak daj zna�

    public bool isStatic = false;
    public MonoBehaviour owner;
    public Transform follow;

    // mo�na da� offset �eby jak b�dzie aktualizowa� pozycj� to wy�ej ni� to co followuje
    public Vector3 offset = new Vector3(0, 250, 0);

    // to jest inicjalizowane kiedy task force albo co� si� spawnuje
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
        // niekt�re obiekty takie jak site s� statyczne
        if (isStatic)
            return;

        if (follow == null)
            return;

        // trzeba doda� warunek, bo jak rozwali dow�dc� to mo�e w jakiej� klatce nie mie� czego followowa�
        if (follow == null && owner is TaskForceController taskForce && taskForce.Commander != null)
            follow = taskForce.Commander.transform;




        // tutaj to co� musi mie� aktualizowan� pozycj� w ka�dej klatce, na podstawie Transform follow
        transform.position = follow.position + offset;

    }
}
