using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    public TextMeshProUGUI name1stPlace;
    public TextMeshProUGUI name2ndPlace;
    public TextMeshProUGUI name3rdPlace;
    public TextMeshProUGUI name4thPlace;

    [HideInInspector] public List<string> names;

    private void Start()
    {
        Leaderboard.Reset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        names = Leaderboard.GetName();

        name1stPlace.text = names[0];
        name2ndPlace.text = names[1];
        name3rdPlace.text = names[2];
        name4thPlace.text = names[3];
    }
}
