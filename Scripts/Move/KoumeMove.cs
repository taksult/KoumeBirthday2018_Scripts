using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Assets.Scripts;

public class KoumeMove : MoveBase
{

    public enum MoveMode
    {
        Stop = 0,
        Default = 1,
        Lost = 2,
        Chasing = 3,
        ChaseBuf = 4,
        Goal = 5,
        End = 6,
    }


    public static readonly Vector3 StartPos = new Vector3(0, -384, -2);
    private Vector2 StartMapPos = new Vector2Int(6, 11);
    public float moveInterval = 1.0f;
    private float _lastUpdate;

    public MoveMode Mode { get; set; }
    public readonly List<float> speedList = new List<float> {1.0f, 1.0f, 1.0f, 2.3f, 1.0f, 1.0f };
    public float _accelaration = 1.0f;

    private List<Point2> _path; 
    private int _playerDir;          // とりあえずこれでプロデーサーがいる方向を管理 
    private bool _chaseBuf = false;  // 追跡モード切り替わり時に+1マス追いかけるためのフラグ.........

    // Use this for initialization
    void Start()
    {
        transform.position = StartPos;
        _speed = _baseSpeed;
        _mapPosition = StartMapPos;
        _lastUpdate = Time.time;
        _dest = transform.position;
        Mode = MoveMode.Default;

        // KoumeChangからゴールまでの最短パスを計算
        UpdatePath();
    }

