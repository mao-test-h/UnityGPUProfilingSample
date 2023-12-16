namespace FTM
{
    /// <summary>
    /// 単一フレームからキャプチャされたタイミングデータ
    /// </summary>
    public struct FrameTimeSample
    {
        public float FramesPerSecond;
        public float FullFrameTime;
        public float MainThreadCPUFrameTime;
        public float MainThreadCPUPresentWaitTime;
        public float RenderThreadCPUFrameTime;
        public float GPUFrameTime;

        internal FrameTimeSample(float initValue)
        {
            FramesPerSecond = initValue;
            FullFrameTime = initValue;
            MainThreadCPUFrameTime = initValue;
            MainThreadCPUPresentWaitTime = initValue;
            RenderThreadCPUFrameTime = initValue;
            GPUFrameTime = initValue;
        }
    }
}
