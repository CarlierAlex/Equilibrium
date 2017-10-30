using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Level
{
    MAIN_MENU,
    LEVEL1,
    LEVEL2,
    LEVEL3,
    LEVEL4
};

public enum EnemyType
{
    NONE,
    GRUNT,
    GUNNER,
    SIEGE,
    HYBRID
};

public enum Faction
{
    NONE,
    DARK,
    LIGHT
};

public struct EnemySpawnData
{
    public EnemyType type;
    public Faction faction;
    public float delay;
};

public enum EnemyState
{
    NONE,
    IDLE,
    MOVE,
    ATTACK,
    EXPLODE,
    SWITCH,
    DEAD,
    STAGGER,
    DESTROY
};

public enum Zone
{
    NONE = 0,
    DARK = 1,
    LIGHT = 2
};

public enum ControlPlayer
{
    NONE,
    PLAYER1,
    PLAYER2
};

public enum ControlSettings
{
    NONE,
    KEYBOARD,
    CONTROLLER1,
    CONTROLLER2
};

public enum ControlAccess
{
    NONE = 0,
    ATTACK = 1,
    BEAM = 2,
    DASH = 3,
    FULL = 10
};

public enum PlayerState
{
    IDLE,
    WALK,
    DASH,
    ATTACKCHARGE,
    ATTACK,
    BEAM,
    STAGGER,
    DEAD
};

public enum AnimationStep
{
    IDLE,
    MOVE,
    DASH,
    ATTACKCHARGE,
    BEAM,
    ATTACK,
    EXPLODE,
    DEATH,
    DEAD,
    SWITCH,
    STAGGER
};

public enum AnimationPhase
{
    NONE,
    ATTACKSTART,
    ATTACKSTOP,
    RESET
};

public class HeaderList : MonoBehaviour {


}
