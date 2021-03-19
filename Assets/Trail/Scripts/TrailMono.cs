using System;
using UnityEngine;

namespace Trail
{
    /// <summary>
    /// This class is used to convert Unity callbacks and built in features to Trail sdk so we can 
    ///     handle specific methods correctly while running on Trail.
    /// </summary>
    internal class TrailMono : MonoBehaviour
    {
        /// <summary>
        /// Creates a new instance of TrailMono connected to provided sdk.
        /// This handles exit of game and tick for internal sdk purposes.
        /// </summary>
        /// <param name="sdk">The sdk to connect Trail Mono to</param>
        public static TrailMono Create()
        {
            var go = new GameObject("Trail Mono");
            var trailMono = go.AddComponent<TrailMono>();
            DontDestroyOnLoad(go);
            return trailMono;
        }

        // Takes care of updating internal sdk callbacks when not running on Trail's website
        private void Update()
        {
            if (SDK.Raw != IntPtr.Zero)
            {
                SDK.Tick();
            }
        }

        private void OnDestroy()
        {
            SDK.instance.Dispose();
        }
    }
}
