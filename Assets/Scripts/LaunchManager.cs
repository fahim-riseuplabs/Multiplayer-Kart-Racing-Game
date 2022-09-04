using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class LaunchManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerName;
    public TextMeshProUGUI feedBackText;

    private byte maxPlayersPerRoom = 4;
    private bool isConnecting;
    private string gameVersion = "1";

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void ConnectNetwork()
    {
        feedBackText.text = "";
        isConnecting = true;

        PhotonNetwork.NickName = playerName.text;

        if (PhotonNetwork.IsConnected)
        {
            feedBackText.text += "\nJoining Room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            feedBackText.text += "\nConnecting...";
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void SetName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }

    public void ConnectSingle()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            SceneManager.LoadScene("Track1");
        }
    }

    ////////////////////////////// Pun Callbacks

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            feedBackText.text += "\nOnConnectedToMaster";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        feedBackText.text += "\nFailed to join random room";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Track1");

        feedBackText.text += "\nDisconnected because " + cause;
        isConnecting = false;
    }

    public override void OnJoinedRoom()
    {
        feedBackText.text += "\nJoined Room With " + PhotonNetwork.CurrentRoom.PlayerCount + " players";
        PhotonNetwork.LoadLevel("Track1");
    }

    public void OnClickButtonFunction_Quit()
    {
        Application.Quit();
    }
}
