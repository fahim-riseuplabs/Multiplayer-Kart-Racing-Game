using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceMonitor : MonoBehaviourPunCallbacks
{
    public GameObject[] countDownImages;

    public static bool isStartedRacing = false;
    public static int totalLaps = 1;

    public int playerSelectedCarIndex;

    public GameObject panelHUD;
    public GameObject gameOverPanel;

    public GameObject[] carPrefabs;

    public GameObject startButton;

    [HideInInspector] public CheckPointManager[] checkPointManagers;
    [HideInInspector] public GameObject[] cars;

    private GameObject[] spawnPoints;
    private GameObject playerCar = null;

    private Vector3 startPos;
    private Quaternion startRot;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");

        foreach (GameObject countDownImage in countDownImages)
        {
            countDownImage.SetActive(false);
        }

        gameOverPanel.SetActive(false);

        startButton.SetActive(false);

        playerSelectedCarIndex = PlayerPrefs.GetInt("PlayerCarIndex", 0);
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length - 1);
        startPos = spawnPoints[randomSpawnPointIndex].transform.position;
        startRot = spawnPoints[randomSpawnPointIndex].transform.rotation;


        if (PhotonNetwork.IsConnected)
        {
            startPos = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.position;
            startRot = spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.rotation;

            if (NetworkedPlayer.localPlayerInstance == null)
            {
                playerCar = PhotonNetwork.Instantiate(carPrefabs[playerSelectedCarIndex].name, startPos, startRot, 0);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                startButton.SetActive(true);
            }
        }
        else
        {
            playerCar = Instantiate(carPrefabs[playerSelectedCarIndex]);

            
            playerCar.transform.position = startPos;
            playerCar.transform.rotation = startRot;

            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (spawnPoint != spawnPoints[randomSpawnPointIndex])
                {
                    GameObject car = Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length - 1)]);

                    car.transform.position = spawnPoint.transform.position;
                    car.transform.rotation = spawnPoint.transform.rotation;
                }
            }

            StartRace();
        }

        playerCar.GetComponent<AIControllerWithTracker>().enabled = false;
        playerCar.GetComponent<Drive>().enabled = true;
        playerCar.GetComponent<PlayerController>().enabled = true;

        SmoothFollow.playerCar = playerCar.transform;
    }

    private void StartRace()
    {
        StartCoroutine(PlayCountDownAnimation());
        startButton.SetActive(false);

        cars = GameObject.FindGameObjectsWithTag("car");

        checkPointManagers = new CheckPointManager[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            checkPointManagers[i] = cars[i].GetComponent<CheckPointManager>();
        }
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
        if (!isStartedRacing)
        {
            return;
        }

        int finishedCount = 0;

        foreach (CheckPointManager checkPointManager in checkPointManagers)
        {
            if (checkPointManager.lap == totalLaps + 1)
            {
                finishedCount++;
            }
        }

        if (finishedCount == cars.Length)
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

    public void OnCliclButtonFunction_StartRace()
    {
        StartRace();
    }
}
