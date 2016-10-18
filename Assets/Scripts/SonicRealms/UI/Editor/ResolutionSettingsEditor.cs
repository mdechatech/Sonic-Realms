using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.UI.Editor
{
    [CustomEditor(typeof(ResolutionSettings))]
    public class ResolutionSettingsEditor : BaseRealmsEditor
    {
        private ResolutionSettings _instance;

        protected override void OnEnable()
        {
            base.OnEnable();

            _instance = (ResolutionSettings) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            /*
            for (var i = 0; i < _instance.WindowedEntryCount; ++i)
            {
                var entry = _instance.GetWindowedEntry(i);
                for (var j = 0; j < entry.ScreenSizeCount; ++j)
                {
                    // TODO Screen size rounding is very glitchy rn
                    entry[j].Round(new ResolutionSettings.AspectRatio(entry.AspectHorizontal, entry.AspectVertical));
                }
            }
            */
        }
    }
}
