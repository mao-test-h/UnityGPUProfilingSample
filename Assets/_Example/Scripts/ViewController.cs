using System.Text;
using FTM;
using UnityEngine;
using UnityEngine.UI;

namespace _Examples
{
    internal sealed class ViewController : MonoBehaviour
    {
        // Set Resolution
        [SerializeField] private Text resolutionText;
        [SerializeField] private RectTransform resolutionButtonNode;
        [SerializeField] private Button resetResolutionButton;

        // Metal Performance HUD
        [SerializeField] private Button showPerformanceHUD;
        [SerializeField] private Button hidePerformanceHUD;

        // FrameTimingManager
        [SerializeField] private Text gpuFrameTimeAvg;
        [SerializeField] private Text gpuFrameTimeMin;
        [SerializeField] private Text gpuFrameTimeMax;

        private Resolution _originalResolution;

        // FrameTimingManager
        private readonly FrameTimingSampler _frameTimingSampler = new FrameTimingSampler();
        private float _lastTime;
        private const float RefreshRate = 1f / 5f;

        private void Start()
        {
            // 情報表示
            {
                var builder = new StringBuilder();
                foreach (var resolution in Screen.resolutions)
                {
                    builder.AppendLine(resolution.ToString());
                }

                Debug.Log($"Current Resolution: {Screen.currentResolution.ToString()}");
                Debug.Log($"Supported Resolutions: {builder}");
                Debug.Log($"FrameTimingManager.IsFeatureEnabled = {FrameTimingManager.IsFeatureEnabled()}");
                Debug.Log($"SystemInfo.supportsGpuRecorder = {SystemInfo.supportsGpuRecorder}");
            }

            _originalResolution = Screen.currentResolution;
            _lastTime = Time.time;

            SetUpResolutionEvent();
            SetUpMetalPerformanceHUDEvent();
            SetUpGPUFrameTimeUI();
        }

        private void Update()
        {
            var refresh = false;
            if (Time.time > _lastTime + RefreshRate)
            {
                refresh = true;
                _lastTime = Time.time;
            }

            UpdateGPUFrameTimeUI(refresh);
        }

        /// <summary>
        /// 解像度変更ボタンの設定
        /// </summary>
        /// <remarks>
        /// NOTE: Landscape についてはそこまで考慮してない
        /// </remarks>
        private void SetUpResolutionEvent()
        {
            resolutionText.text = $"Resolution ({_originalResolution.width} x {_originalResolution.height})";

            // width から height を計算し、解像度を返す
            (int width, int height) CalcResolution(int width)
            {
                var aspectRatio = (float)_originalResolution.height / _originalResolution.width;
                var newHeight = (int)(width * aspectRatio);
                return (width, newHeight);
            }

            // ボタンイベントの登録
            void AddListener(int width, int height, Button button)
            {
                button.onClick.AddListener(() =>
                {
                    resolutionText.text = $"Resolution ({width} x {height})";
                    Screen.SetResolution(width, height, true);
                });
            }

            // 解像度のリスト (width)
            var resolutions = new[]
            {
                270,
                540,
                720,
                1080,
            };

            foreach (var width in resolutions)
            {
                // `resolutionButtonNode` をベースとして使い回して解像度ごとのボタンを生成していく
                var resolution = CalcResolution(width);
                var button = Instantiate(resetResolutionButton, resolutionButtonNode);
                button.GetComponentInChildren<Text>().text = $"{resolution.width}p";
                AddListener(resolution.width, resolution.height, button);
            }

            AddListener(_originalResolution.width, _originalResolution.height, resetResolutionButton);
        }

        private void SetUpMetalPerformanceHUDEvent()
        {
            showPerformanceHUD.onClick.AddListener(MetalPerformanceHUD.Plugins.iOS.MetalPerformanceHUD.ShowPerformanceHUD);
            hidePerformanceHUD.onClick.AddListener(MetalPerformanceHUD.Plugins.iOS.MetalPerformanceHUD.HidePerformanceHUD);
        }

        private void SetUpGPUFrameTimeUI()
        {
            if (FrameTimingManager.IsFeatureEnabled())
            {
                gpuFrameTimeAvg.text = "0.00 ms";
                gpuFrameTimeMin.text = "0.00 ms";
                gpuFrameTimeMax.text = "0.00 ms";
            }
            else
            {
                gpuFrameTimeAvg.text = "-";
                gpuFrameTimeMin.text = "-";
                gpuFrameTimeMax.text = "-";
            }
        }

        private void UpdateGPUFrameTimeUI(bool refresh)
        {
            if (FrameTimingManager.IsFeatureEnabled() == false)
            {
                return;
            }

            _frameTimingSampler.UpdateFrameTiming();

            if (refresh)
            {
                var avgStr = $"{_frameTimingSampler.SampleAverage.GPUFrameTime:0.00} ms";
                var minStr = $"{_frameTimingSampler.SampleMin.GPUFrameTime:0.00} ms";
                var maxStr = $"{_frameTimingSampler.SampleMax.GPUFrameTime:0.00} ms";

                gpuFrameTimeAvg.text = avgStr;
                gpuFrameTimeMin.text = minStr;
                gpuFrameTimeMax.text = maxStr;

                Debug.Log($"Avg: {avgStr}, Min: {minStr}, Max: {maxStr}");
            }
        }
    }
}
