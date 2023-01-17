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

        #endregion
        
        
    }
}