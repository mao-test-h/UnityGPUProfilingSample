using System.Linq;
using UnityEngine;

namespace FTM
{
    /// <summary>
    /// `FrameTimingManager` のサンプラー
    /// </summary>
    /// <remarks>
    /// USAGE:
    /// - インスタンス化したクラス側で `UpdateFrameTiming()` を呼び出して更新して下さい
    /// NOTE:
    /// - Rendering Debugger -> Display Stats を参考に実装
    /// </remarks>
    public sealed class FrameTimingSampler
    {
        private readonly FrameTimeSampleHistory _frameHistory;
        private readonly BottleneckHistory _bottleneckHistory;
        private readonly FrameTiming[] _timing = new FrameTiming[1];
        private FrameTimeSample _sample;

        /// <summary>
        /// ボトルネック履歴のサイズ (サンプル数)
        /// </summary>
        public int BottleneckHistorySize { get; set; } = 60;

        /// <summary>
        /// サンプル履歴のサイズ (サンプル数)
        /// </summary>
        public int SampleHistorySize { get; set; } = 30;

        /// <summary>
        /// サンプル履歴の平均値
        /// </summary>
        public FrameTimeSample SampleAverage => _frameHistory.SampleAverage;

        /// <summary>
        /// サンプル履歴の最小値
        /// </summary>
        public FrameTimeSample SampleMin => _frameHistory.SampleMin;

        /// <summary>
        /// サンプル履歴の最大値
        /// </summary>
        public FrameTimeSample SampleMax => _frameHistory.SampleMax;

        /// <summary>
        /// ボトルネック履歴の分布
        /// </summary>
        public BottleneckHistogram Histogram => _bottleneckHistory.Histogram;

        public FrameTimingSampler()
        {
            _frameHistory = new FrameTimeSampleHistory(SampleHistorySize);
            _bottleneckHistory = new BottleneckHistory(BottleneckHistorySize);
        }

        /// <summary>
        /// データ更新
        /// </summary>
        public void UpdateFrameTiming()
        {
            _timing[0] = default;
            _sample = default;

            // FrameTiming データの取得
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(1, _timing);

            // データのサンプリング
            if (_timing.Length > 0)
            {
                _sample.FullFrameTime = (float)_timing.First().cpuFrameTime;
                _sample.FramesPerSecond = _sample.FullFrameTime > 0f ? 1000f / _sample.FullFrameTime : 0f;
                _sample.MainThreadCPUFrameTime = (float)_timing.First().cpuMainThreadFrameTime;
                _sample.MainThreadCPUPresentWaitTime = (float)_timing.First().cpuMainThreadPresentWaitTime;
                _sample.RenderThreadCPUFrameTime = (float)_timing.First().cpuRenderThreadFrameTime;
                _sample.GPUFrameTime = (float)_timing.First().gpuFrameTime;
            }

            // サンプリングデータを保存
            _frameHistory.DiscardOldSamples(SampleHistorySize);
            _frameHistory.Add(_sample);
            _frameHistory.ComputeAggregateValues();

            // ボトルネックデータを保存
            _bottleneckHistory.DiscardOldSamples(BottleneckHistorySize);
            _bottleneckHistory.AddBottleneckFromAveragedSample(_frameHistory.SampleAverage);
            _bottleneckHistory.ComputeHistogram();
        }

        public void Reset()
        {
            _frameHistory.Clear();
            _bottleneckHistory.Clear();
        }
    }
}
