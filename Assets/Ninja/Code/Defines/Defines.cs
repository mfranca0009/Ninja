
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
    AttackEffects = 2,
    DamageEffects = 3,
    ItemEffects = 4
}

public enum AudioMixerGroup
{
    Master = 1,
    BGMusic = 2,
    MasterSFX = 3,
    AttackSFX = 4,
    DamageSFX = 5,
    ItemSFX = 6,
    
    Max = 6
}