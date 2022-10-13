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

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public Achievement(Achievement achi)
    {
        Type = achi.Type;
        Title = achi.Title;
        Description = achi.Description;
        Obtained = false;
        Eligible = true;
    }

    public AchievementType Type { get; set; }
    
    /// <summary>
    /// The top line of the achievement, aka the title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The fulfillment criteria for the achievement. What the player needs to do.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Determines if the player has achieved the achievement or not.
    /// </summary>
    public bool Obtained { get; set; }

    /// <summary>
    /// Determines if the player is allowed to get the achievement during the active run.
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

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public SpeedBasedAchievement(SpeedBasedAchievement sAchi) : base(sAchi.Type, sAchi.Title, sAchi.Description)
    {
        TimeToBeat = sAchi.TimeToBeat;
        TimeElapsed = 0.0f;
    }

    /// <summary>
    /// float is used to determine speed based achievements. Not used by others.
    /// </summary>
    public float TimeToBeat { get; set; }

    /// <summary>
    /// The time that has elapsed towards the specific achievement.
    /// </summary>
    public float TimeElapsed { get; set; }
}

public class CollectiblesAchievement : Achievement
{
    public CollectiblesAchievement(AchievementType type, string title, string description,
        int numberOfCollectablesToObtains) : base(type, title, description)
    {
        ObtainableCollectables = numberOfCollectablesToObtains;
        CollectablesObtained = 0;
    }

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public CollectiblesAchievement(CollectiblesAchievement cAchi) : base(cAchi.Type, cAchi.Title, cAchi.Description)
    {
        ObtainableCollectables = cAchi.ObtainableCollectables;
        CollectablesObtained = 0;
    }

    /// <summary>
    /// float is used to determine speed based achievements. Not used by others.
    /// </summary>
    public int ObtainableCollectables { get; set; }

    /// <summary>
    /// The time that has elapsed towards the specific achievement.
    /// </summary>
    public int CollectablesObtained { get; set; }
}

#endregion

public class AchievementManager : MonoBehaviour
{
    #region Public Properties

    public List<Achievement> Achievements { get; private set; }
    public GameObject AchiList;

    #endregion

    private void Awake()
    {
        Achievements = new List<Achievement>();
        InitAchievements();
        FillAchievementUIList();


    }


    private void InitAchievements()
    {
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Enter the Jungle", "Clear level 1"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Jungle with a View", "Clear level 2"));
        Achievements.Add(new Achievement(AchievementType.TriggerType, "Source of the corruption", "Clear level 3"));

        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Quick Ninja",
            "Clear level 1 in 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Hasty Ninja",
            "Clear level 2 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Un-trackable Ninja",
            "Clear level 3 in under 1:30 minutes", 90f));
        Achievements.Add(new SpeedBasedAchievement(AchievementType.SpeedType, "Coup de Grâce",
            "Defeat White Face in under 45 Seconds", 45f));

        Achievements.Add(
            new Achievement(AchievementType.TriggerType, "The Corruption Lingers", "Obtain the bad ending"));
        Achievements.Add(new CollectiblesAchievement(AchievementType.CollectibleType, "The corruption is cleansed",
            "Obtain the good ending", 3));

        Achievements.Add(new CollectiblesAchievement(AchievementType.CollectibleType, "Level 1 Genocide",
            "Kill all enemies in level 1", 8));

        Achievements.Add(new CollectiblesAchievement(AchievementType.CollectibleType, "Level 2 Genocide",
            "Kill all enemies in Level 2", 7));

        Achievements.Add(new CollectiblesAchievement(AchievementType.CollectibleType, "Level 3 Genocide",
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

        Achievements.Add(new CollectiblesAchievement(AchievementType.CollectibleType, "Resourceful Ninja",
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
        foreach (var achievement in Achievements)
        {
            GameObject gameObject = new GameObject();
            GameObject title = new GameObject();
            GameObject description = new GameObject();

            title.name = "Name";
            description.name = "Description";
            gameObject.name = "Achievement";

            gameObject.AddComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(990, 40);
            gameObject.AddComponent<VerticalLayoutGroup>();
            gameObject.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            gameObject.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
            gameObject.layer = LayerMask.NameToLayer("UI");

            title.AddComponent<TextMeshProUGUI>();
            title.AddComponent<RectTransform>();
            title.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(990, 40);

            description.AddComponent<TextMeshProUGUI>();
            description.GetComponent<TextMeshProUGUI>().text = achievement.Description;
            description.AddComponent<RectTransform>();
            description.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            description.GetComponent<RectTransform>().sizeDelta = new Vector2(990, 40);

            if (achievement.Obtained)
            {
                title.GetComponent<TextMeshProUGUI>().text = achievement.Title;
                title.GetComponent<TextMeshProUGUI>().color = Color.black;
                description.GetComponent<TextMeshProUGUI>().color = Color.black;
            }
            else
            {
                title.GetComponent<TextMeshProUGUI>().text = "???";
                title.GetComponent<TextMeshProUGUI>().color = Color.grey;
                description.GetComponent<TextMeshProUGUI>().color = Color.grey;
            }


            title.transform.SetParent(gameObject.transform, false);
            description.transform.SetParent(gameObject.transform, false);
            gameObject.transform.SetParent(AchiList.transform, false);
        }
    }
}
