using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Scripts.Utils
{
    public static class CommonUtils
    {

        #region Class Implementation

        public static T GetRequiredComponent<T>(ref T reference, Func<T> func)
        {
            if (reference == null && func != null)
            {
                reference = func();
            }

            return reference;
        }

        public static List<T> SelectNotNull<T>(this List<T> checkList)
        {
            return checkList.Where(c => c != null).ToList();
        }

        public static List<T> ToList<T>(this IEnumerable<T> list)
        {
            var newList = new List<T>();
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!(enumerator.Current is T item)) {
                    continue;
                }
                newList.Add(item);
            }

            return newList;
        }

        #endregion
        
        
    }
}