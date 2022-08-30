using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    private CanvasGroup CanvasGroup;
    private float HUDAlphaSettings = 1;

    // Start is called before the first frame update
    void Start()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        CanvasGroup.alpha = 0;

        if (PlayerPrefs.HasKey("HUDApha"))
        {
            HUDAlphaSettings = PlayerPrefs.GetFloat("HUDAlpha");
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            HUD();
        }

        if (RaceMonitor.isStartedRacing)
        {
            CanvasGroup.alpha = HUDAlphaSettings;
        }

    }

    private void HUD()
    {

        if (HUDAlphaSettings == 1)
        {
            HUDAlphaSettings = 0;
        }
        else
        {
            HUDAlphaSettings = 1;
        }

        PlayerPrefs.SetFloat("HUDAlpha", HUDAlphaSettings);
    }

    public void OnClickButtonFunction_HUD()
    {
        HUD();
    }
}
