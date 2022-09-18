using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceMonitor : MonoBehaviourPunCallbacks
{
    public GameObject[] countDownImages;

    public static bool isStartedRacing = false;
    public static bool isCountDownStarter = false;

    public static int totalLaps = 2;

    public int playerSelectedCarIndex;
    public int spwanIndex;

    public GameObject panelHUD;
    public GameObject gameOverPanel;

    public GameObject[] carPrefabs;
    public string[] carAIPrefabsName;

    public GameObject startButton;

    public GameObject waitingText;

    public GameObject mobileControllerUI;

    [HideInInspector] public CheckPointManager[] checkPointManagers;
    [HideInInspector] public GameObject[] cars;

    private GameObject[] spawnPoints;
    private GameObject playerCar = null;

    private Vector3 startPos;
    private Quaternion startRot;

    private SmoothFollow smoothFollow;

    // Start is called before the first frame update
    void Start()
    {

        smoothFollow = FindObjectOfType<SmoothFollow>();

        if (Application.platform == RuntimePlatform.Android)
        {
            mobileControllerUI.SetActive(true);
        }
        else
        {
            mobileControllerUI.SetActive(false);
        }

        isStartedRacing = false;
        isCountDownStarter = false;

        spawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");

        foreach (GameObject countDownImage in countDownImages)
        {
            countDownImage.SetActive(false);
        }

        gameOverPanel.SetActive(false);

        startButton.SetActive(false);
        waitingText.SetActive(false);

        playerSelectedCarIndex = PlayerPrefs.GetInt("PlayerCarIndex", 0);

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startButton.SetActive(true);
            }
            else
            {
                waitingText.SetActive(true);
            }
        }
        else
        {
            int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length - 1);
            startPos = spawnPoints[randomSpawnPointIndex].transform.position;
            startRot = spawnPoints[randomSpawnPointIndex].transform.rotation;

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

            playerCar.GetComponent<AIControllerWithTracker>().enabled = false;
            playerCar.GetComponent<Drive>().enabled = true;
            playerCar.GetComponent<PlayerController>().enabled = true;

            smoothFollow.playerCar = playerCar.transform;
        }
    }

    private void RaceBegin()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("Instantiate", RpcTarget.All, null);


            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            string[] nameNPC = { "Ratul", "Rafiq", "Masud", "Emraan", "Ovi", "Sunny", "Sumon" };
            int numAIPlayers = PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;

            for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                int random = Random.Range(0, carPrefabs.Length);
                object[] intanceData = new object[1];
                intanceData[0] = (string)nameNPC[Random.Range(0, nameNPC.Length)];

                GameObject AIcar = PhotonNetwork.InstantiateRoomObject(carAIPrefabsName[random], spawnPoints[i].transform.position, spawnPoints[i].transform.rotation, 0, intanceData);
                AIcar.GetComponent<Drive>().networkName = (string)intanceData[0];
            }


            photonView.RPC("StartRace", RpcTarget.All, null);
        }
    }

    [PunRPC]
    private void Instantiate()
    {
        int index = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer)
            {
                startPos = spawnPoints[index].transform.position;
                startRot = spawnPoints[index].transform.rotation;

                if (NetworkedPlayer.localPlayerInstance == null)
                {
                    playerCar = PhotonNetwork.Instantiate(carPrefabs[playerSelectedCarIndex++].name, startPos, startRot, 0);
                }

                playerCar.GetComponent<AIControllerWithTracker>().enabled = false;
                playerCar.GetComponent<Drive>().enabled = true;
                playerCar.GetComponent<PlayerController>().enabled = true;

                smoothFollow.playerCar = playerCar.transform;

                break;
            }

            index++;
        }
    }

    [PunRPC]
    private void StartRace()
    {
        waitingText.SetActive(false);

        StartCoroutine(PlayCountDownAnimation());

        startButton.SetActive(false);
    }

    [PunRPC]
    private void Restart()
    {
        PhotonNetwork.LoadLevel("Track1");
    }

    private void GetCheckPointManagers()
    {
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

        GetCheckPointManagers();

        isCountDownStarter = true;

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && !isStartedRacing)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
            }
        }

        if (isStartedRacing)
        {
            smoothFollow.FindCars();
        }

        string playerNameUI = otherPlayer.NickName + otherPlayer.ActorNumber;

        Destroy(GameObject.Find(playerNameUI));

        GetCheckPointManagers();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && !isCountDownStarter)
        {
            waitingText.SetActive(false);
            startButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        PlayerPrefs.SetString("PlayerName", PhotonNetwork.LocalPlayer.NickName);
        PhotonNetwork.LoadLevel("MainMenu");

    }

    public void OnClickButtonFunction_Restart()
    {
        isStartedRacing = false;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("Restart", RpcTarget.All, null);
        }
        else
        {
            SceneManager.LoadScene("Track1");
        }
    }

    public void OnClickButtonFunction_StartRace()
    {
        RaceBegin();
    }

    public void OnClickButtonFunction_MainMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }


}
