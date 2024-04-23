namespace Enums
{
    public enum EPopUpState
    {
        None = 0,   

        // -- screens
        LoadingScreen,
        RewardsScreen,
        LevelUpScreen,
        ArenaPathScreen,

        // -- info PopUps
        SpellInfoPopUp,
        CharacterInfoPopUp,
        StateEffectPopUp,
        RuneSelectionPopUp,

        // -- menu PopUps
        ProfilePopUp,

        // -- message PopUps
        MessagePopUp,
        ErrorMessagePopUp,
        ConfirmBuyPopUp,
        ConfirmBuyItemPopUp,
        ConfirmBuyBundlePopUp,
    }

    public enum EGameMode
    {
        Solo,
        Multi
    }

    public enum EArenaType
    {
        FireArena,
        FrostArena,
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
        RockShower,
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
        FrostVenomBarrage,
        PyrotoxinMist,
        FireBarrage,

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

    public enum ECollectableType
    {
        None,
        Character,
        Spell,
        Rune
    }

    public enum ERune
    {
        None,
        FrostRune,
        FireRune,
        PoisonRune,
        CurseRune,
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
        /// <summary> Game started </summary>
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

        EnemyZoneStart,
        AllyZoneStart,
        EnemyZoneCenter,
        AllyZoneCenter,
    }

    public enum EMultiProjectileType
    {
        None,

        Line,
        Random,
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

    public enum ESpellProperty
    {
        Non,

        Heal,
        Damages,
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
        AttackSpeed,
        CastSpeed,
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

    public enum ECounterActivation
    {
        OnHitPlayer,        // activated when the player gets hit
        SelfTrigger,        // activated when the CounterSpell gets triggered
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
        Debug,

        Gameplay,
        Spells,
        Rewards,

        // AI
        AI,
        AIFinalDecision,
        AICheckers,
        AITaskAttack,
        AITaskMove,

    }

    public enum ERewardType
    {
        Currency,
        Chest, 
        Collectable
    }

    public enum ECurrency
    {
        Golds,
        Gems,
        Dollars
    }

    public enum EChest
    {
        // classics
        Common,
        Rare,
        Epic,
        Legendary,

        // fire chests
        Ember,
        Scorchstone,
        PyroMaster,

        // frost chests
        WintersBreath,
        Iceforged,
        FrostMaster,
    }

    public enum EChestAnimState
    {
        Idle,
        Ready,
        Opening,
    }

    public enum EChestLockState
    {
        Empty,
        Locked,
        Unlocking,
        Ready
    }

    public enum EAnimationUI
    {
        None,

        Pulse,
    }

    public enum EOrdering
    {
        None,

        Ascending,
        Descending,
    }


    public enum ELeague
    {
        None = 0,

        Bronze,
        Silver,
        Gold,
        Platinium,
        Diamond,
        Champion
    }

    public enum EStatData
    {
        None,

        PlayedGames,
        Wins,
    }

    public enum EBadge
    {
        None = 0,

        PlayedGame,
    }

    public enum EAchievementReward
    {
        None, 

        Title,
        Avatar,
        Border
    }
}