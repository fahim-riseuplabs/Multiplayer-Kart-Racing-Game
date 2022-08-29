using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameUIController : MonoBehaviour
{
    [HideInInspector] public TextMeshProUGUI nameText;
    [HideInInspector] public Transform target;
    [HideInInspector] public Renderer carRend;

    public TextMeshProUGUI lapDetailsDisplay;

    private CanvasGroup canvasGroup;
    private bool carInView;
    private CheckPointManager checkPointManager;

    private int carRego;
    private string position;
    private bool isCarRego = false;

    // Start is called before the first frame update
    void Awake()
    {
        transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        nameText = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
    }


    private void Start()
    {
        checkPointManager = target.GetComponent<CheckPointManager>();
        carRego = Leaderboard.RegisterCar(nameText.text);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (carRend == null)
        {
            return;
        }

        if (!isCarRego)
        {
            carRego = Leaderboard.RegisterCar(nameText.text);
            isCarRego = true;
            return;
        }
     
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        carInView = GeometryUtility.TestPlanesAABB(planes, carRend.bounds);
        
        canvasGroup.alpha = carInView ? 1 : 0;

        transform.position = Camera.main.WorldToScreenPoint(target.position+target.up*2);

        Leaderboard.SetPosition(carRego, checkPointManager.lap, checkPointManager.checkPoint,checkPointManager.enteredTime);
        position = Leaderboard.GetPosition(carRego);

        if (checkPointManager != null)
        {
            lapDetailsDisplay.text = $" {position} ";
        }
    }
}
