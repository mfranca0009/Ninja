using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
	// Audio players components.
	public AudioSource EffectsSource;
	public AudioSource MusicSource;
	// Random pitch adjustment range.
	public float LowPitchRange = .95f;
	public float HighPitchRange = 1.05f;


	// Play a single clip through the sound effects source.
	public void PlaySoundEffect(AudioClip clip)
	{
		EffectsSource.clip = clip;
		EffectsSource.Play();
	}
	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		MusicSource.clip = clip;
		MusicSource.Play();
	}
	// Play a random clip from an array, and randomize the pitch slightly.
	public void RandomSoundEffect(params AudioClip[] clips)
	{
		int randomIndex = Random.Range(0, clips.Length);
		float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
		EffectsSource.pitch = randomPitch;
		EffectsSource.clip = clips[randomIndex];
		EffectsSource.Play();
	}

	public static IEnumerator StartAudioFade(AudioSource audioSource, float duration, float targetVolume)
	{
		float currentTime = 0;
		float start = audioSource.volume;
		while (currentTime < duration)
		{
			currentTime += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
			yield return null;
		}
		yield break;
	}

	/// TODO: Still playing with this. Goal: fade in the music for each scene. right
	/// now it's only fading in on the _main scene (also want to fade out as you tranisiton scenes)

	// called first
	void OnEnable()
	{
		//Debug.Log("OnEnable called");
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// called second
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("OnSceneLoaded: " + scene.name);
		Debug.Log(mode);
		StartCoroutine(StartAudioFade(MusicSource, 5f, 0.3f));

	}

	// called third
	void Start()
	{
		Debug.Log("Start");
        SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private void OnSceneUnloaded(Scene current)
	{
		Debug.Log("OnSceneUnloaded: " + current);
		SceneManager.sceneLoaded -= OnSceneLoaded;
		//StartCoroutine(StartAudioFade(MusicSource, 0.1f, 0f));
		//StartCoroutine(StartAudioFade(MusicSource, 10f, 0.3f));
	}

	// called when the game is terminated
	void OnDisable()
	{
		Debug.Log("OnDisable");
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

	///
	/// EXAMPLES ON HOW TO USE
	///
	/*public class GameScript : MonoBehaviour
{
	[SerializeField]
	private SoundManager _soundManager;

    public AudioClip BattleMusic;

    void Start() {
        _soundManager = FindObjectOfType<SoundManager>();
		_soundManager.PlayMusic(BattleMusic);
    }
}
 
	 * public class CharacterScript : MonoBehaviour
{
	[SerializeField]
	private SoundManager _soundManager;

    public AudioClip[] AttackNoises;

    void Start() {
        _soundManager = FindObjectOfType<SoundManager>();
		_soundManager.RandomSoundEffect(AttackNoises);
    }
}
	*/


}
