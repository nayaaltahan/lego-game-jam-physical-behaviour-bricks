using CoreUnityBleBridge.Model;
using CoreUnityBleBridge.ToNative.Bridge;
using CoreUnityBleBridge.ToUnity;
using LEGO.Logger;

namespace CoreUnityBleBridge.ToNative
{
    internal static class UnityToNativeFactory
    {
        private static ILog logger;

        public static IUnityToNative Create(NativeToUnity nativeToUnity)
        {
            logger = LogManager.GetLogger(typeof(UnityToNativeFactory));

            IUnityToNative unityToNativeBridge = null;
#if UNITY_EDITOR_OSX
            bool shouldUseRealConnection = Editor.EditorConnection.GetShouldUseRealConnection();
            if (shouldUseRealConnection)
                unityToNativeBridge = new UnityToNative(new OSXUnityToNativeBridge());            
            else
                unityToNativeBridge = new EditorUnityToNative(nativeToUnity);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            unityToNativeBridge = new UnityToNative(new OSXUnityToNativeBridge());
#elif UNITY_IOS && !UNITY_EDITOR
            unityToNativeBridge = new UnityToNative(new IOSUnityToNativeBridge());
#elif UNITY_ANDROID && !UNITY_EDITOR
            unityToNativeBridge = new UnityToNative(new AndroidUnityToNativeBridge());
#elif UNITY_WSA_10_0 && !UNITY_EDITOR
            unityToNativeBridge = new UWPUnityToNative(nativeToUnity);
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            unityToNativeBridge = new UnityToNative(new MikeUnityToNativeBridge());
#else
            unityToNativeBridge = new EditorUnityToNative(nativeToUnity);

#endif
            LogManager.OnLogLevelChanged += level =>
            {
                unityToNativeBridge.SetLogLevel((int) level);
            };
            
            return unityToNativeBridge;
        }
    }
}