using System;
using System.Collections.Generic;
using UnityEngine;


public class Producer : MonoBehaviour
{
    // キャラクターの状態系の変数は全部ここにまとめればよかったな...
    // 他のスクリプトから弱参照で持たせるイメージ？
    private int _life;
    private bool _isRespawing;

    public int Life {
        set {
            _life = value;
            GameObject.Find("Life").GetComponent<Animator>().SetInteger("Life", value);
        }
        get
        {
            return _life ;
        }
    }
    
    public bool IsRespawing
    {
        set
        {
            _isRespawing = value;
            GetComponent<Animator>().SetBool("Respawn", value);

        }
        get
        {
            return _isRespawing;
        }
    }

    private void Start()
    {
        _life = 3;
        _isRespawing = false;
    }

    private void Update()
    {
        
    }

}

