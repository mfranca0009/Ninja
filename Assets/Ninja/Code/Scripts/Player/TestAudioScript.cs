using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudioScript : MonoBehaviour
{
    [SerializeField]
    private SoundManager _soundManager;

    public AudioClip soundEffect;

    void Start()
    {
        _soundManager = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _soundManager.PlaySoundEffect(soundEffect);

        }
    }
}
