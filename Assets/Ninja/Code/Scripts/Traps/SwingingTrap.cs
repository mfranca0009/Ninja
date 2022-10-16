using UnityEngine;

public class SwingingTrap : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// Whether the trap should rotate or not.
    /// </summary>
    public bool ShouldRotate { get; private set; }
    
    #endregion
    
    #region Public Fields
    
    [Header("Swinging Trap Settings")]
    
    [Tooltip("The trap that will be triggered from pressure plate")]
    public GameObject trap;
    
    [Tooltip("The trap's blade")]
    public GameObject blade;
    
    [Tooltip("The rotation speed in which the blade will rotate")]
    public float rotationSpeed = 360.0f;
    
    #endregion

    #region Private Fields
    
    private bool _hasActivated;
    private float _step = -4.0f;
    private GameObject _trapParent;
    private AchievementManager _achievementManager;
    
    #endregion

    #region Unity Events
    
    private void Start()
    {
        _trapParent = trap.transform.parent.gameObject;
        _achievementManager = FindObjectOfType<AchievementManager>();
    }
    // Update is called once per frame
    private void Update()
    {
        if (!ShouldRotate)
            return;
        
        trap.transform.Rotate(0.0f, 0.0f, _step * Time.deltaTime);
        _step -= 1.0f;
        
        if (_trapParent.transform.localScale.x >= 0.00f)
        {
            blade.transform.Rotate(0.0f, 0.0f, rotationSpeed);
            if (trap.transform.rotation.z <= -.92f) // Based on the z value of the rotation Quaternion
            {
                ShouldRotate = false;
            }
        }
        else
        {
            blade.transform.Rotate(0.0f, 0.0f, -rotationSpeed);

            if (trap.transform.rotation.z >= .92f) //Based on the z value of the rotation Quaternion
            {
                ShouldRotate = false;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _hasActivated)
            return;

        ShouldRotate = true;
        _hasActivated = true;

        Achievement noTrapAchievement =
            _achievementManager.Achievements.Find(achievement => achievement.Title == "No Traps Activated");

        if (noTrapAchievement == null)
            return;
        
        noTrapAchievement.Eligible = false;
    }
    
    #endregion
}