    void FixedUpdate()
    {
        // 連続っぽく移動する
        Vector3 p = Vector3.MoveTowards(transform.position, _dest, _speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

        // MovePosition()で移動しきらない問題対策
        if ((transform.position - _dest).magnitude <= (Constant.Step / 100))
        {
            transform.position = _dest;
            // Map.RemoveObject(preMapPosition); // マップの以前いた場所からオブジェクトを消す
        }

        // ==== 移動処理分岐 ==================================================================


        // ゴールに着いた場合
        if ((_mapPosition == Map.Goal && !isMoving() ) || Mode == MoveMode.Goal)
        {
            ChangeMode(MoveMode.Goal);
            // 全てのスクリプトを無効化
            // Game.DisableAllScripts();
            if (Game.IsGameClear)
            {
                Move(new Vector2(0, 1));
                GameObject.Find("clear").GetComponent<SpriteRenderer>().enabled = true;   // ゲームクリア表示
            }
        }

        // 直線上にプロデューサーがいる場合は追いかける
        else if (IsPlayerSeeable() && !isMoving())
        {
            ChangeMode(MoveMode.Chasing);
            ChasePlayer();
            _chaseBuf = true;
        }
        // プロデューサーを見失っても1マス余分に動く
        else if (_chaseBuf && !isMoving())
        {
            _chaseBuf = false;
            _lastUpdate = Time.time;
            ChangeMode(MoveMode.Chasing);
            ChasePlayer();
        }

        // その他の移動は前回の移動から一定時間経っていたら
        else if ( transform.position == _dest && Mathf.Abs(Time.time - _lastUpdate) > moveInterval / speedList[(int)Mode])
        {
            Vector2 next = NextNode();
            
            
            // ゴール未到達で次の経路が存在しない場合
            if(!Map.IsWithinMapRange(next))
            {
                _lastUpdate = Time.time;
                ChangeMode(MoveMode.Lost);
                MoveRandom();
            }

            // それ以外ならゴールへ向かう
            else
            {
                _lastUpdate = Time.time;
                if ( ChangeMode(MoveMode.Default) )
                {
                    // 状態変化があった場合は次のノードを取得しなおす
                    next = NextNode();
                }
                GoTowardGoal(next);
            }

            // 移動間隔短縮
            moveInterval -= 0.005f;
        }

        
        // ==================================================================================

        // Animation Parameters
        Vector2 dirAnim = (Vector2)(_dest - transform.position);
        if (Mode == MoveMode.Goal)
        {
            dirAnim = new Vector2(0, 1);
        }
        GetComponent<Animator>().SetFloat("DirX", dirAnim.x);
        GetComponent<Animator>().SetFloat("DirY", dirAnim.y);
        GetComponent<Animator>().SetInteger("MoveMode", (int)Mode);

    }

    bool Move(Vector2 dir)
    {
        if (!Map.MoveObject(_mapPosition, _mapPosition + new Vector2(dir.x, -dir.y)))
        {
            _path.Clear();
            return false;
        }
        _preMapPosition = _mapPosition;    // 移動完了時にマップからオブジェクトを削除するために以前の場所を覚えておく
        _mapPosition = _mapPosition + new Vector2(dir.x, -dir.y);    // 座標軸の向きが異なるため
        _dest = transform.position + (Vector3)(dir * Constant.Step);
        return true;
    }

    bool Valid(Vector2 dir)
    {
        return Map.IsVacant(_mapPosition + new Vector2(dir.x, -dir.y));
    }

    // ゴールに向かう
    void GoTowardGoal(Vector2 next)
    {
        Vector2 dir;
        // ゴールまでの最短経路を辿る。pathが空ならランダム移動(TODO:)
        dir = new Vector2((int)(next.x - _mapPosition.x), -(int)(next.y - _mapPosition.y));
        Move(dir);
    }

    void MoveRandom ()
    {
        // 閉じ込め判定
        bool isClosed = true;
        foreach (var d in MoveDirList)
        {
            if (Map.IsVacant(_mapPosition + new Vector2(d.x, -d.y)))
            {
                isClosed = false;
            }
        }

        if (isClosed)
        {
            // 閉じ込め状態の場合、動かない
            return;
        }

        // 動ける場合
        Vector2 dir;
        dir = MoveDirList[GameUtil.Rand.Next() % 4 + 1];

        while (!Map.IsVacant(_mapPosition + new Vector2(dir.x, -dir.y)))
        {
            // 移動先は移動可能な場所からランダム
            dir = MoveDirList[GameUtil.Rand.Next() % 4 + 1];
        }
        Move(dir);
    }

    // プロデューサーを追いかける
    void ChasePlayer ()
    {
        Move(MoveDirList[_playerDir]);
    }

    // 直線上にプロデューサーがいるか否か(TODO: ここに書くべきではない気がする)
    bool IsPlayerSeeable ()
    {
        bool res = false;
        Vector2 pos = transform.position;
        pos = pos + MoveBase.Center;
        for (int i = 1; i < 5; i++)
        {
            int layerMask = LayerMaskConstant.Player + LayerMaskConstant.Hako;
            RaycastHit2D hit = Physics2D.Linecast(pos, pos + MoveDirList[i] * 2000, layerMask);
            
            // Debug.Log("hit.collider:" + hit.collider);
            if (hit.collider == GameObject.Find("producer").GetComponent<Collider2D>())
            {
                res = true;
                _playerDir = i;
            }
        }

        return res;
    }

    bool ChangeMode(MoveMode m)
    {
        bool ret = false;
        // 移動モードが切り替わったら経路を再計算する
        if (Mode != m)
        {
            ret = true;
            Mode = m;
            if (m != MoveMode.Goal)
            {
                UpdatePath();
            }
        }
        // 移動スピード更新
        _speed = _baseSpeed * speedList[(int)m] + _accelaration;

        if(Game.IsGameClear)
        {
            _speed = _baseSpeed * 2.0f;
        }

        return ret;
    }

    public void UpdatePath()
    {
        // Debug.Log("Koume Position:" + mapPosition);
        // path = GameObject.Find("Map").GetComponent<Map>().GetShortestPath(mapPosition);
        _path = GameObject.Find("Map").GetComponent<Map>().GetShortestPath(_mapPosition);
        if (_path.Count != 0)
        {
            _path.Pop(); // スタートノードを捨てる
        }

        // 経路再計算 = 箱が動かされるごとにストレスを加算する
        Koume koume = GetComponent<Koume>();
        koume.IncreaseStress(Koume.StressStep * 25);
    }

    private Vector2 NextNode()
    {
        if (_path.Count > 0)
        {
            Point2 p = _path.Pop();
            return new Vector2(p.x,p.y);
        }
        else
        {
            return Constant.OutOfMap;
        }
    }
}