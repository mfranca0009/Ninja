using UnityEngine;

public class MenuSounds : MonoBehaviour
{
    #region Public Fields
    
    [Header("Menu Sounds Settings")]
    
    [Tooltip("The background music sound clip to be played on loop")]
    public AudioClip bgMusic;
    
    #endregion

    #region Private Fields

    // Sound / Music
    private SoundManager _soundManager;
    
    #endregion
    
    #region Unity Events
    
    private void Start()
    {
        _soundManager = FindObjectOfType<SoundManager>();
        _soundManager.PlayMusic(bgMusic);
    }

    private void OnEnable()
    {
        if (!_soundManager || _soundManager.musicSource.isPlaying)
            return;
        
        _soundManager.PlayMusic(bgMusic);
    }

    private void OnDisable()
    {
        if (!_soundManager || !_soundManager.musicSource.isPlaying)
            return;
        
        _soundManager.musicSource.Stop();
    }

    #endregion
}
