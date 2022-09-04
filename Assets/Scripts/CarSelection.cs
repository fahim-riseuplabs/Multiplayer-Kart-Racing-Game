using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelection : MonoBehaviour
{
    public GameObject[] cars;
    public Image mainMenuBG;

    public Color[] mainMenuColorsBG;

    private int currentCarIndex ;
    private Quaternion direction;

    // Start is called before the first frame update
    void Start()
    {
        currentCarIndex = PlayerPrefs.GetInt("PlayerCarIndex",0);

        mainMenuBG.color = mainMenuColorsBG[currentCarIndex];

        this.transform.LookAt(cars[currentCarIndex].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LeftCarSelection();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RightCarSelection();
        }

        direction = Quaternion.LookRotation(cars[currentCarIndex].transform.position - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, direction, Time.deltaTime*2);
        mainMenuBG.color = Color.Lerp(mainMenuBG.color, mainMenuColorsBG[currentCarIndex],Time.deltaTime*2);
    }

    private void LeftCarSelection()
    {
        currentCarIndex++;
        if (currentCarIndex > cars.Length - 1)
        {
            currentCarIndex = 0;
        }

        PlayerPrefs.SetInt("PlayerCarIndex", currentCarIndex);
    }

    private void RightCarSelection()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = cars.Length - 1;
        }

        PlayerPrefs.SetInt("PlayerCarIndex", currentCarIndex);
    }

    public void OnClickButtonFunction_LeftCarSelection()
    {
        LeftCarSelection();
    }

    public void OnClickButtonFunction_RightCarSelection()
    {
        RightCarSelection();
    }
}
