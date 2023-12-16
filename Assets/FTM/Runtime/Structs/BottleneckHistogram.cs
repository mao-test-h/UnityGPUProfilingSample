namespace FTM
{
    /// <summary>
    /// ボトルネックの分布
    /// </summary>
    /// <remarks>
    /// サイズは <see cref="FrameTimingSampler.BottleneckHistorySize"/> によって決定される。
    /// </remarks>
    public struct BottleneckHistogram
    {
        public float PresentLimited;
        public float CPU;
        public float GPU;
        public float Balanced;
    }
}
