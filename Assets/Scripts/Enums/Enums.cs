namespace Enums
{
    public enum EPopUpState
    {
        None = 0,   

        LoadingScreen,
        ChestOpeningScreen,
        LevelUpScreen,
        SpellInfoPopUp,
        StateEffectPopUp,
        RuneSelectionPopUp,

        ErrorMessagePopUp,
    }

    public enum EGameMode
    {
        Solo,
        Multi
    }

    public enum ECharacter
    {
        Kahnan,
        Alexander,
        Srug,
        Marcus,
        Bruh,

        Count
    }

    public enum ESpell
    {
        Arrow,
        Blazeburst,
        Fireball,
        FireBomb,
        Heal,
        Counter,
        Invisibility,
        AxeThrow,
        Erasement,
        Rempart,
        IronSkin,
        PoisonSpit,
        PoisonFury,
        ShadowRealm,
        Curse,
        Torment,
        Sanctuary,
        ScorchedEarth,
        BerzerkerRage,
        SmokeBomb,
        ArcticToundra,
        Blizzard,
        FrozenOrb,
        FrostbiteTouch,
        Silence,
        PyroBurst,

        Count
    }

    public enum ESpellType
    {
        Projectile,
        InstantSpell,
        Aoe,
        Counter,
        Jump,
        Zone,
        Buff,

        Count
    }

    public enum ERarety
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum ERune
    {
        None,
        Frost,
        Fire,
    }

    public enum EAppState
    {
        /// <summary> entry point </summary>
        Release,
        /// <summary> screen displayed between 2 scenes </summary>$
        LoadingScreen,
        /// <summary> main menu of the application </summary>
        MainMenu,
        /// <summary> setting up a lobby before the game starts</summary>
        Lobby,
        InGame,

        Count
    }

    public enum EGameState
    {
        /// <summary> waiting for player data & connections </summary>
        WaitingForConnection,
        /// <summary> The GameManager received all data and clients, is now preparing the game (spawning players, UI, etc..) </summary>
        PreparingGame,
        /// <summary> all players are connected, playing the intro before starting the game </summary>
        Intro,
        /// <summary> game is currently running </summary>
        GameRunning,
        /// <summary> game is over </summary>
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

    public enum ESpellTarget
    {
        None,

        EnemyZone,
        AllyZone,
        Free,
        Self,
        FirstAlly,
        FirstEnemy,
    }

    public enum EJumpType
    {
        None,

        Curve,
        Dash,
        Teleport,
    }

    public enum EListEvent
    {
        Add,
        Remove,
    }

    public enum ESpawnLocation
    {
        None = 0,

        Caster,                 // spawn of the center of the caster
        CasterFeets,            // spawn at the feets of the caster
        CasterSpellSpawn,       // spawn at the spell spawn of the caster

        Target,
        TargetFeets,
        TargetSpellSpawn,

        Mouse,                  // on the mouse location    
        MouseGround,            // on the ground at the mouse location
    }

    public enum EStateEffect
    {
        // default effect
        None,

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
        IronSkin,
        Cursed,
        Silence,
        Scorched,

    }

    public enum EStateEffectProperty
    {
        None,

        Duration,
        MaxStacks,
        SpeedBonus,
        Shield,
        ResistanceFix,
        ResistancePerc,
        BonusDamages,
        BonusDamagesPerc,
        LifeSteal,
        MissingLifeFactor,
        Damages,
        Tick,
        TickDamages,
        TickHeal,
        TickShield,
    }

    public enum EAnimation
    {
        None,
        CastShootStraight,
        CastShoot,
        CancelCast,
        Counter,
        Jump,
        CastAOE,
        Win,
        Loss,
        CastBuff,
    }

    public enum ECounterType
    {
        None,

        Proc,
        Block,
        Reflect,
        ApplyStateEffect
    }

    public enum ELogType
    {
        None = 0,

        Normal,
        Warning,
        Error,
        FatalError
    }

    public enum ELogTag
    {
        None = 0,

        System, 
        Network,
        Gameplay,
        Debug,
    }

    public enum ERewardType
    {
        Golds,
        Spell,
    }

    public enum EChestType
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum EChestState
    {
        Idle,
        Ready,
        Opening,
    }
}