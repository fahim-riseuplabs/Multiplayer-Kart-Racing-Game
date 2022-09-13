using Photon.Pun;
using TMPro;
using UnityEngine;

public class NetworkedPlayer : MonoBehaviourPunCallbacks
{
    public static GameObject localPlayerInstance;

    public GameObject playerNamePrefab;
    public Rigidbody carRigidbody;
    public Renderer carRenderer;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            localPlayerInstance = this.gameObject;
        }
        else
        {
           
            string sentName = null;

            if (photonView.InstantiationData != null)
            {
                sentName = (string)photonView.InstantiationData[0];
            }

            if (sentName != null)
            {
                GetComponent<Drive>().networkName = sentName;
            }
            else
            {
                GameObject playerName = Instantiate(playerNamePrefab);

                string playerNameUI = photonView.Owner.NickName + photonView.Owner.ActorNumber;

                playerName.transform.name = playerNameUI;

                playerName.GetComponent<NameUIController>().target = carRigidbody.transform;
                playerName.GetComponent<TextMeshProUGUI>().text = photonView.Owner.NickName;
                playerName.GetComponent<NameUIController>().carRend = carRenderer;
            }
        }
    }
}
