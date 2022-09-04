using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;

public class RaceMonitor : MonoBehaviourPunCallbacks
{
    public GameObject[] countDownImages;

    public static bool isStartedRacing = false;
    public static int totalLaps = 1;

    public int playerSelectedCarIndex;
    public int spwanIndex;

    public GameObject panelHUD;
    public GameObject gameOverPanel;
    //public GameObject quitButton;

    public GameObject[] carPrefabs;

    public GameObject startButton;

    public GameObject waitingText;

    public GameObject mobileControllerUI;

    [HideInInspector] public CheckPointManager[] checkPointManagers;
    [HideInInspector] public GameObject[] cars;

    private GameObject[] spawnPoints;
    private GameObject playerCar = null;

    private Vector3 startPos;
    private Quaternion startRot;


    // Start is called before the first frame update
    void Start()
    {
       if(Application.platform == RuntimePlatform.Android)
        {
            mobileControllerUI.SetActive(true);
        }
        else
        {
            mobileControllerUI.SetActive(false);
        }

        isStartedRacing = false;

        spawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");

        foreach (GameObject countDownImage in countDownImages)
        {
            countDownImage.SetActive(false);
        }

        gameOverPanel.SetActive(false);

        startButton.SetActive(false);
        waitingText.SetActive(false);

        playerSelectedCarIndex = PlayerPrefs.GetInt("PlayerCarIndex", 0);
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length - 1);
        startPos = spawnPoints[randomSpawnPointIndex].transform.position;
        startRot = spawnPoints[randomSpawnPointIndex].transform.rotation;


        if (PhotonNetwork.IsConnected)
        {
            //quitButton.SetActive(true);

            startPos = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].transform.position;
            startRot = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].transform.rotation;

            if (NetworkedPlayer.localPlayerInstance == null)
            {
                playerCar = PhotonNetwork.Instantiate(carPrefabs[playerSelectedCarIndex++].name, startPos, startRot, 0);
            }

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
            //quitButton.SetActive(true);

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

    private void RaceBegin()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        string[] nameNPC = { "Ratul", "Rafiq", "Masud", "Emraan", "Ovi", "Sunny", "Sumon" };
        int numAIPlayers = PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;

        for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            int random = Random.Range(0, carPrefabs.Length);
            object[] intanceData = new object[1];
            intanceData[0] = (string)nameNPC[Random.Range(0, nameNPC.Length)];

            GameObject AIcar = PhotonNetwork.Instantiate(carPrefabs[random].name, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation, 0, intanceData);
            AIcar.GetComponent<AIControllerWithTracker>().enabled = true;
            AIcar.GetComponent<Drive>().enabled = true;
            AIcar.GetComponent<Drive>().networkName = (string)intanceData[0];
            AIcar.GetComponent<PlayerController>().enabled = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            photonView.RPC("StartRace", RpcTarget.All, null);
        }
    }

    [PunRPC]
    private void StartRace()
    {
        waitingText.SetActive(false);
        StartCoroutine(PlayCountDownAnimation());
        startButton.SetActive(false);

        cars = GameObject.FindGameObjectsWithTag("car");

        checkPointManagers = new CheckPointManager[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            checkPointManagers[i] = cars[i].GetComponent<CheckPointManager>();
        }
    }

    [PunRPC]
    private void Restart()
    {
        PhotonNetwork.LoadLevel("Track1");
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

        //quitButton.SetActive(true);
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
            //quitButton.SetActive(false);
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
            return;
        }

        //int index = 0;

        //foreach(Player player in PhotonNetwork.PlayerList)
        //{
        //    if (player == PhotonNetwork.LocalPlayer)
        //    {

        //        playerCar.SetActive(false);
        //        playerCar.transform.position = spawnPoints[index].transform.position;
        //        playerCar.transform.rotation = spawnPoints[index].transform.rotation;
        //        playerCar.SetActive(true);
        //        break;
        //    }

        //    index++;
        //}
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient && !isStartedRacing)
        {
            waitingText.SetActive(false);
            startButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
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
