using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Achievement Classes

public class Achievement
{
    //Used to restart the Achievement when going to the main menu or restarting the game.
    public Achievement (string topDesc, string botDesc)
    {
        TopDescription = topDesc;
        BottomDescription = botDesc;
        Obtained = false;
        Elegable = true;
    }

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public Achievement(Achievement achi)
    {
        TopDescription = achi.TopDescription;
        BottomDescription = achi.BottomDescription;
        Obtained = false;
        Elegable = true;
    }    

    /// <summary>
    /// The top line of the achievement, aka the title.
    /// </summary>
    public string TopDescription { get; set; }

    /// <summary>
    /// The fulfillment criteria for the achievement. What the player needs to do.
    /// </summary>
    public string BottomDescription { get; set; }

    /// <summary>
    /// Determines if the player has achieved the achievement or not.
    /// </summary>
    public bool Obtained { get; set; }

    /// <summary>
    /// Determines if the player is allowed to get the achievement during the active run.
    /// </summary>
    public bool Elegable{ get; set; }
}

public class SpeedBasedAchievement : Achievement
{
    public SpeedBasedAchievement(string topDesc, string botDesc, float timer) : base(topDesc, botDesc)
    {
        TimeToBeat = timer;
        TimeElapsed = 0.0f;
    }

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public SpeedBasedAchievement(SpeedBasedAchievement sAchi) : base(sAchi.TopDescription, sAchi.BottomDescription)
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

public class CollectablesAchievement 
    : Achievement
{
    public CollectablesAchievement(string topDesc, string botDesc, int NumberOfCollectablesToObtains) : base(topDesc, botDesc)
    {
        ObtainableCollectables = NumberOfCollectablesToObtains;
        CollectablesObtained = 0;
    }

    //Used to restart the Achievement when going to the main menu or restarting the game.
    public CollectablesAchievement(CollectablesAchievement cAchi) : base(cAchi.TopDescription, cAchi.BottomDescription)
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
    #region Achievement Declaration

    //1. Entering the Jungle - Clear level 1
    public Achievement clearLevel1 = new Achievement("Enter the Jungle", "Clear level 1");

    //2. Jungle with a View - Clear level 2
    public Achievement clearLevel2 = new Achievement("Jungle with a View", "Clear level 2");

    //3. Source of the Coruption - Clear Level 3
    public Achievement clearLevel3 = new Achievement("Source of the Coruption", "Clear Level 3");

    //4. Quick Ninja - Clear level 1 in under X:XX minutes
    public SpeedBasedAchievement speedClear1 = new SpeedBasedAchievement("Quick Ninja", "Clear level 1 in under 1:30 minutes", 90.0f);

    //5. Hasty Ninja - Clear level 2 in under X:XX minutes
    public SpeedBasedAchievement speedClear2 = new SpeedBasedAchievement("Hasty Ninja", "Clear level 2 in under 1:30 minutes", 90.0f);

    //6. Untrackable Ninja - Clear level 3 in under X:XX minutes
    public SpeedBasedAchievement speedClear3 = new SpeedBasedAchievement("Untrackable Ninja", "Clear level 3 in under 1:30 minutes", 90.0f);

    //7. Coup de Grace - Defeat White Face in under XX Seconds
    public SpeedBasedAchievement speedClearBoss = new SpeedBasedAchievement("Coup de Grace", "Defeat White Face in under 45 Seconds", 45.0f);

    //8. The Coruption Lingers - Obtain the Bad Ending
    public Achievement badEnding = new Achievement("The Coruption Lingers", "Obtain the Bad Ending");

    //9. The Coruption is Cleansed - Obtain the Good Ending
    public CollectablesAchievement goodEnding = new CollectablesAchievement("The Coruption is Cleansed", "Obtain the Good Ending", 3);

    //10. Level 1 Genocide - Kill all enemies in Level 1
    public CollectablesAchievement genecide1 = new CollectablesAchievement("Level 1 Genocide", "Kill all enemies in Level 1", 8);

    //11. Level 2 Genocide - Kill all enemies in Level 2
    public CollectablesAchievement genecide2 = new CollectablesAchievement("Level 2 Genocide", "Kill all enemies in Level 2", 7);

    //12. Level 3 Genocide - Kill all enemies in Level 3
    public CollectablesAchievement genecide3 = new CollectablesAchievement("Level 3 Genocide", "Kill all enemies in Level 3", 11);

    //13. Level 1 Mostly Pasifist - Only Kill Daichi in Level 1
    public Achievement pacifist1 = new Achievement("Level 1 Mostly Pasifist", "Only Kill Daichi in Level 1");

    //14. Level 2 Mostly Pasifist - Only Kill Renu in Level 1
    public Achievement pacifist2 = new Achievement("Level 2 Mostly Pasifist", "Only Kill Renu in Level 1");

    //15. Level 3 Mostly Pasifist - Only Kill Bill in Level 1
    public Achievement pacifist3 = new Achievement("Level 3 Mostly Pasifist", "Only Kill Bill in Level 1");

    //16. Not an Italian Plumber - Jump on an enemies head
    public Achievement mario = new Achievement("Not an Italian Plumber", "Jump on an enemies head");

    //17. Being a Bully - Push an enemy into a pit
    public Achievement bully = new Achievement("Being a Bully", "Push an enemy into a pit");

    //18. No Traps Activated - Clear the game without activating any swinging traps
    public Achievement trapless = new Achievement("No Traps Activated", "Clear the game without activating any swinging traps");

    //19. Expert Ninja - Clear the game without losing any lives
    public Achievement noLifeLoss = new Achievement("Expert Ninja", "Clear the game without losing any lives");

    //20. Proud Ninja - clear game without grabing any pick-ups
    public Achievement noPickups = new Achievement("Proud Ninja", "clear game without grabing any pick-ups");

    //21. Resourceful Ninja - Grab a total of 50 pick-ups throughout your journey
    public CollectablesAchievement yesPickups = new CollectablesAchievement("Resourceful Ninja", "Grab a total of 50 pick-ups throughout your journey", 50);

    //22. Martial Ninja - Beat the game without throwing any knives
    public Achievement martialNinja = new Achievement("Martial Ninja", "Beat the game without throwing any knives");

    //23. Distance Ninja - Beat the game without using any melee attacks
    public Achievement distanceNinja = new Achievement("Distance Ninja", "Beat the game without using any melee attacks");

    //24. Master Ninja - Obtain all other Achievements
    public Achievement allClear = new Achievement("Master Ninja", "Obtain allother Achievements");

    #endregion

}
