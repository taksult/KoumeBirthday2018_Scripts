using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{

    public class GameSystem
    {

    }

    public class Constant
    {

        public static readonly int Step = 64;
        public const int MAPSIZE = 13;

        public static readonly float ActDuraion = 0.25f;

        public static readonly Vector2 MapOrigin = new Vector2(-384, 320);
        public static readonly Vector2 OutOfMap = new Vector2(-1, -1);

        public static readonly List<Vector2> MobMoveList = new List<Vector2>{
                // new Vector2 (-192,330),
                new Vector2 (-128,330),
                new Vector2 (128,330),
                // new Vector2 (192,330),
                /*
                new Vector2 (-256,384),
                new Vector2 (-192,384),
                new Vector2 (192,384),
                new Vector2 (256,384),
                */
                // new Vector2 (-256,448),
                new Vector2 (-192,448),
                new Vector2 (192,448),
                // new Vector2 (256,448),
            };

        public static readonly List<int> MapObjetGenList = new List<int>
        {
            3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
        };

    }

    public class LayerMaskConstant
    {
        public static readonly int Player = LayerMask.GetMask(new string[] { "Player" });
        public static readonly int Koume = LayerMask.GetMask(new string[] { "Koume" });
        public static readonly int Hako = LayerMask.GetMask(new string[] { "Obstacle" });
        public static readonly int Light = LayerMask.GetMask(new string[] { "Light" });
    }

    public enum GameStatus
    {
        Start = 1,
        Playing = 2,
        GameOver = 3,
        Clear = 4,
    }

    public enum MapObject
    {
        NONE = 0,
        PLAYER = 1,
        KOUMECHANG = 2,
        HAKO = 3,
        KABE = 4,
        GOAL = 999,
    }

    public enum TimerPhase
    {
        Start = 0,
        Phase1 = 15,
        Phase2 = 30,
        Phase3 = 45,
        End = 60,
        // Phase4 = 60,
        // End = 65,
    }
}
