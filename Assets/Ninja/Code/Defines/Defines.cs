
public enum AnimationLayers
{
    BaseAnimLayer = 0,
    AttackAnimLayer = 1
}

public enum UpdateMode
{
    Update = 0,
    FixedUpdate = 1
}

public enum AttackType
{
    None = 0,
    Melee = 1,
    Ranged = 2
}

public enum AttackState
{
    None = 0,
    LightAttack = 1,
    SlowAttack = 2,
    ThrowKnife = 3
}

public enum ItemType
{
    HealthPotion = 0,
    StrengthPotion = 1,
    ThrowingKnife  = 2
}

public enum AudioSourceType
{
    None = 0,
    Music = 1,
    MovementEffects = 2,
    AttackEffects = 3,
    DamageEffects = 4,
    ItemEffects = 5,
    UiEffects = 6
}

public enum AudioMixerGroup
{
    Master = 0,
    SoundEffects = 1,
    BgMusic = 2,
    
    Max = 3
}

public enum AchievementType
{
    TriggerType = 0,
    SpeedType = 1,
    CounterType = 2,
    
    Max = 3
}