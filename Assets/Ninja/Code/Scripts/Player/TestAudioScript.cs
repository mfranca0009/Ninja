using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudioScript : MonoBehaviour
{
    [SerializeField]
    private SoundManager _soundManager;

    public AudioClip soundEffect;

    private void Start()
    {
        _soundManager = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Matt: randomly place attack effects source, this is only a test anyways.
            _soundManager.PlaySoundEffect(AudioSourceType.AttackEffects, soundEffect);

        }
    }
}
