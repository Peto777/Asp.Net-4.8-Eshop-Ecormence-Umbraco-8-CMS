using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Util
{
    public class RandomUtil
    {
        public static Random random = new Random();

        public static int GetRandomInt(int from, int to)
        {
            return random.Next(from, to);
        }

        public static IEnumerable<int> GetRandomIntList(List<int> values, int cnt)
        {
            return values.Shuffle().Take(cnt);
        }

        public static List<int> GetIntList(int min, int max, int cnt)
        {
            if (cnt > max)
            {
                cnt = max;
            }
            Hashtable ht = new Hashtable();
            List<int> ret = new List<int>();

            while (cnt > 0)
            {
                int num = random.Next(min, max + 1);

                if (!ht.ContainsKey(num))
                {
                    ret.Add(num);
                    ht.Add(num, num);
                    cnt--;
                }
            }

            return ret;
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(RandomUtil.random);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            List<T> buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
