using System.Runtime.InteropServices;

namespace MetalPerformanceHUD.Plugins.iOS
{
    public static class MetalPerformanceHUD
    {
        /// <summary>
        /// Performance HUD を表示する
        /// </summary>
        public static void ShowPerformanceHUD()
        {
#if !UNITY_EDITOR && UNITY_IOS
            NativeMethod();

            [DllImport("__Internal", EntryPoint = "showPerformanceHUD")]
            static extern void NativeMethod();
#endif
        }

        /// <summary>
        /// Performance HUD を非表示にする
        /// </summary>
        public static void HidePerformanceHUD()
        {
#if !UNITY_EDITOR && UNITY_IOS
            NativeMethod();

            [DllImport("__Internal", EntryPoint = "hidePerformanceHUD")]
            static extern void NativeMethod();
#endif
        }
    }
}
