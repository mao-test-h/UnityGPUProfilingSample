using System.Collections.Generic;
using UnityEngine;

namespace FTM
{
    /// <summary>
    /// ヒストグラムを計算するヘルパーを持つボトルネック履歴のためのコンテナクラス
    /// </summary>
    internal sealed class BottleneckHistory
    {
        internal BottleneckHistogram Histogram;

        private readonly List<PerformanceBottleneck> _bottlenecks = new();

        internal BottleneckHistory(int initialCapacity)
        {
            _bottlenecks.Capacity = initialCapacity;
        }

        internal void AddBottleneckFromAveragedSample(FrameTimeSample frameHistorySampleAverage)
        {
            var bottleneck = DetermineBottleneck(frameHistorySampleAverage);
            _bottlenecks.Add(bottleneck);
        }

        internal void ComputeHistogram()
        {
            var stats = new BottleneckHistogram();
            foreach (var bottleneck in _bottlenecks)
            {
                switch (bottleneck)
                {
                    case PerformanceBottleneck.Balanced:
                        stats.Balanced++;
                        break;
                    case PerformanceBottleneck.CPU:
                        stats.CPU++;
                        break;
                    case PerformanceBottleneck.GPU:
                        stats.GPU++;
                        break;
                    case PerformanceBottleneck.PresentLimited:
                        stats.PresentLimited++;
                        break;
                }
            }

            stats.Balanced /= _bottlenecks.Count;
            stats.CPU /= _bottlenecks.Count;
            stats.GPU /= _bottlenecks.Count;
            stats.PresentLimited /= _bottlenecks.Count;

            Histogram = stats;
        }

        internal void DiscardOldSamples(int historySize)
        {
            Debug.Assert(historySize > 0, "Invalid sampleHistorySize");

            while (_bottlenecks.Count >= historySize)
            {
                _bottlenecks.RemoveAt(0);
            }

            _bottlenecks.Capacity = historySize;
        }

        internal void Clear()
        {
            _bottlenecks.Clear();
            Histogram = new BottleneckHistogram();
        }

        private static PerformanceBottleneck DetermineBottleneck(FrameTimeSample s)
        {
            const float nearFullFrameTimeThresholdPercent = 0.2f;
            const float nonZeroPresentWaitTimeMs = 0.5f;

            // In direct mode, render thread doesn't exist
            if (s.GPUFrameTime == 0 || s.MainThreadCPUFrameTime == 0)
            {
                // Missing data
                return PerformanceBottleneck.Indeterminate;
            }

            var fullFrameTimeWithMargin = (1f - nearFullFrameTimeThresholdPercent) * s.FullFrameTime;

            // GPU time is close to frame time, CPU times are not
            if (s.GPUFrameTime > fullFrameTimeWithMargin &&
                s.MainThreadCPUFrameTime < fullFrameTimeWithMargin &&
                s.RenderThreadCPUFrameTime < fullFrameTimeWithMargin)
            {
                return PerformanceBottleneck.GPU;
            }

            // One of the CPU times is close to frame time, GPU is not
            if (s.GPUFrameTime < fullFrameTimeWithMargin &&
                (s.MainThreadCPUFrameTime > fullFrameTimeWithMargin ||
                 s.RenderThreadCPUFrameTime > fullFrameTimeWithMargin))
            {
                return PerformanceBottleneck.CPU;
            }

            // Main thread waited due to Vsync or target frame rate
            if (s.MainThreadCPUPresentWaitTime > nonZeroPresentWaitTimeMs)
            {
                // None of the times are close to frame time
                if (s.GPUFrameTime < fullFrameTimeWithMargin &&
                    s.MainThreadCPUFrameTime < fullFrameTimeWithMargin &&
                    s.RenderThreadCPUFrameTime < fullFrameTimeWithMargin)
                {
                    return PerformanceBottleneck.PresentLimited;
                }
            }

            return PerformanceBottleneck.Balanced;
        }
    }
}
