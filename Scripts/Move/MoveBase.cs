using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class MoveBase : MonoBehaviour
    {
        public float _baseSpeed = 0.4f;
        protected float _speed;
        public Vector2 _mapPosition;

        protected Vector2 _direction = Vector2.zero;
        protected Vector2 _preMapPosition;
        protected Vector3 _dest = Vector2.zero;
        public static readonly Vector2 Center = new Vector2(0.5f * Constant.Step, 0.5f * Constant.Step);

        public static readonly List<Vector2> MoveDirList = new List<Vector2>
        {
            new Vector2(0,0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0)
        };

        public Vector2 Direction
        {
            get { return _direction;  }
            set { _direction = value;  }
        }

        public Vector2 MapPosition
        {
               get { return _mapPosition; }
               set { _mapPosition = value; }
        }


        public  bool Move(Vector2 dir)
        {
            if (!Map.MoveObject(_mapPosition, _mapPosition + new Vector2(dir.x, -dir.y)))
            {
                return false;
            }
            // preMapPosition = mapPosition;    // 移動完了時にマップからオブジェクトを削除するために以前の場所を覚えておく
            _mapPosition = _mapPosition + new Vector2(dir.x, -dir.y);    // 座標軸の向きが異なるため
            _dest = transform.position + (Vector3)(dir * Constant.Step);
            return true;
        }

        protected bool valid(Vector2 dir)
        {
            return Map.IsVacant(_mapPosition + new Vector2(dir.x, -dir.y));
        }

        public  bool isMoving()
        {
            return (transform.position != _dest);
        }

        public void ForceMove(Vector3 forceDest)
        {
            transform.position = forceDest;
            _dest = forceDest;
        }
    }
}
