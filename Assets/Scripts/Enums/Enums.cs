namespace Enums
{
    public enum ECharacter
    {
        GreenArcher,
        Reaper,

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

        Count
    }

    public enum ESpellType
    {
        Projectile,
        InstantSpell,
        Aoe,
        Counter,

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

        Count
    }

    public enum EAnimation
    {
        CastShootStraight,
        CastShoot,
        CancelCast,
        Counter,

        Count
    }
}