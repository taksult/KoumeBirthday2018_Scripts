using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts;

public class ProducerMove : MoveBase {

    public static readonly Vector3 StartPos = new Vector3(0, 256,-2);
    public static readonly Vector2 StartMapPos = new Vector2Int(6, 1);
    private Vector2 moveDir = MoveDirList[0];


    // Use this for initialization
    void Start()
    {
        transform.position = StartPos;
        _speed = _baseSpeed;
        _mapPosition =  StartMapPos;
        _dest = transform.position;
        _direction = MoveDirList[0];
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
        }

        var pAction = GetComponent<ProducerAction>();

        // 移動が完了している かつ アクション中でければ 次の移動ができる
        if (!isMoving() && !pAction.IsActing)
        {
            moveDir = MoveDirList[0];
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                _direction = MoveDirList[1];
                moveDir = MoveDirList[1];
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                _direction = MoveDirList[2];
                moveDir = MoveDirList[2];
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                _direction = MoveDirList[3];
                moveDir = MoveDirList[3];
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                _direction = MoveDirList[4];
                moveDir = MoveDirList[4];
            }

            if (moveDir != MoveDirList[0])
            {
                this.Move(moveDir);
            }

        }


        // Animation Parameters
        Vector2 dirAnim = (Vector2)(_dest - transform.position);
        /*
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
        */

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision enter:" + collision.collider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision enter 2D:" + collision.collider);
    }

    public void Reset()
    {
        Start();
    }

}
