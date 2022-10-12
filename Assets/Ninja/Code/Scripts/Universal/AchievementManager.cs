using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement
{
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

    /// <summary>
    /// float is used to determine speed based achievements. Not used by others.
    /// </summary>
    public float TimeToBeat { get; set; }

    /// <summary>
    /// The time that has elapsed towards the specific achievement.
    /// </summary>
    public float TimeElapsed { get; set; }

    public void PrimeAchievement(string topDesc, string botDesc)
    {
        TopDescription = topDesc;
        BottomDescription = botDesc;
        Elegable = true;
    }

    public void PrimeAchievement(string topDesc, string botDesc, float timeToBeat)
    {
        TopDescription = topDesc;
        BottomDescription = botDesc;
        Elegable = true;
        TimeToBeat = timeToBeat;
    }
}

public class AchievementManager : MonoBehaviour
{
    #region Achievement Declaration
    //1. Entering the Jungle - Clear level 1
    public Achievement clearLevel1;
    
    //2. Jungle with a View - Clear level 2
    public Achievement clearLevel2;
    
    //3. Source of the Coruption - Clear Level 3
    public Achievement clearLevel3;
    
    //4. Quick Ninja - Clear level 1 in under X:XX minutes
    public Achievement speedClear1;
    
    //5. Hasty Ninja - Clear level 2 in under X:XX minutes
    public Achievement speedClear2;
    
    //6. Untrackable Ninja - Clear level 3 in under X:XX minutes
    public Achievement speedClear3;
    
    //7. Coup de Grace - Defeat White Face in under XX Seconds
    public Achievement speedClearBoss;
    
    //8. The Coruption Lingers - Obtain the Bad Ending
    public Achievement badEnding;
    
    //9. The Coruption is Cleansed - Obtain the Good Ending
    public Achievement goodEnding;
    
    //10. Level 1 Genocide - Kill all enemies in Level 1
    public Achievement genecide1;
    
    //11. Level 2 Genocide - Kill all enemies in Level 2
    public Achievement genecide2;
    
    //12. Level 3 Genocide - Kill all enemies in Level 3
    public Achievement genecide3;
    
    //13. Level 1 Mostly Pasifist - Only Kill Daichi in Level 1
    public Achievement pacifist1;
    
    //14. Level 2 Mostly Pasifist - Only Kill Renu in Level 1
    public Achievement pacifist2;
    
    //15. Level 3 Mostly Pasifist - Only Kill Bill in Level 1
    public Achievement pacifist3;
    
    //16. Not an Italian Plumber - Jump on an enemies head
    public Achievement mario;
    
    //17. Being a Bully - Push an enemy into a pit
    public Achievement bully;
    
    //18. No Traps Activated - Clear the game without activating any swinging traps
    public Achievement trapless;
    
    //19. Expert Ninja - Clear the game without losing any lives
    public Achievement noLifeLoss;
    
    //20. Proud Ninja - clear game without grabing any pick-ups
    public Achievement noPickups;
    
    //21. Resourceful Ninja - Grab a total of 50 pick-ups throughout your journey
    public Achievement yesPickups;
    
    //22. Martial Ninja - Beat the game without throwing any knives
    public Achievement martialNinja;
    
    //23. Distance Ninja - Beat the game without using any melee attacks
    public Achievement distanceNinja;
    
    //24. Master Ninja - Obtain all other Achievements
    public Achievement allClear;

    #endregion

    public int pickupCounter;

    // Start is called before the first frame update
    void Start()
    {
        InitializeAchievements();
    }

    // Update is called once per frame
    //void Update()
    //{
    //   
    //}

    private void InitializeAchievements()
    {
        clearLevel1.PrimeAchievement("Entering the Jungle", "Clear level 1");
        clearLevel2.PrimeAchievement("Jungle with a View", "Clear level 2");
        clearLevel3.PrimeAchievement("Source of the Coruption", "Clear Level 3");
        speedClear1.PrimeAchievement("Quick Ninja", "Clear level 1 in under 1:30 minutes", 90.0f);
        speedClear2.PrimeAchievement("Hasty Ninja", "Clear level 2 in under 1:30 minutes", 90.0f);
        speedClear3.PrimeAchievement("Untrackable Ninja", "Clear level 3 in under 1:30 minutes", 90.0f);
        speedClearBoss.PrimeAchievement("Coup de Grace", "Defeat White Face in under 45 Seconds", 45.0f);
        badEnding.PrimeAchievement("The Coruption Lingers", "Obtain the Bad Ending");
        goodEnding.PrimeAchievement("The Coruption is Cleansed", "Obtain the Good Ending");
        genecide1.PrimeAchievement("Level 1 Genocide", "Kill all enemies in Level 1");
        genecide2.PrimeAchievement("Level 2 Genocide", "Kill all enemies in Level 2");
        genecide3.PrimeAchievement("Level 3 Genocide", "Kill all enemies in Level 3");
        pacifist1.PrimeAchievement("Level 1 Mostly Pasifist", "Only Kill Daichi in Level 1");
        pacifist2.PrimeAchievement("Level 2 Mostly Pasifist", "Only Kill Renu in Level 1");
        pacifist3.PrimeAchievement("Level 3 Mostly Pasifist", "Only Kill Bill in Level 1");
        mario.PrimeAchievement("Not an Italian Plumber", "Jump on an enemies head");
        bully.PrimeAchievement("Being a Bully", "Push an enemy into a pit");
        trapless.PrimeAchievement("No Traps Activated", "Clear the game without activating any swinging traps");
        noLifeLoss.PrimeAchievement("Expert Ninja", "Clear the game without losing any lives");
        noPickups.PrimeAchievement("Proud Ninja", "clear game without grabing any pick-ups");
        yesPickups.PrimeAchievement("Resourceful Ninja", "Grab a total of 50 pick-ups throughout your journey");
        martialNinja.PrimeAchievement("Martial Ninja", "Beat the game without throwing any knives");
        distanceNinja.PrimeAchievement("Distance Ninja", "Beat the gamewithoutusing any melee attacks");
        allClear.PrimeAchievement("Master Ninja", "Obtain allother Achievements");
    }
}
