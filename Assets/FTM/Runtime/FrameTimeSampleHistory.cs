using System;
using System.Collections.Generic;
using UnityEngine;

namespace FTM
{
    /// <summary>
    /// 1回のパスで「最小、最大、平均」を計算するヘルパー持つサンプル履歴のためのコンテナクラス
    /// </summary>
    internal sealed class FrameTimeSampleHistory
    {
        public FrameTimeSample SampleAverage;
        public FrameTimeSample SampleMin;
        public FrameTimeSample SampleMax;

        private readonly List<FrameTimeSample> _samples = new();

        internal FrameTimeSampleHistory(int initialCapacity)
        {
            _samples.Capacity = initialCapacity;
        }

        internal void Add(FrameTimeSample sample)
        {
            _samples.Add(sample);
        }

        internal void ComputeAggregateValues()
        {
            static void ForEachSampleMember(ref FrameTimeSample aggregate, FrameTimeSample sample, Func<float, float, float> func)
            {
                aggregate.FramesPerSecond = func(aggregate.FramesPerSecond, sample.FramesPerSecond);
                aggregate.FullFrameTime = func(aggregate.FullFrameTime, sample.FullFrameTime);
                aggregate.MainThreadCPUFrameTime = func(aggregate.MainThreadCPUFrameTime, sample.MainThreadCPUFrameTime);
                aggregate.MainThreadCPUPresentWaitTime = func(aggregate.MainThreadCPUPresentWaitTime, sample.MainThreadCPUPresentWaitTime);
                aggregate.RenderThreadCPUFrameTime = func(aggregate.RenderThreadCPUFrameTime, sample.RenderThreadCPUFrameTime);
                aggregate.GPUFrameTime = func(aggregate.GPUFrameTime, sample.GPUFrameTime);
            }

            FrameTimeSample average = new();
            FrameTimeSample min = new(float.MaxValue);
            FrameTimeSample max = new(float.MinValue);
            // Using the struct to record how many valid samples each field has
            FrameTimeSample numValidSamples = new();

            foreach (var s in _samples)
            {
                ForEachSampleMember(ref min, s, SampleValueMin);
                ForEachSampleMember(ref max, s, SampleValueMax);
                ForEachSampleMember(ref average, s, SampleValueAdd);
                ForEachSampleMember(ref numValidSamples, s, SampleValueCountValid);
            }

            ForEachSampleMember(ref min, numValidSamples, SampleValueEnsureValid);
            ForEachSampleMember(ref max, numValidSamples, SampleValueEnsureValid);
            ForEachSampleMember(ref average, numValidSamples, SampleValueDivide);

            SampleAverage = average;
            SampleMin = min;
            SampleMax = max;
        }

        internal void DiscardOldSamples(int sampleHistorySize)
        {
            Debug.Assert(sampleHistorySize > 0, "Invalid sampleHistorySize");

            while (_samples.Count >= sampleHistorySize)
            {
                _samples.RemoveAt(0);
            }

            _samples.Capacity = sampleHistorySize;
        }

        internal void Clear()
        {
            _samples.Clear();
            SampleAverage = new FrameTimeSample();
            SampleMin = new FrameTimeSample();
            SampleMax = new FrameTimeSample();
        }

        // Helper functions

        private static readonly Func<float, float, float> SampleValueAdd = (value, other)
            => value + other;

        private static readonly Func<float, float, float> SampleValueMin = (value, other)
            => other > 0 ? Mathf.Min(value, other) : value;

        private static readonly Func<float, float, float> SampleValueMax = Mathf.Max;

        private static readonly Func<float, float, float> SampleValueCountValid = (value, other)
            => other > 0 ? value + 1 : value;

        private static readonly Func<float, float, float> SampleValueEnsureValid = (value, other)
            => other > 0 ? value : 0;

        private static readonly Func<float, float, float> SampleValueDivide = (value, other)
            => other > 0 ? value / other : 0;
    }
}
