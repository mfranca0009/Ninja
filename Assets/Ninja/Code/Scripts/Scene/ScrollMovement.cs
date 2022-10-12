using UnityEngine;

public class ScrollMovement : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scroll Motion Settings")]
    
    [Tooltip("The amplitude of the sin wave, how fast it will travel up/down.")]
    [SerializeField] private float amplifier = 0.3f;
    
    [Tooltip("The frequency of the sin wave, how far it will travel up/down.")]
    [SerializeField] private float frequency = 3f;
    
    #endregion
    
    #region Private Fields
    
    // Scroll
    private Vector2 _homePos;
    
    #endregion
    
    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _homePos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        LoopScrollMotion();
    }
    
    #endregion
    
    #region Update Methods
    
    private void LoopScrollMotion()
    {
        transform.position = new Vector3(_homePos.x, Mathf.Sin(Time.time * frequency) * amplifier + _homePos.y, 0);
    }
    
    #endregion
}
