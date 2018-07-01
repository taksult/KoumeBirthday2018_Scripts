using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Assets.Scripts;
using AStarAlg;


public class Map : MonoBehaviour {

    public static readonly int[,] MapBase = new int[Constant.MAPSIZE, Constant.MAPSIZE]{
            { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
            { 4, 0, 3, 0, 0, 0, 1, 0, 3, 0, 0, 0, 4 },
            { 4, 0, 0, 0, 3, 0, 0, 0, 0, 3, 0, 0, 4 },
            { 4, 3, 0, 3, 3, 3, 0, 3, 3, 3, 3, 0, 4 },
            { 4, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 4 },
            { 4, 0, 3, 0, 3, 0, 0, 0, 0, 3, 0, 0, 4 },
            { 4, 0, 3, 0, 3, 3, 3, 3, 0, 0, 0, 3, 4 },
            { 4, 0, 0, 0, 0, 0, 0, 3, 0, 3, 0, 3, 4 },
            { 4, 0, 3, 0, 3, 0, 3, 0, 0, 3, 0, 0, 4 },
            { 4, 0, 0, 0, 3, 0, 3, 3, 3, 3, 3, 0, 4 },
            { 4, 3, 0, 3, 3, 0, 0, 3, 0, 0, 3, 0, 4 },
            { 4, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 4 },
            { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
        };

    public static int[,] map;

    public static readonly Vector2 Goal = new Vector2(6.0f, 1.0f);      // ゴール座標
    public static readonly Vector2 originOffset = new Vector2(-6 * Constant.Step, 5 * Constant.Step);   // ゲーム空間上のマップ原点位置
    private static int numHako = 0; 

    private static SemaphoreSlim promise = new SemaphoreSlim(1, 1); // Promiseっぽく使ってmap書き換えの排他処理できる？

    // マップ情報生成
    void Start () {
        // マップオブジェクト生成
        map = new int[Constant.MAPSIZE, Constant.MAPSIZE];
        List<int> genList = new List<int>(Constant.MapObjetGenList);
        genList.Shuffle();
        for (int i = 0; i < Constant.MAPSIZE; i++)
        {
            for (int j = 0; j < Constant.MAPSIZE; j++)
            {
                // map[i, j] = MapBase[i, j];
                // マップ自動生成
                if (i != 0 && i != 12 && j != 0 && j != 12)
                {
                    if (i == 1 && j == 6)
                    {
                        map[i, j] = 1;
                    }
                    else if (i == 11 && j == 6)
                    {
                        map[i, j] = 2;
                    }
                    else
                    {
                        map[i, j] = genList.Pop();
                    }
                }
                else
                {
                    map[i, j] = 4;
                }
            }
        }
        GameObject pTile0 = (GameObject)Resources.Load("tile0");
        GameObject pTile1 = (GameObject)Resources.Load("tile1");
        List<GameObject> hakos = new List<GameObject>();
        hakos.Add((GameObject)Resources.Load("hako1"));
        hakos.Add((GameObject)Resources.Load("hako2"));

        // 左上始まり
        for (int i = 0; i < Constant.MAPSIZE; i++)
        {
            for (int j = 0; j < Constant.MAPSIZE; j++)
            {
                Vector3 position = (Vector3)originOffset + new Vector3(i * Constant.Step, -j * Constant.Step, 0);   //オブジェクトを置く座標

                // マップ情報にしたがってオブジェクトを配置
                MapObject o = (MapObject)map[j, i];

                // 壁以外にはタイルを配置
                if (o != MapObject.KABE)
                {

                }

                // 箱を配置
                if (o == MapObject.HAKO)
                {
                    GameObject hako = (GameObject)Instantiate(hakos[GameUtil.Rand.Next() % 2], position, Quaternion.identity);
                    hako.name = "hako_clone" + numHako;
                    numHako++;
                    hako.GetComponent<HakoMove>().MapPosition = new Vector2(i, j);
                    hako.transform.parent = GameObject.Find("Obstacles").transform;
                } 
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // マップ上のオブジェクトをfromからtoへ移動
    // @return 移動成功(true) / 失敗(false)
    // memo: 排他制御必要？ -> Unityはシングルスレッドらしい ひとまず早いもの勝ちということで...
    public static bool MoveObject(Vector2 from, Vector2 to)
    {
        bool res = false;

        // promise.WaitAsync(); // ロックを取得する
        lock (map)
        {
            if (!IsWithinMapRange(from) || !IsWithinMapRange(to))
            {
                // マップ外指定
                Debug.Log("index out of map bounds");
                res = false;
            }

            else if (!IsVacant(to))
            {
                // 移動先にオブジェクトがある
                // Debug.Log("can't move map(" + to.x + "," + to.y + ")");
                res = false;
            }

            else
            {
                bool isHakoMove = (map[(int)from.y, (int)from.x] == (int)MapObject.HAKO);
                if (isHakoMove && to == Goal)
                {
                    // 箱をゴールに移動させることはできない
                    res = false;
                }
                else
                { 
                    // Debug.Log(map[(int)from.y, (int)from.x] + " is " + to + " from " + from);
                    map[(int)to.y, (int)to.x] = map[(int)from.y, (int)from.x];  // オブジェクト移動
                    map[(int)from.y, (int)from.x] = (int)MapObject.NONE;  // 元の位置をNONE(空き)に
                
                    if (isHakoMove)
                    {
                        KoumeMove kMove = GameObject.Find("koume").GetComponent<KoumeMove>();
                        kMove.UpdatePath();
                    }
                    res = true;
                }
            }

            return res;
        }
        /*
        finally
        {
            promise.Release();
        }
        */
        /*
        string mapString = "";
        for (float i = 0; i < Constant.MAPSIZE; i++)
        {
            for (float j = 0; j < Constant.MAPSIZE; j++)
            {
                mapString += " "+ map[(int)i, (int)j] + ",";
                // mapString +=  j + "," + i  + ":" +  map[i, j] + ",";
                // mapString += "\r\n";
            }
            mapString += "\r\n";
        }
        Debug.Log(mapString);
        */

        // return res;
    }

    public static void RemoveObject(Vector2 at)
    {
        if (map[(int)at.y, (int)at.x] == (int)MapObject.HAKO)
        {
            KoumeMove kMove = GameObject.Find("koume").GetComponent<KoumeMove>();
            kMove.UpdatePath();
        }
        map[(int)at.y, (int)at.x] = (int)MapObject.NONE;
    }

    // 指定インデックスにオブジェクトをを配置する
    public static void SetObject(MapObject obj, Vector2 on)
    {

        if (!IsWithinMapRange(on))
        {
            // マップ外指定
            Debug.Log("index out of map bounds" + on);
            return;
        }
       
        map[(int)on.y,(int)on.x] = (int)obj;  // オブジェクト設定

        return;
    }

    // 指定インデックスにあるオブジェクト番号を返す
    public static MapObject GetMapObject(Vector2 at)
    {
        if (!IsWithinMapRange(at))
        {
            // マップ外指定
            Debug.Log("index out of map bounds" + at);
            return MapObject.NONE;
        }

        return (MapObject)map[(int)at.y, (int)at.x];

    }

    // 指定インデックスが空き(MapObject.NONE)かどうか判定
    public static bool IsVacant(Vector2 at)
    {
        if (!IsWithinMapRange(at))
        {
            // マップ外指定
            Debug.Log("index out of map bounds" + at);
            return false;   // マップ外はfalse
        }

        return map[(int)at.y, (int)at.x] == (int)MapObject.NONE;
    }
    
    // 指定インデックスが箱かどうか判定
    public static bool IsObstacle(Vector2 at)
    {
        if (!IsWithinMapRange(at))
        {
            // マップ外指定
            Debug.Log("index out of map bounds" + at);
            return false;   // マップ外はfalse
        }

        return map[(int)at.y, (int)at.x] == (int)MapObject.HAKO;
    }


    // 指定インデックスがマップ内かどうかを判定する
    public static bool IsWithinMapRange(Vector2 at)
    {
        return Enumerable.Range(0, Constant.MAPSIZE).Contains((int)at.y) && Enumerable.Range(0, Constant.MAPSIZE).Contains((int)at.x);
    }



    public List<Point2> GetShortestPath(Vector2 start, Vector2 goal, int[,] map, bool allowdiag = false)
    {
        List<Point2> ret = new List<Point2>();
        Layer2D layer = new Layer2D();
      
        layer.Create(Constant.MAPSIZE, Constant.MAPSIZE); // TODO: 配列メソッドで取得する方法
        for (int i = 0; i < Constant.MAPSIZE; i++)
        {
            for (int j = 0; j < Constant.MAPSIZE; j++)
            {
                layer.Set(j, i, map[i, j]);
            }
        }

        var mgr = new ANodeMgr(layer, (int)goal.x, (int)goal.y, allowdiag);
        ANode node = mgr.OpenNode((int)start.x, (int)start.y, 0, null);
        if (node == null)
        {
            Debug.Log("node is null start" + start); 
        }
        int cnt = 0;
        while (cnt < 1000)
        {
            mgr.RemoveOpenList(node);
            // 周囲を開く
            mgr.OpenAround(node);
            // 最小スコアのノードを探す.
            node = mgr.SearchMinScoreNodeFromOpenList();
            if (node == null)
            {
                // 袋小路なのでおしまい.
                // Debug.Log("Not found path.");
                break;
            }
            if (node.X == (int)goal.x && node.Y == (int)goal.y)
            {
                // ゴールにたどり着いた.
                // Debug.Log("Success.");
                mgr.RemoveOpenList(node);
                node.DumpRecursive();
                // パスを取得する
                node.GetPath(ret);
                // 反転する
                ret.Reverse();
                break;
            }

        }

        return ret;
    }

    public List<Point2> GetShortestPath(Vector2 start)
    {
        List<Point2> ret = new List<Point2>();

        ret = GetShortestPath(start, Goal, map);

        return ret;
    }
}
