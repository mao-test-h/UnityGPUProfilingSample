namespace FTM
{
    /// <summary>
    /// システムのボトルネック
    /// </summary>
    internal enum PerformanceBottleneck
    {
        /// <summary>
        /// 決定的でない
        /// </summary>
        Indeterminate,

        /// <summary>
        /// プレゼンテーションによる制限 (`vSync`またはフレームレート上限) 
        /// </summary>
        /// <remarks>
        /// ターゲットとなるフレームレートに対して CPU も GPU も十分に余裕がある状態
        /// </remarks>
        PresentLimited,

        /// <summary>
        /// CPU による制限 (メインスレッド、レンダースレッド)
        /// </summary>
        /// <remarks>
        /// 現在のフレーム当たりの処理時間のうち、そのほとんどを CPU が占めている状態
        /// </remarks>
        CPU,

        /// <summary>
        /// GPU による制限
        /// </summary>
        /// <remarks>
        /// 現在のフレーム当たりの処理時間のうち、ほとんどを GPU が占めている状態
        /// </remarks>
        GPU,

        /// <summary>
        /// CPU と GPU の両方で制限されている (つまりバランスが取れている)
        /// </summary>
        /// <remarks>
        /// 現在のフレーム当たりの処理時間のうち、CPU も GPU も十分な時間を使っている状態  
        /// バランスが良い状態ではあるが、フレーム時間のほとんどを使用しているため、余裕はあまり無い状態とも言える  
        /// </remarks>
        Balanced,
    }
}
