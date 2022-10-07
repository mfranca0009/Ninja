using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
	// Audio players components.
	public AudioMixer MainAudioMixer;
	public AudioSource MusicSource;
	public AudioSource MovementEffectsSource;
	public AudioSource AttackEffectsSource;
	public AudioSource DamageEffectsSource;
	public AudioSource ItemEffectsSource;

	// Random pitch adjustment range.
	public float LowPitchRange = .95f;
	public float HighPitchRange = 1.05f;

	
	/// <summary>
	/// Play a sound effect on the effects audio source.<br></br><br></br>
	/// Note: If one shot is true [default], it will allow for simultaneous sound effects playing at one time.
	/// If false, it will override the current playing sound effect(s) with a single sound effect.
	/// </summary>
	/// <param name="source">The audio source type to play the sound effect</param>
	/// <param name="clip">The sound effect to play.</param>
	/// <param name="oneShot">
	/// Whether the sound effect should play simultaneously with other sound effects or not; defaults to true.
	/// </param>
	public void PlaySoundEffect(AudioSourceType source, AudioClip clip, bool oneShot = true)
	{
		AudioSource sourceSfx = source switch
		{
			AudioSourceType.MovementEffects => MovementEffectsSource,
			AudioSourceType.AttackEffects => AttackEffectsSource,
			AudioSourceType.DamageEffects => DamageEffectsSource,
			AudioSourceType.ItemEffects => ItemEffectsSource,
			AudioSourceType.None or _ => null
		};

		if (!sourceSfx)
			return;

		if (!oneShot)
		{
			sourceSfx.clip = clip;
			sourceSfx.Play();
		}
		else
			sourceSfx.PlayOneShot(clip, sourceSfx.volume);
	}
	
	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		MusicSource.clip = clip;
		MusicSource.Play();
	}
	
	// Play a random clip from an array, and optionally randomize the pitch slightly.
	public void RandomSoundEffect(AudioSourceType source, bool oneShot, bool randomizePitch, params AudioClip[] clips)
	{
		int randomIndex = Random.Range(0, clips.Length);
		float randomPitch = Random.Range(LowPitchRange, HighPitchRange);

		AudioSource sourceSfx = source switch
		{
			AudioSourceType.MovementEffects => MovementEffectsSource,
			AudioSourceType.AttackEffects => AttackEffectsSource,
			AudioSourceType.DamageEffects => DamageEffectsSource,
			AudioSourceType.ItemEffects => ItemEffectsSource,
			AudioSourceType.None or _ => null
		};

		if (!sourceSfx)
			return;

		if (randomizePitch)
			sourceSfx.pitch = randomPitch;

		if (!oneShot)
		{
			sourceSfx.clip = clips[randomIndex];
			sourceSfx.Play();
		}
		else
			sourceSfx.PlayOneShot(clips[randomIndex], sourceSfx.volume);
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
	private void OnEnable()
	{
		//Debug.Log("OnEnable called");
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// called second
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log($"OnSceneLoaded: [Name: {scene.name}, Build Index: {scene.buildIndex}]");
		Debug.Log(mode);
		StartCoroutine(StartAudioFade(MusicSource, 5f, 0.3f));
	}

	// called third
	private void Start()
	{
		Debug.Log("Start");
        SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private void OnSceneUnloaded(Scene current)
	{
		Debug.Log($"OnSceneUnloaded: [Name: {current.name}, Build Index: {current.buildIndex}]");
		SceneManager.sceneLoaded -= OnSceneLoaded;
		//StartCoroutine(StartAudioFade(MusicSource, 0.1f, 0f));
		//StartCoroutine(StartAudioFade(MusicSource, 10f, 0.3f));
	}

	// called when the game is terminated
	private void OnDisable()
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
