using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SiteHUDController : MonoBehaviour
{
    public Vector3 hudOffset;
    public Vector3 indicatorOffset;

    private RectTransform rectTransform;

    public Color colorMainAlly;
    public Color colorBackgroundAlly;
    public Color colorMainEnemy;
    public Color colorBackgroundEnemy;


    public LineRenderer lineRenderer;
    public Material ringDefault;
    public Material ringEnemy;
    public Material ringAlly;


    public SiteController siteController;

    public Image icon;
    public Image indicator;
    public Image captureBar;
    public Image captureBarBackground;
    public Image background;

    public GameObject additionalInfo;
    public TMP_Text resourceMetals;
    public TMP_Text resourceCrystals;
    public TMP_Text resourceCredits;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }


    public void Init(SiteController site)
    {
        siteController = site;
        site.onAttacked.AddListener(StartCaptureAnimation);
        site.onCaptured.AddListener(SetCapturerColors);
        rectTransform.position = site.transform.position + hudOffset;
        indicator.rectTransform.position = site.transform.position + indicatorOffset;

        foreach (var resource in siteController.resources)
        {
            switch (resource.resourceType)
            {
                case ResourceHolder.ResourceType.Metals:
                    resourceMetals.text = "Metals: " + resource.value;
                    break;

                case ResourceHolder.ResourceType.Crystals:
                    resourceCrystals.text = "Crystals: " + resource.value;
                    break;

                case ResourceHolder.ResourceType.Credits:
                    resourceCredits.text = "Credits: " + resource.value;
                    break;
            }
        }

        if (siteController.gameObject.TryGetComponent<LineRenderer>(out lineRenderer))
            lineRenderer.material = ringDefault;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
        float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        rectTransform.localScale = new Vector3(1, 1, 1) * cameraDistance * 0.0005f;
        indicator.rectTransform.position = siteController.transform.position + indicatorOffset;
        indicator.rectTransform.rotation = Quaternion.Euler(90, 0, 0);
    }

    private void StartCaptureAnimation()
    {
        StartCoroutine(ProgressBarAnim());
    }

    private IEnumerator ProgressBarAnim()
    {
        float elapsedTime = 0;
        
        switch (siteController.attacker)
        {
            case Affiliation.Blue:
                captureBar.color = colorMainAlly;
                break;

            case Affiliation.Red:
                captureBar.color = colorMainEnemy;
                break;
        }

        while (true)
        {
            if (siteController.timeLeft == siteController.captureTime && !siteController.capturing)
            {
                captureBar.fillAmount = 0;
                break;
            }

            if (elapsedTime == 0)
            {
                captureBar.fillAmount = 0;
                yield return null;
            }

            else if (elapsedTime >= siteController.captureTime)
            {
                captureBar.fillAmount = 1;
                break;
            }

            // timeLeft progress in siteController may be blocked, so changing elapsed time only when there is a difference
            if (elapsedTime + siteController.captureTime - siteController.timeLeft != elapsedTime)
                elapsedTime = siteController.captureTime - siteController.timeLeft;

            captureBar.fillAmount = elapsedTime / siteController.captureTime;

            yield return null;
        }
    }

    private void SetCapturerColors()
    {
        StopAllCoroutines();

        switch (siteController.currentController)
        {
            case Affiliation.Blue:
                icon.color = colorMainAlly;
                indicator.color = colorMainAlly;
                captureBar.color = colorMainAlly;
                background.color = colorBackgroundAlly;
                captureBarBackground.color = colorMainAlly;
                lineRenderer.material = ringAlly;
                break;

            case Affiliation.Red:
                icon.color = colorMainEnemy;
                indicator.color = colorMainEnemy;
                captureBar.color = colorMainEnemy;
                background.color = colorBackgroundEnemy;
                captureBarBackground.color = colorMainEnemy;
                lineRenderer.material = ringEnemy;
                break;
        }
    }

    public void OnPointerEnter(BaseEventData data)
    {
        additionalInfo.SetActive(true);
    }

    public void OnPointerExit(BaseEventData data)
    {
        additionalInfo.SetActive(false);
    }
}
