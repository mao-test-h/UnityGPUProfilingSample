using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace MetalPerformanceHUD.Editor
{
    internal sealed class XcodePostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;

            var outputPath = report.summary.outputPath;
            var schemePath = Path.Combine(
                outputPath,
                "Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme");

            SetFrameCaptureMode(schemePath);
            EnablePerformanceHUD(outputPath);
            ShowPerformanceHUD(schemePath);
        }

        /// <summary>
        /// GPU FrameCapture の有効化
        /// </summary>
        private void SetFrameCaptureMode(string schemePath)
        {
            var xcScheme = new XcScheme();
            xcScheme.ReadFromFile(schemePath);
            xcScheme.SetFrameCaptureModeOnRun(XcScheme.FrameCaptureMode.Metal);
            xcScheme.WriteToFile(schemePath);
        }

        /// <summary>
        /// Metal Performance HUD をコード上から切り替えられるようにする
        /// </summary>
        private void EnablePerformanceHUD(string outputPath)
        {
            // plist.info に `MetalHudEnabled : true` を追加することでコード上から ON/OFF することが可能になる
            var plistPath = Path.Combine(outputPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var root = plist.root;
            // NOTE: `false` にするとコード上から ON/OFF 出来なくなる
            root.SetBoolean("MetalHudEnabled", true);
            plist.WriteToFile(plistPath);
        }

        /// <summary>
        /// Metal Performance HUD を起動時に表示する
        /// </summary>
        private void ShowPerformanceHUD(string schemePath)
        {
            var xcScheme = new XcScheme();
            xcScheme.ReadFromFile(schemePath);

            // Performance HUD を有効化するための API が存在しないので、`XcScheme` が持つ `m_Doc` を取得して直接編集する
            var fieldInfo = typeof(XcScheme).GetField("m_Doc",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo == null || fieldInfo.GetValue(xcScheme) is not XDocument xDocument || xDocument.Root == null)
            {
                Debug.LogError("Failed to get XDocument from XcScheme");
                return;
            }

            // `LaunchAction` に `showGraphicsOverview` と `logGraphicsOverview` を追加
            var xElement = xDocument.Root.XPathSelectElement("./LaunchAction");
            Assert.IsNotNull(xElement, "The XcScheme document does not contain build configuration setting");
            xElement.SetAttributeValue((XName)"showGraphicsOverview", "Yes");
            xElement.SetAttributeValue((XName)"logGraphicsOverview", "Yes");

            xcScheme.WriteToFile(schemePath);
        }
    }
}
