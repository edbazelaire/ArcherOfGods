namespace Enums
{
    public enum ECharacter
    {
        Reaper,
        Kahnan,
        Alexander,

        Count
    }

    public enum ESpell
    {
        Arrow,
        StraightArrow,
        Fireball,
        FireBomb,
        Heal,
        Counter,
        Invisibility,
        AxeThrow,
        Erasement,
        Rempart,

        Count
    }

    public enum ESpellType
    {
        Projectile,
        InstantSpell,
        Aoe,
        Counter,
        Jump,

        Count
    }

    public enum EGameState
    {
        MainMenu,
        Lobby,
        LoadingScreen,
        InGame,
        GameOver,

        Count
    }

    public enum ESpellTrajectory 
    {
        Hight,
        Curve,
        Straight,

        Count
    }

    public enum EStateEffect
    {
        // default effect
        Stun,
        Frozen,
        Invulnerable,
        Invisible,

        // knockback effects
        Knockback,

        // slow effects
        Frost,
        Slow,

        // tick effects
        Burn,
        Poison,

        Uncontrollable,
        Jump,

        Count
    }

    public enum EAnimation
    {
        CastShootStraight,
        CastShoot,
        CancelCast,
        Counter,
        Jump,

        Count
    }

    public enum ECounterType
    {
        None,

        Proc,
        Block,
        Reflect,
        ApplyStateEffect
    }
}