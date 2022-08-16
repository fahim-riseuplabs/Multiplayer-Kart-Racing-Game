using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceMonitor : MonoBehaviour
{
    public GameObject[] countDownImages;

    public static bool isStartedRacing = false;
    public static int totalLaps = 1;

    public int playerSelectedCarIndex;

    public GameObject panelHUD;
    public GameObject gameOverPanel;

    public GameObject[] carPrefabs;

    [HideInInspector] public CheckPointManager[] checkPointManagers;
    [HideInInspector] public GameObject[] cars;

    private GameObject[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");

        playerSelectedCarIndex = PlayerPrefs.GetInt("PlayerCarIndex",0);

        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length - 1);

        GameObject playerCar = Instantiate(carPrefabs[playerSelectedCarIndex]);

        playerCar.GetComponent<AIControllerWithTracker>().enabled = false;
        playerCar.GetComponent<PlayerController>().enabled = true;

        playerCar.transform.position = spawnPoints[randomSpawnPointIndex].transform.position;
        playerCar.transform.rotation = spawnPoints[randomSpawnPointIndex].transform.rotation;

        SmoothFollow.playerCar = playerCar.transform;

        foreach (GameObject spawnPoint in spawnPoints)
        {
            if (spawnPoint != spawnPoints[randomSpawnPointIndex])
            {
                GameObject car = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length - 1)]);

                car.transform.position = spawnPoint.transform.position;
                car.transform.rotation = spawnPoint.transform.rotation;
            }
        }

        foreach(GameObject countDownImage in countDownImages)
        {
            countDownImage.SetActive(false);
        }

        gameOverPanel.SetActive(false);

        cars = GameObject.FindGameObjectsWithTag("car");

        checkPointManagers = new CheckPointManager[cars.Length];

        for (int i=0;i<cars.Length;i++)
        {
            checkPointManagers[i] = cars[i].GetComponent<CheckPointManager>();
        }

        StartCoroutine(PlayCountDownAnimation());
    }
    
    IEnumerator PlayCountDownAnimation()
    {
        yield return new WaitForSeconds(2f);

        foreach (GameObject countDownImage in countDownImages)
        {
            countDownImage.SetActive(true);
            yield return new WaitForSeconds(1f);
            countDownImage.SetActive(false);
        }

        isStartedRacing = true;
    }

    private void LateUpdate()
    {
        int finishedCount = 0;

       foreach(CheckPointManager checkPointManager in checkPointManagers)
        {
            if(checkPointManager.lap == totalLaps + 1)
            {
                finishedCount++;
            }
        }


       if(finishedCount == cars.Length)
        {
            print("GameOver");
            panelHUD.SetActive(false);
            gameOverPanel.SetActive(true);
        }
    }

    public void OnClickButtonFunction_Restart()
    {
        isStartedRacing = false;
        SceneManager.LoadScene("Track1");
    }
}
