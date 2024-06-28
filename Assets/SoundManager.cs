using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SoundManager : MonoBehaviour
{
    public float playCooldown = 0.1f;
    public float pitchMaxDeviation = 0.05f;
    public float volumeMaxDeviation = 0.1f;

    private float cooldownRemaining = 0.0f;
    private bool onCooldown = false;

    public void PlaySoundEffect(AudioSource source)
    {
        if (onCooldown || source == null) return;

        float cachePitch = source.pitch;
        float cacheVolume = source.volume;

        source.pitch = UnityEngine.Random.Range(cachePitch - (cachePitch * pitchMaxDeviation), cachePitch + (cachePitch * pitchMaxDeviation));
        source.volume = UnityEngine.Random.Range(cacheVolume - (cacheVolume * volumeMaxDeviation), cacheVolume + (cacheVolume * volumeMaxDeviation));

        source.Play();

        source.pitch = cachePitch;
        source.volume = cacheVolume;

        onCooldown = true;
        cooldownRemaining = playCooldown;
    }

    private void Update()
    {
        if (onCooldown)
            cooldownRemaining -= Time.deltaTime;

        if (cooldownRemaining <= 0)
            onCooldown = false;
    }
}
