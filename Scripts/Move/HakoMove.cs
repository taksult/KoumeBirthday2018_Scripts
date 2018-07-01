using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts;

public class HakoMove : MoveBase {

    public int id;

	// Use this for initialization
	void Start () {
        _speed = _baseSpeed;
        _dest = transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        // 連続っぽく移動する
        Vector3 p = Vector3.MoveTowards(transform.position, _dest, _speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

        // MovePosition()で移動しきらない問題対策
        if ((transform.position - _dest).magnitude <= (Constant.Step / 100))
        {
            transform.position = _dest;
        }
    }
}
