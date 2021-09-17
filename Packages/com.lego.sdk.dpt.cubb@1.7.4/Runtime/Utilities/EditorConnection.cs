#if UNITY_EDITOR_OSX
using UnityEditor;

namespace CoreUnityBleBridge.Editor
{
    [InitializeOnLoad]
    public static class EditorConnection 
    {
        private const string CUBB_REAL_EDITOR_CONNECTION_KEY = "CUBB_REAL_EDITOR_CONNECTION_KEY";
        private const string BLE_CONNECTION_FAKE = "Ble Connection/Fake";
        private const string BLE_CONNECTION_REAL = "Ble Connection/Real";

        static EditorConnection()
        {
            EditorApplication.delayCall += () =>
            {
                UpdateCheckmark();
            };
        }

        [MenuItem(BLE_CONNECTION_FAKE)]
        private static void ChangeToFakeConnection()
        {
            if (!GetShouldUseRealConnection())
                return;
            
            EditorPrefs.SetBool(CUBB_REAL_EDITOR_CONNECTION_KEY, false);
            UpdateCheckmark();
        }
        
        [MenuItem(BLE_CONNECTION_REAL)]
        private static void ChangeToRealConnection()
        {
            if (GetShouldUseRealConnection())
                return;
            
            EditorPrefs.SetBool(CUBB_REAL_EDITOR_CONNECTION_KEY, true);
            UpdateCheckmark();
        }

        [MenuItem(BLE_CONNECTION_FAKE, true)]
        private static bool FakeConnectionCheck()
        {
            return !EditorApplication.isPlaying;
        }
        
        [MenuItem(BLE_CONNECTION_REAL, true)]
        private static bool RealConnectionCheck()
        {
            return !EditorApplication.isPlaying;
        }
        
        private static void UpdateCheckmark()
        {
            bool isRealConnection = GetShouldUseRealConnection();
            
            Menu.SetChecked(BLE_CONNECTION_FAKE, !isRealConnection);
            Menu.SetChecked(BLE_CONNECTION_REAL, isRealConnection);
        }

        public static bool GetShouldUseRealConnection()
        {
            if (!EditorPrefs.HasKey(CUBB_REAL_EDITOR_CONNECTION_KEY))
                EditorPrefs.SetBool(CUBB_REAL_EDITOR_CONNECTION_KEY, false);
            
            return EditorPrefs.GetBool(CUBB_REAL_EDITOR_CONNECTION_KEY);
        }
    }
}
#endif