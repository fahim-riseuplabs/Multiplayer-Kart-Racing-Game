using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkedPlayer : MonoBehaviourPunCallbacks
{
    public static GameObject localPlayerInstance;

    public GameObject playerNamePrefab;
    public Rigidbody carRigidbody;
    public Renderer carRenderer;
}
