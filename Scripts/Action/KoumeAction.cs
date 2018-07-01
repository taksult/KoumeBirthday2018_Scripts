using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts;

public class KoumeAction : ActionBase
{

    // Use this for initialization
    void Start()
    {
        lastAct = 0;
        ActDuration = Constant.ActDuraion;
    }

    // Update is called once per frame
    void Update()
    {
        Koume koume = GetComponent<Koume>();
        KoumeMove kMove = GetComponent<KoumeMove>();

        // 移動中でないとき、アクションできる
        if (!kMove.isMoving() && Time.time - lastAct > ActDuration)
        {
            IsActing = false;
            ActDuration = Constant.ActDuraion;
            GetComponent<Animator>().SetBool("Special", false);
            
            // プロデューサーが隣接しているとき、プロデューサーを殺す 
            if (IsNextToProducer())
            {
                KillProducer();
            }
            // ストレスが最大の時、特殊アクションを行う
            else if (koume.Stress >= Koume.StressMax)
            {
                int special = GameUtil.Rand.Next() % 2;

                switch(special)
                {
                    case 0:
                        // 8方向の障害物を破壊
                        BlowObstacles();    
                        break;
                    case 1:
                        // ワープ
                        Warp();        
                        break;
                   default:
                        break;
                }
                koume.ReduceStress(Koume.StressStep * 100);     // ストレスを減らす
            }
        }
    }

    // プロデューサーが隣(上下左右)にいればtrueを返す
    private bool IsNextToProducer()
    {
        Vector2 pos = (Vector2)transform.position + MoveBase.Center;
        Vector2 mapPos = GetComponent<KoumeMove>().MapPosition;
        foreach (var dir in MoveBase.MoveDirList)
        {
            // マップデータでプロデューサーかどうか判定
            Vector2 targetPos = mapPos + new Vector2(dir.x, -dir.y);
            if (Map.GetMapObject(targetPos) == MapObject.PLAYER)
            {
                // プロデューサーのコライダーが有効か判定
                int layerMask = LayerMaskConstant.Player;
                RaycastHit2D hit = Physics2D.Linecast(pos, pos + dir * Constant.Step, layerMask);
                if (hit.collider.GetComponent<Producer>() != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // プロデューサーを殺す
    private void KillProducer()
    {
        GameObject p = GameObject.Find("producer");
        ProducerAction pAction = p.GetComponent<ProducerAction>();
        pAction.Die();

        // プロデューサーを殺すとストレスが減る(TODO ストレスコントロールは定数にしたい)
        Koume koume = GetComponent<Koume>();
        koume.ReduceStress(Koume.StressStep * 350);

        IsActing = true;
        lastAct = Time.time;
        ActDuration = 1.0f;
    }

    // 8方向の障害物を破壊
    private void BlowObstacles()
    {
        GetComponent<Animator>().SetBool("Special", true);
        int layerMask = LayerMaskConstant.Hako;
        Vector3 origin = transform.position + (Vector3)MoveBase.Center;
        RaycastHit2D[] hits = 
                Physics2D.BoxCastAll(origin - new Vector3(0,Constant.Step*1.5f + 0.1f,0),
                                    new Vector3(Constant.Step * 1.5f, 0.1f),
                                    0,
                                    new Vector2(0, 1),
                                    Constant.Step * 3 - 0.1f,
                                    layerMask);
        
        foreach (var hako in hits)
        {
            Debug.Log(hako.collider);
            HakoMove hMove = hako.collider.GetComponent<HakoMove>();
            Map.RemoveObject(hMove.MapPosition);
            UnityEngine.Object.Destroy(hako.collider.transform.gameObject);
        }

        IsActing = true;
        lastAct = Time.time;
        ActDuration = 2.0f;
    }

    // ワープ
    private void Warp()
    {
        GetComponent<Animator>().SetBool("Special", true);
        // ワープ先を探す
        Vector2 dest;
        List<Point2> path = new List<Point2>();
        int count = 0;
        do
        {
            dest = new Vector2(GameUtil.Rand.Next(1, 11), GameUtil.Rand.Next(3, 8));
            if (!Map.IsObstacle(dest))
            {
                path = GameObject.Find("Map").GetComponent<Map>().GetShortestPath(dest);
                // 移動先が見つからなければ障害物破壊に切り替え
                if (count++ > 200)
                {
                    BlowObstacles();
                    break;
                }
            }

        } while (path.Count <= 5 || path.Count >= 8);

            // ワープする
            KoumeMove kMove = GetComponent<KoumeMove>();
        Map.RemoveObject(kMove.MapPosition);

        kMove.ForceMove(new Vector3(Constant.MapOrigin.x + dest.x * 64,
                                          Constant.MapOrigin.y - dest.y * 64, -2));

        // マップに設定
        Map.SetObject(MapObject.KOUMECHANG, dest);
        kMove.MapPosition = dest;
        kMove.UpdatePath();

        IsActing = true;
        lastAct = Time.time;
        ActDuration = 2.0f;
    }
}
