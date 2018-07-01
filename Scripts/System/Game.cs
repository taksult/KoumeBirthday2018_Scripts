using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts
{
    
    class Game : MonoBehaviour
    {
        private bool GameEnd = false;
        public static bool IsGameClear = false;
        public float StartTime { get; set; }
        public float TimerCount
        {
            get;
            set;
        }
        public GameStatus GameState = GameStatus.Start;
        private TimerPhase currentPhase;
        private TimerPhase nextPhase;
        private float LastUpdate;

        public void Start()
        {
            GameEnd = false;
            IsGameClear = false;
            StartTime = Time.time;
            GameState = GameStatus.Playing;
            currentPhase = TimerPhase.Start;
            nextPhase = currentPhase.Next();
            GameObject.Find("TimerCounter").GetComponent<Animator>().SetFloat("Phase", (float)currentPhase + 0.1f);
            LastUpdate = Time.time;
    }

        public void FixedUpdate()
        {
            if (!GameEnd)
            {
                TimerCount = Time.time;

                // 制限時間処理
                if (TimerCount - StartTime > (float)nextPhase)
                {
                    changeNextPhase();
                }


                // タイムアップでゲームクリア
                if (currentPhase == TimerPhase.End)
                {
                    GameObject producer = GameObject.Find("producer");
                    producer.GetComponent<Animator>().SetBool("Respawn", false);
                    producer.GetComponent<CircleCollider2D>().enabled = false;   // Pのコライダーを外す
                    Map.RemoveObject(producer.GetComponent<ProducerMove>().MapPosition);    // マップからPを除外
                    DisableScriptsOn(GameObject.Find("producer"));
                    GameObject.Find("goal_shade").GetComponent<SpriteRenderer>().enabled = false;   // パーティ会場の影を消す
                    GameObject.Find("door").GetComponent<SpriteRenderer>().enabled = false;    // ドア非表示
                    IsGameClear = true;
                    GameEnd = true;
                }

                // タイムアップ前に小梅ちゃんがゴール or プロデューサーのライフが尽きたら
                // ゲームオーバー
                else if (GameObject.Find("koume").GetComponent<KoumeMove>().Mode == KoumeMove.MoveMode.Goal ||
                            GameObject.Find("producer").GetComponent<Producer>().Life <= 0 )
                {

                    // 小梅ちゃんとプロデューサーのスクリプトを無効化
                    // DisableScriptsOn(GameObject.Find("koume"));
                    GameObject producer = GameObject.Find("producer");

                    producer.GetComponent<CircleCollider2D>().enabled = false;   // Pのコライダーを外す
                    Map.RemoveObject(producer.GetComponent<ProducerMove>().MapPosition);    // マップからPを除外
                    DisableScriptsOn(GameObject.Find("producer"));
                    GameOver();
                    GameObject.Find("full_shade").GetComponent<SpriteRenderer>().enabled = true;   // ゲームオーバー表示
                    GameObject.Find("gameover").GetComponent<SpriteRenderer>().enabled = true;   // ゲームオーバー表示
                    GameEnd = true;
                }

                // 1秒ごとにパーティ準備勢を動かす
                if (Time.time - LastUpdate >= 2.0f)
                {
                    var mMoveList = new List<Vector2>(Constant.MobMoveList);
                    mMoveList.Shuffle();
                    GameObject.Find("mirei").transform.position = (Vector3)mMoveList.Pop() + new Vector3(0, 0, 1);
                    GameObject.Find("morikubo").transform.position = (Vector3)mMoveList.Pop() + new Vector3(0, 0, 1);
                    GameObject.Find("shoko").transform.position = (Vector3)mMoveList.Pop() + new Vector3(0, 0, 1);
                    GameObject.Find("sachiko").transform.position = (Vector3)mMoveList.Pop() + new Vector3(0, 0, 1);
                    LastUpdate = Time.time;
                }
            }
        }

        public void GameClear()
        {
           
        }

        public void GameOver()
        {
        }

        public static void DisableAllScripts()
        {
           
            foreach (var o in (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                // 全てのオブジェクトを処理
                if (o != GameObject.Find("Game"))
                {
                    DisableScriptsOn(o);
                }
            }
        }

        public static void DisableScriptsOn(GameObject o)
        {
            // 1つのオブジェクトのすべてのスクリプトを無効化
            foreach (var c in o.GetComponents<MonoBehaviour>())
            {
                if (c != null)
                {
                    c.enabled = false;
                }
            }

        }

        private void changeNextPhase()
        {
            currentPhase = currentPhase.Next();
            if (nextPhase != TimerPhase.End)
            {
                nextPhase = currentPhase.Next();
            }
            GameObject.Find("TimerCounter").GetComponent<Animator>().SetFloat("Phase", (float)currentPhase + 0.1f);
            GameObject.Find("table").GetComponent<Animator>().SetFloat("Phase", (float)currentPhase + 0.1f);
        }


    }
}
