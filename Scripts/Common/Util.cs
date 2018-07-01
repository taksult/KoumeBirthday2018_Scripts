using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class GameUtil
    {
        public static readonly System.Random Rand = new System.Random();
    }

    public static class CommonExtentions {

        // ListにPop,Pushを追加
        public static T Pop<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                var ret = list[0];
                list.RemoveAt(0);
                return ret;
            }
            else
            {
                return default(T);
            }
        }

        public static void Push<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
        }

        public static List<T> Shuffle<T>(this List<T> list)
        {

            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = UnityEngine.Random.Range(0, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }

            return list;
        }


        // Enumに自分の次の値を取得するNextを追加
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}
