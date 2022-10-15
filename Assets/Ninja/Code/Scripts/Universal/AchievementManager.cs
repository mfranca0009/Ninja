using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public bool Eligible{ get; set; }
}

public class SpeedBasedAchievement : Achievement
{
    public SpeedBasedAchievement(AchievementType type, string title, string description, float timer) : base(type,
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

    #endregion
    
    #region Private Fields
    
    private TMP_Text[,] _cachedAchievementTexts;
    private TMP_Text _cachedCompletionPctText;
    private int _obtainedCount;
    
    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        Achievements = new List<Achievement>();
        InitAchievements();
        
        _cachedAchievementTexts = new TMP_Text[Achievements.Count, 2];
        FillAchievementUIList();
    }

    private void Update()
    {
        // DEBUG
        
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;
        
        ObtainAchievement(Achievements[_obtainedCount].Title);
        
        // DEBUG END
    }
    
    #endregion

    #region Private Helper Methods
    
    private void InitAchievements()
    {
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Enter the Jungle", "Clear level 1"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Jungle with a View", "Clear level 2"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Source of the corruption", "Clear level 3"));

        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Quick Ninja",
            "Clear level 1 in 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Hasty Ninja",
            "Clear level 2 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Untraceable Ninja",
            "Clear level 3 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Coup de Grâce",
            "Defeat White Face in under 45 Seconds", 45f));

        Achievements.Add(
            new Achievement(AchievementType.TriggerType, "The Corruption Lingers", "Obtain the bad ending"));
        Achievements.Add(new CounterAchievement(AchievementType.CollectibleType, "The corruption is cleansed",
            "Obtain the good ending", 3));

        Achievements.Add(new CounterAchievement(AchievementType.CollectibleType, "Level 1 Genocide",
            "Kill all enemies in level 1", 8));

        Achievements.Add(new CounterAchievement(AchievementType.CollectibleType, "Level 2 Genocide",
            "Kill all enemies in Level 2", 7));

        Achievements.Add(new CounterAchievement(AchievementType.CollectibleType, "Level 3 Genocide",
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
            "Clear the game without activating any swinging traps"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Expert Ninja",
            "Clear the game without losing any lives"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Proud Ninja",
            "Clear game without grabbing any pick-ups"));

        Achievements.Add(new CounterAchievement(AchievementType.CollectibleType, "Resourceful Ninja",
            "Grab a total of 50 pick-ups throughout your journey", 50));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Martial Ninja",
            "Beat the game without throwing any knives"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Distance Ninja",
            "Beat the game without using any melee attacks"));

        Achievements.Add(new Achievement(AchievementType.TriggerType, "Master Ninja", "Obtain all other Achievements"));

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

        /*SpeedBasedAchievement achi =
            Achievements.Find(possibleAchievement =>
                possibleAchievement.Title == "Enter the Jungle") as SpeedBasedAchievement;*/
    }
    private void FillAchievementUIList()
    {
        for(int i = 0; i < Achievements.Count; i++)
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
    
    #endregion

    #region Public Helper Methods
    
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

    public void ObtainAchievement(string achievementName)
    {
        Achievements.Find(achi => achi.Title == achievementName).Obtained = true;
        _obtainedCount++;

        //24. Master Ninja Check
        if (_obtainedCount == Achievements.Count - 1)
        {
            Achievements.Find(achi => achi.Title == "Master Ninja").Obtained = true;
            _obtainedCount++;
        }
    }
    
    public void ResetTimers()
    {
        (Achievements.Find(achi => achi.Title == "Quick Ninja") as SpeedBasedAchievement).TimeElapsed = 0.0f;
        (Achievements.Find(achi => achi.Title == "Hasty Ninja") as SpeedBasedAchievement).TimeElapsed = 0.0f;
        (Achievements.Find(achi => achi.Title == "Untraceable Ninja") as SpeedBasedAchievement).TimeElapsed = 0.0f;
        (Achievements.Find(achi => achi.Title == "Coup de Grâce") as SpeedBasedAchievement).TimeElapsed = 0.0f;
    }
    #endregion
}
