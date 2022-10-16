using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAchievementTrigger : MonoBehaviour
{
    private AchievementManager _achievementManager;
    // Start is called before the first frame update
    void Start()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
    }

    //Achievements #19 and #20
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        _achievementManager.Achievements.Find(achi => achi.Title == "Proud Ninja").Eligible = false;

        CounterAchievement cAchi = _achievementManager.Achievements.Find(achi => achi.Title == "Resourceful Ninja") as CounterAchievement;
        cAchi.Counter++;

        if (cAchi.Counter >= 50)
            _achievementManager.ObtainAchievement("Resourceful Ninja");
    }
}
