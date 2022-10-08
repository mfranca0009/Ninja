using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{
    [SerializeField]
    private SoundManager _soundManager;

    public AudioClip bg_music;

    void Start()
    {
        _soundManager = FindObjectOfType<SoundManager>();

        if (!_soundManager)
            return;
        
        _soundManager.PlayMusic(bg_music);
    }

    private void Update()
    {

    }
}
