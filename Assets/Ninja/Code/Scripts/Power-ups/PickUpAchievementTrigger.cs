using UnityEngine;

public class PickUpAchievementTrigger : MonoBehaviour
{
    #region Private Fields

    private AchievementManager _achievementManager;

    #endregion
    
    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        // Achievement 19 and 20
        Achievement proudAchievement =
            _achievementManager.Achievements.Find(achievement => achievement.Title == "Proud Ninja");

        if (proudAchievement == null)
            return;

        proudAchievement.Eligible = false;

        if (_achievementManager.Achievements.Find(achievement => achievement.Title == "Resourceful Ninja") is not
            CounterAchievement counterAchievement)
            return;
        
        counterAchievement.Counter++;

        if (counterAchievement.Counter >= 50)
            _achievementManager.ObtainAchievement("Resourceful Ninja");
    }
    
    #endregion
}
