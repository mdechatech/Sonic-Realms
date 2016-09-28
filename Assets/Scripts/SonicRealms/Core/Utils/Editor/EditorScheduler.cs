using System;
using System.Collections.Generic;
using UnityEditor;

namespace SonicRealms.Core.Utils.Editor
{
    [InitializeOnLoad]
    public class EditorScheduler
    {
        private static bool Initialized;
        
        private static List<RepeatingCallback> RepeatingItems = new List<RepeatingCallback>();

        static EditorScheduler()
        {
            EditorApplication.update += Update;
        }

        public static RepeatingCallback Repeat(Action callback, double updateRate)
        {
            var item = new RepeatingCallback
            {
                UpdateRate = updateRate,
                NextUpdateTime = EditorApplication.timeSinceStartup + 1/updateRate,
                Callback = callback
            };

            RepeatingItems.Add(item);
            return item;
        }

        public static bool Remove(RepeatingCallback callback)
        {
            return RepeatingItems.Remove(callback);
        }

        protected static void Update()
        {
            foreach (var item in new List<RepeatingCallback>(RepeatingItems))
            {
                if (EditorApplication.timeSinceStartup > item.NextUpdateTime)
                {
                    item.NextUpdateTime += 1/item.UpdateRate;
                    item.Callback();
                }
            }
        }

        public class RepeatingCallback
        {
            public double UpdateRate;
            public double NextUpdateTime;
            public Action Callback;
        }
    }
}
