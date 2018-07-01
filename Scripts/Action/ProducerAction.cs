using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts;

public class ProducerAction : ActionBase {

    private static readonly float RespawnTime = 3.0f;
    private float lastRespawn = 0;

    // Use this for initialization
    void Start () {
        lastAct = 0;
        ActDuration = Constant.ActDuraion;
    }
	
	// Update is called once per frame
	void Update () {
        
        var pMove = GetComponent<ProducerMove>();

        // 移動中でないとき、アクションできる
        if (!pMove.isMoving() && Time.time - lastAct > ActDuration)
        {
            IsActing = false;
            ActDuration = Constant.ActDuraion;
            if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
            {
                pushObstacle(pMove.Direction);
            }
        }

        // リスポーン中の無敵管理
        if (GetComponent<Producer>().IsRespawing && Time.time - lastRespawn > RespawnTime)
        {
            GetComponent<CircleCollider2D>().enabled = true;     // 無敵解除
            GetComponent<Producer>().IsRespawing = false;       // リスポーン中フラグを落とす    
        }
	}

    void pushObstacle(Vector2 dir)
    {
        // Rayを飛ばして障害物にぶつかった場合は動かせる
        // TODO: 現状箱があるかどうかは分かるけどどの箱か特定することができないのでとりあえずRaycastを使う
        //       箱にコライダーがついてると問題があるようならまた考える

        Vector2 pos = transform.position;
        pos = pos + center;
        int layerMask = LayerMaskConstant.Hako;
       
        RaycastHit2D hit = Physics2D.Linecast(pos, pos + dir * Constant.Step, layerMask);
        RaycastHit2D lastHit = new RaycastHit2D();
        Vector2 pPos = GetComponent<ProducerMove>().MapPosition;
        Vector2 targetMapPosition = pPos + new Vector2(dir.x, -dir.y);
        bool isHit = false;

        // 箱にぶつからなくなるまで隣にRayを伝播させる
        while (Map.IsObstacle(targetMapPosition) && hit.collider.GetComponent<HakoMove>() != null)
        {
            isHit = true;
            pos = pos + dir * Constant.Step;
            hit = Physics2D.Linecast(pos, pos + dir * Constant.Step, layerMask);
            targetMapPosition += new Vector2(dir.x, -dir.y);
        }

        if (isHit)
        {
            lastHit = hit; 
        }

        if ( lastHit.collider != null)
        {
            HakoMove hako = lastHit.collider.GetComponent<HakoMove>();
            hako.Move(dir);
            IsActing = true;
            ActDuration = 0.25f;
            lastAct = Time.time;
        }   
    }

    
    public void Die()
    {
        Producer producer = GetComponent<Producer>();
        producer.Life--;
        if (producer.Life <= 0)
        {
            // ライフが0になったら消滅する
            // 倒れたスプライトにする
            producer.GetComponent<Animator>().SetBool("Dead", true);   // Pを寝かせる

        }
        else
        {
            // ライフが1以上あればリスポーンする
            Respawn();
        }
    }

    // リスポーン
    public void Respawn()
    {
        ProducerMove pMove = GetComponent<ProducerMove>();

        // マップから除外
        Map.RemoveObject(pMove.MapPosition);

        // コライダーを無効化(無敵)
        GetComponent<CircleCollider2D>().enabled = false;

        // 初期位置にリスポーン
        pMove.Reset();

        // マップに設定
        Map.SetObject(MapObject.PLAYER, ProducerMove.StartMapPos);

        GetComponent<Producer>().IsRespawing = true;
        lastRespawn = Time.time;
    }
}
