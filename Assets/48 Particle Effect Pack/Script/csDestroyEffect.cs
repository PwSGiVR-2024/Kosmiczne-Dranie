using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour {
    public float lifespan;
	
	void Update()
    {
        lifespan -= Time.deltaTime;
        if (lifespan <= 0)
            gameObject.SetActive(false);
    }
}
