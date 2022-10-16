using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#region Achievement Classes

public class Achievement
{
    public Achievement (AchievementType type, string title, string description)
    {
        Type = type;
        Title = title;
        Description = description;
        Obtained = false;
        Eligible = true;
    }
    
    /// <summary>
    /// The type of the achievement.
    /// </summary>
    public AchievementType Type { get; set; }
    
    /// <summary>
    /// The achievement's title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The achievement's description, fulfillment criteria for the achievement; what the player needs to do.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Determines if the player has obtained the achievement or not.
    /// </summary>
    public bool Obtained { get; set; }

    /// <summary>
    /// Determines if the player is eligible to receive the achievement during the active run.
    /// </summary>
    public bool Eligible { get; set; }
    
    /// <summary>
    /// Determines if the achievement has shown its pop notification or not.
    /// </summary>
    public bool HasPopped { get; set; }
}

public class SpeedAchievement : Achievement
{
    public SpeedAchievement(AchievementType type, string title, string description, float timer) : base(type,
        title, description)
    {
        TimeToBeat = timer;
        TimeElapsed = 0.0f;
    }

    /// <summary>
    /// The time that is required to be beat or met to obtain the achievement.
    /// </summary>
    public float TimeToBeat { get; set; }

    /// <summary>
    /// The time that has elapsed towards the achievement.
    /// </summary>
    public float TimeElapsed { get; set; }
}

public class CounterAchievement : Achievement
{
    public CounterAchievement(AchievementType type, string title, string description,
        int requiredAmount) : base(type, title, description)
    {
        MaxCounter = requiredAmount;
        Counter = 0;
    }
    
    /// <summary>
    /// The required amount that needs to be met to fulfill the achievement.
    /// </summary>
    public int MaxCounter { get; set; }

    /// <summary>
    /// The counter which tracks progress to the completion of the achievement.
    /// </summary>
    public int Counter { get; set; }
}

#endregion

public class AchievementManager : MonoBehaviour
{
    #region Public Properties

    public List<Achievement> Achievements { get; private set; }

    #endregion

    #region Public Fields

    public GameObject achievementList;
    public Scrollbar scrollbar;

    // public Canvas popAchievementCanvas;
    public GameObject popAchievementBg;
    public TMP_Text popAchievementName;
    public Animator popAchievementAnimator;
    public float hidePopAchievementDelay = 5f;

    public AudioClip achievementObtainedSound;

    #endregion

    #region Private Fields

    private TMP_Text[,] _cachedAchievementTexts;
    private TMP_Text _cachedCompletionPctText;
    private int _obtainedCount;

    private float _hidePopAchievementTimer;

    // Speed achievement currently tracked for active level
    private SpeedAchievement _trackedSpeedAchievement;

    private SoundManager _soundManager;

    private Queue<Achievement> _obtainedAchievementQueue;
    private bool _achievementPopInProgress;
    
    private int _currentBuildIndex;
    #endregion

    #region Unity Events

    private void Awake()
    {
        _soundManager = FindObjectOfType<SoundManager>();

        _obtainedAchievementQueue = new Queue<Achievement>();
        Achievements = new List<Achievement>();
        InitAchievements();

        _cachedAchievementTexts = new TMP_Text[Achievements.Count, 2];
        FillAchievementUIList();

        _hidePopAchievementTimer = hidePopAchievementDelay;
    }

    private void Update()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;

        AchievementCleanUp(buildIndex);
        
        ShowPopAchievementUI();
        HidePopAchievementUI();
        popAchievementAnimator.SetBool("PopInProgress", _achievementPopInProgress);
        
        TrackAchievementTimer(buildIndex);
        UpdateTrackedSpeedAchievementTimer();

        // Update the build index of the active level last.
        _currentBuildIndex = buildIndex;
        
        // DEBUG
        
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        ObtainAchievement(Achievements[_obtainedCount].Title);

