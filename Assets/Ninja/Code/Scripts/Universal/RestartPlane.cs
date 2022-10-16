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
        bool isPlayer = collision.gameObject.CompareTag("Player");

        if (!isPlayer && !collision.gameObject.CompareTag("Enemy"))
            return;

        collision.gameObject.GetComponent<Health>().InstaKill(true);

        if (isPlayer)
            return;

        //Implementation for Achievement 17. Being a Bully
        Achievement achievement = _achievementManager.Achievements.Find(achi => achi.Title == "Being a Bully");
        if ( achievement.Obtained == false)
            achievement.Obtained = true;

        if (shouldDestroyEnemy)
            Destroy(collision.gameObject, enemyDestroyDelay);
        else
            collision.gameObject.SetActive(false);
    }
    
    #endregion
}
