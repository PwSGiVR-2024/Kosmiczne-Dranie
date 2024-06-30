using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EconomyPanelController : MonoBehaviour
{
    public PlayerResourceManager resources;
    public TMP_Text metalsValue;
    public TMP_Text creditsValue;
    public TMP_Text crystalsValue;
    public TMP_Text metalsGather;
    public TMP_Text crystalsGather;
    public TMP_Text creditsGather;

    public Color positiveBalance;
    public Color negativeBalance;

    private void Start()
    {
        resources.onUpdate.AddListener(UpdateValues);
    }

    private void UpdateValues()
    {
        metalsValue.text = resources.Metals.ToString();
        crystalsValue.text = resources.Crystals.ToString();
        creditsValue.text = resources.Credits.ToString();

        metalsGather.text = $"(+{resources.metalsValue})";
        crystalsGather.text = $"(+{resources.crystalsValue})";

        if (resources.creditsValue - resources.Maintenance < 0)
        {
            creditsGather.color = negativeBalance;
            creditsGather.text = $"({resources.creditsValue - resources.Maintenance})";
        }

        else
        {
            creditsGather.color = positiveBalance;
            creditsGather.text = $"(+{resources.creditsValue - resources.Maintenance})";
        }
    }
}
