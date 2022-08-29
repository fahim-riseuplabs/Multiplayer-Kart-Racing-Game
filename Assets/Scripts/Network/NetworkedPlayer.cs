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
            GameObject playerName = Instantiate(playerNamePrefab);
            playerName.GetComponent<NameUIController>().target = carRigidbody.transform;

            string sentName = null;

            if (photonView.InstantiationData != null)
            {
                sentName = (string) photonView.InstantiationData[0];
            }

            if (sentName != null)
            {
                playerName.GetComponent<TextMeshProUGUI>().text = sentName;
            }
            else
            {
                playerName.GetComponent<TextMeshProUGUI>().text = photonView.Owner.NickName;
            }

            playerName.GetComponent<NameUIController>().carRend = carRenderer;
        }
    }
}
