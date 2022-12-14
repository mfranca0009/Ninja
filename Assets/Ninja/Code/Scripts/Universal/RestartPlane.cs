using UnityEngine;

public class RestartPlane : MonoBehaviour
{
    #region Public Fields
    
    [Tooltip("Where the gameobject should respawn")]
    public Vector3 respawnLocation = new Vector3(0, 0, 0);
    
    [Tooltip("Should destroy enemy gameobject after death?")]
    public bool shouldDestroyEnemy = true;
    
    [Tooltip("The delay time before destroying the enemy gameobject")]
    public float enemyDestroyDelay = 3f;

    #endregion

    #region Private Fields
    
    private AchievementManager _achievementManager;

    #endregion

    #region Unity Events

    private void Awake()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.gameObject.layer == LayerMask.NameToLayer("Player");

        if (!isPlayer && collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            return;

        collision.gameObject.GetComponent<Health>().InstaKill(true);

        if (isPlayer)
            return;

        if (shouldDestroyEnemy)
            Destroy(collision.gameObject, enemyDestroyDelay);
        else
            collision.gameObject.SetActive(false);

        if (!_achievementManager)
            return;
        
        // Achievement 17 [Being a bully]
        Achievement achievement =
            _achievementManager.Achievements.Find(achievement => achievement.Title == "Being a Bully");

        if (achievement == null)
            return;

        _achievementManager.ObtainAchievement(achievement.Title);
    }
    
    #endregion
}