        // DEBUG END
    }
    
    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Initialize and store all achievements for the game.
    /// </summary>
    private void InitAchievements()
    {
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Enter the Jungle", "Clear level 1"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Jungle with a View", "Clear level 2"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Source of the corruption", "Clear level 3"));

        Achievements.Add(new SpeedAchievement(AchievementType.SpeedType, "Quick Ninja",
            "Clear level 1 in 1:30 minutes", 90f));
        Achievements.Add(new SpeedAchievement(AchievementType.SpeedType, "Hasty Ninja",
            "Clear level 2 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedAchievement(AchievementType.SpeedType, "Untraceable Ninja",
            "Clear level 3 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedAchievement(AchievementType.SpeedType, "Coup de Grâce",
            "Defeat White Face in under 45 Seconds", 45f));

        Achievements.Add(
            new Achievement(AchievementType.TriggerType, "The Corruption Lingers", "Obtain the bad ending"));
        Achievements.Add(new CounterAchievement(AchievementType.CounterType, "The corruption is cleansed",
            "Obtain the good ending", 3));

        Achievements.Add(new CounterAchievement(AchievementType.CounterType, "Level 1 Genocide",
            "Kill all enemies in level 1", 8));

        Achievements.Add(new CounterAchievement(AchievementType.CounterType, "Level 2 Genocide",
            "Kill all enemies in Level 2", 7));

        Achievements.Add(new CounterAchievement(AchievementType.CounterType, "Level 3 Genocide",
            "Kill all enemies in Level 3", 11));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Level 1 Mostly Pacifist",
            "Only kill Daichi in level 1"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Level 2 Mostly Pacifist",
            "Only kill Renu in level 2"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Level 3 Mostly Pacifist",
            "Only kill Bill in level 3"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Not an Italian Plumber",
            "Jump on an enemy's head"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Being a Bully", "Push an enemy into a pit"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "No Traps Activated",
            "Reach the boss without activating any swinging traps"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Proud Ninja",
            "Reach the boss without grabbing any pick-ups"));

        Achievements.Add(new CounterAchievement(AchievementType.CounterType, "Resourceful Ninja",
            "Grab a total of 50 pick-ups throughout your journey", 50));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Martial Ninja",
            "Beat the game without throwing any knives"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Distance Ninja",
            "Beat the game without using any melee attacks"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Expert Ninja",
            "Clear the game without dying"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Master Ninja", "Obtain all other Achievements"));
    }

    /// <summary>
    /// Initial setup of the achievement UI building all achievement objects.
    /// </summary>
    private void FillAchievementUIList()
    {
        for (int i = 0; i < Achievements.Count; i++)
        {
            // Retrieve the achievement.
            Achievement achievement = Achievements[i];

            // Create the achievement's parent and children objects.
            GameObject parentObject =
                new GameObject($"Achievement {i + 1}", typeof(RectTransform), typeof(VerticalLayoutGroup));
            parentObject.layer = LayerMask.NameToLayer("UI");
            GameObject title = new GameObject("Name", typeof(TextMeshProUGUI));
            GameObject description = new GameObject("Description", typeof(TextMeshProUGUI));

            // Set the parent for the parent and children objects of the achievement object.
            title.transform.SetParent(parentObject.transform, false);
            description.transform.SetParent(parentObject.transform, false);
            parentObject.transform.SetParent(achievementList.transform, false);

            // Retrieve necessary components for parent and child objects of the newly created achievement object.
            RectTransform parentRectTransform = parentObject.GetComponent<RectTransform>();
            VerticalLayoutGroup parentVerticalLayoutGroup = parentObject.GetComponent<VerticalLayoutGroup>();
            RectTransform titleRectTransform = title.GetComponent<RectTransform>();
            TMP_Text titleText = title.GetComponent<TMP_Text>();
            RectTransform descriptionRectTransform = description.GetComponent<RectTransform>();
            TMP_Text descriptionText = description.GetComponent<TMP_Text>();


            // Setup necessary components for parent and child objects of the newly created achievement object.
            parentRectTransform.anchoredPosition = titleRectTransform.anchoredPosition =
                descriptionRectTransform.anchoredPosition = Vector2.zero;

            parentRectTransform.sizeDelta = titleRectTransform.sizeDelta =
                descriptionRectTransform.sizeDelta = new Vector2(990, 40);

            parentVerticalLayoutGroup.childControlHeight = false;
            parentVerticalLayoutGroup.childControlWidth = false;
            descriptionText.text = achievement.Description;
            titleText.text = achievement.Obtained ? achievement.Title : "???";
            titleText.color = descriptionText.color = achievement.Obtained ? Color.black : Color.grey;

            // Cache the title and description text for that achievement [Refreshing purposes].
            _cachedAchievementTexts[i, 0] = titleText;
            _cachedAchievementTexts[i, 1] = descriptionText;
        }

        // Create the completion percent gameobject and set its parent object.
        GameObject completionPct =
            new GameObject("Completion Percent", typeof(RectTransform), typeof(TextMeshProUGUI));
        completionPct.transform.SetParent(achievementList.transform, false);

        // Retrieve necessary components from the completion percent and achievement list gameobjects.
        RectTransform achieveListRectTransform = achievementList.GetComponent<RectTransform>();
        RectTransform completionPctRectTransform = completionPct.GetComponent<RectTransform>();
        TMP_Text completionPctText = completionPct.GetComponent<TMP_Text>();

        // Setup the completion percent and achievement list gameobjects.
        completionPctRectTransform.anchoredPosition = Vector2.zero;
        completionPctRectTransform.sizeDelta = new Vector2(990, 40);
        completionPctText.text = $"Achievements Obtained: {_obtainedCount}/{Achievements.Count}";
        completionPctText.color = Color.grey;
        achieveListRectTransform.anchoredPosition = new Vector2(0, -1236);

        // Cache the completion percent text [Refreshing purposes]
        _cachedCompletionPctText = completionPctText;
    }

    /// <summary>
    /// Play the obtain pop achievement notification show animation.
    /// </summary>
    private void ShowPopAchievementUI()
    {
        if (!popAchievementAnimator.IsPlayingAnimation("Empty", (int)AnimationLayers.BaseAnimLayer) ||
            _achievementPopInProgress || _obtainedAchievementQueue.Count == 0)
            return;

        _achievementPopInProgress = true;
        
        Achievement achievement = _obtainedAchievementQueue.Dequeue();

        achievement.HasPopped = true;
        popAchievementName.text = achievement.Title;
        popAchievementAnimator.SetTrigger("Show");
        
        if (!_soundManager)
            return;

        _soundManager.PlaySoundEffect(AudioSourceType.UiEffects, achievementObtainedSound);
    }

    /// <summary>
    /// Play the obtain pop achievement notification hide animation. Also updates pop-notification progress to false.
    /// </summary>
    private void HidePopAchievementUI()
    {
        if (popAchievementAnimator.IsPlayingAnimation("Show", (int)AnimationLayers.BaseAnimLayer) &&
            popAchievementAnimator.GetCurrentAnimatorStateInfo((int)AnimationLayers.BaseAnimLayer).normalizedTime >= 1f)
        {
            if (_hidePopAchievementTimer <= 0)
            {
                popAchievementAnimator.SetTrigger("Hide");
                _hidePopAchievementTimer = hidePopAchievementDelay;
            }
            else
                _hidePopAchievementTimer -= Time.unscaledDeltaTime;
        }

        if (popAchievementAnimator.IsPlayingAnimation("Hide", (int)AnimationLayers.BaseAnimLayer) &&
            popAchievementAnimator.GetCurrentAnimatorStateInfo((int)AnimationLayers.BaseAnimLayer).normalizedTime >= 1f)
            _achievementPopInProgress = false;
    }

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Refresh the achievement UI to reflect any changes with obtained achievements.
    /// </summary>
    public void RefreshAchievementUIList()
    {
        //Gameobject.transform.childCount starts counting at 1 not 0.
        //The last object in achievementList is a completion total and is handled after the loop.
        for (int i = 0; i < achievementList.transform.childCount - 1; i++)
        {
            // Move to next achievement if current has not obtained.
            if (!Achievements[i].Obtained)
                continue;

            // Update the cached title and description texts as needed.
            _cachedAchievementTexts[i, 0].text = Achievements[i].Title;
            _cachedAchievementTexts[i, 0].color = _cachedAchievementTexts[i, 1].color = Color.black;
        }

        // Update the cached completion percent text and color appropriately
        _cachedCompletionPctText.text = $"Achievements Obtained: {_obtainedCount}/{Achievements.Count}";
        _cachedCompletionPctText.color = _obtainedCount == Achievements.Count ? Color.black : Color.grey;
    }


    /// <summary>
    /// This Function takes the title of an achievement and checks if that achievement
    /// is eligible and hasn't been obtained. If both are true, it marks that named 
    /// achievement as obtained. Then if all other achievements have been obtained, 
    /// it awards the Master Ninja Achievement.
    /// </summary>
    /// <param name="achievementName">The achievement's title to obtain.</param>
    public void ObtainAchievement(string achievementName)
    {
        Achievement achievement = Achievements.Find(achievement => achievement.Title == achievementName);
        
        if (achievement is not { Eligible: true } || achievement.Obtained)
            return;
        
        achievement.Obtained = true;
        _obtainedCount++;
        _obtainedAchievementQueue.Enqueue(achievement);
        
        //24. Master Ninja Check - return if achievement count is not max count.
        if (_obtainedCount != Achievements.Count - 1)
            return;

        achievement = Achievements.Find(possibleAchievement => possibleAchievement.Title == "Master Ninja");
        
        if (achievement is not { Eligible: true } || achievement.Obtained)
            return;

        achievement.Obtained = true;
        _obtainedCount++;
        _obtainedAchievementQueue.Enqueue(achievement);

    }
    
    /// <summary>
    /// Assign the appropriate speed achievement that should be tracked if the active level has changed.
    /// </summary>
    /// <param name="newBuildIndex">The new scene build index.</param>
    private void TrackAchievementTimer(int newBuildIndex)
    {
        if (_currentBuildIndex == newBuildIndex)
            return;
        
        SpeedAchievement speedAchievement = newBuildIndex switch
        {
            1 => Achievements.Find(achievement => achievement.Title == "Quick Ninja") as SpeedAchievement,
            2 => Achievements.Find(achievement => achievement.Title == "Hasty Ninja") as SpeedAchievement,
            3 => Achievements.Find(achievement => achievement.Title == "Untrackable Ninja") as SpeedAchievement,
            4 => Achievements.Find(achievement => achievement.Title == "Coup de Grâce") as SpeedAchievement,
            _ => null
        };

        _trackedSpeedAchievement = speedAchievement;
    }
    
    /// <summary>
    /// Update the achievement timer for the tracked speed achievement for the active level.
    /// </summary>
    private void UpdateTrackedSpeedAchievementTimer()
    {
        if (_trackedSpeedAchievement == null)
            return;
        
        _trackedSpeedAchievement.TimeElapsed += Time.deltaTime;
        
        if (_trackedSpeedAchievement.TimeElapsed > _trackedSpeedAchievement.TimeToBeat)
            _trackedSpeedAchievement.Eligible = false;
    }
    
    /// <summary>
    /// Clean-up timers and counters when the active level has changed depending on circumstances.
    /// </summary>
    /// <param name="newBuildIndex">The new scene build index.</param>
    private void AchievementCleanUp(int newBuildIndex)
    {
        if (_currentBuildIndex == newBuildIndex)
            return;
        
        ResetTimers();

        if (newBuildIndex != 1) 
            return;
        
        ResetCounters();
        Achievements.Find(achievement => achievement.Title == "Martial Ninja").Eligible = true;
        Achievements.Find(achievement => achievement.Title == "Distance Ninja").Eligible = true;
        Achievements.Find(achievement => achievement.Title == "Expert Ninja").Eligible = true;
    }
    
    /// <summary>
    /// Reset timers for all speed-based achievements
    /// </summary>
    public void ResetTimers()
    {
        if (Achievements.FindAll(possibleAchievements => possibleAchievements.Type == AchievementType.SpeedType)
                .ToArray() is not SpeedAchievement[] speedAchievements)
            return;

        foreach (SpeedAchievement achievement in speedAchievements)
        {
            achievement.TimeElapsed = 0.0f;
            achievement.Eligible = true;
        }
    }

    /// <summary>
    /// Reset counters for all counter-type achievements.
    /// </summary>
    public void ResetCounters()
    {
        if (Achievements.FindAll(possibleAchievements => possibleAchievements.Type == AchievementType.CounterType)
            .ToArray() is not CounterAchievement[] counterAchievements)
            return;
        
        foreach (CounterAchievement achievement in counterAchievements)
        {
            achievement.Counter = 0;
            achievement.Eligible = true;
        }
    }
}

#endregion

/*-----------------------------------------------------------------------------------/
|                                                                                    |
|   Filter out non-trigger type achievements, only select trigger type achievements  |
|                                                                                    |
|-----------------------------------------------------------------------------------*/

/*Achievement[] triggerAchievementArray = Achievements.FindAll(possibleAchievements =>
    possibleAchievements.Type == AchievementType.TriggerType).ToArray();*/


/*-------------------------------------------------------/
|                                                        |
|  Search for a specific achievement by title and type   |
|                                                        |
|-------------------------------------------------------*/

/*Achievement achievementFound = Achievements.Find(possibleAchievement =>
    possibleAchievement.Type == AchievementType.TriggerType &&
    possibleAchievement.Title == "Expert Ninja");*/

/*-------------------------------------------------------/
|                                                        |
|        Search for an achievement by title              |
|                                                        |
|-------------------------------------------------------*/

/*SpeedAchievement achi =
    Achievements.Find(possibleAchievement =>
        possibleAchievement.Title == "Enter the Jungle") as SpeedAchievement;*/