using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LaunchManager : MonoBehaviour
{
    public TMP_InputField PlayerName;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerName.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SetName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }

    public void ConnectSingle()
    {
        SceneManager.LoadScene("Track1");
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
