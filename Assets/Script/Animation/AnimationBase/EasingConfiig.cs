using UnityEngine;

namespace MyTween
{

    /// <summary>
    /// イージング種別のプリセット定義。
    /// 数式ベースの一般的な easing を列挙する。
    /// </summary>
    public enum EaseType
    {
        Linear,                         // 等速
        SineIn, SineOut, SineInOut,     // 正弦波ベース
        QuadIn, QuadOut, QuadInOut,     // 2次関数
        CubicIn, CubicOut, CubicInOut,  // 3次関数
        QuartIn, QuartOut, QuartInOut,  // 4次関数
        QuintIn, QuintOut, QuintInOut,  // 5次関数
        ExpoIn, ExpoOut, ExpoInOut,     // 指数関数
        CircIn, CircOut, CircInOut      // 円弧
    }

    /// <summary>
    /// イージングの「設定値」だけを保持するクラス。
    /// 
    /// ・MonoBehaviour 非依存
    /// ・シリアライズ可能
    /// ・データコンテナ（DTO）として使用する
    /// 
    /// コルーチンやTweenロジック側に直接渡されることを前提とする。
    /// </summary>
    [System.Serializable]
    public class EasingConfigOption
    {
        /// <summary>
        /// プリセットイージング種別。
        /// customCurve が有効な場合は無視される。
        /// </summary>
        [SerializeField]
        private EaseType easeType = EaseType.SineInOut;

        /// <summary>
        /// 任意の AnimationCurve。
        /// キーが1つ以上存在する場合、こちらを最優先で使用する。
        /// 入出力は 0～1 を想定。
        /// </summary>
        [SerializeField]
        private AnimationCurve customCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// true の場合、Time.timeScale の影響を受けない時間進行を行う。
        /// （ポーズ中のアニメーション等）
        /// </summary>
        [SerializeField]
        private bool useUnscaledTime = false;

        // 外部公開用の読み取り専用プロパティ
        public EaseType EaseType => easeType;
        public AnimationCurve CustomCurve => customCurve;
        public bool UseUnscaledTime => useUnscaledTime;
    }

    /// <summary>
    /// 線形補間値 (0～1) をイージング済みの値 (0～1) に変換する評価クラス。
    /// 
    /// ・状態を持たない
    /// ・EasingConfigOption を引数で受け取る
    /// ・GameObject / Component に依存しない
    /// 
    /// コルーチンやTweenロジックから直接利用することを想定。
    /// </summary>
    public class EasingEvaluator
    {
        /// <summary>
        /// 線形時間 linT (0～1) をイージング済みの値に変換する。
        /// </summary>
        /// <param name="linT">
        /// 正規化された線形進行度 (0～1)。
        /// </param>
        /// <param name="opt">
        /// イージング設定。
        /// null の場合は Linear として評価される。
        /// </param>
        /// <returns>
        /// イージング適用後の値 (0～1)。
        /// </returns>
        public float Evaluate(float linT, EasingConfigOption opt)
        {
            // 安全のため入力値をクランプ
            linT = Mathf.Clamp01(linT);

            // カスタムカーブが有効な場合は最優先で使用
            if (opt != null &&
                opt.CustomCurve != null &&
                opt.CustomCurve.keys.Length > 0)
            {
                return Mathf.Clamp01(opt.CustomCurve.Evaluate(linT));
            }

            // 設定が無い場合は Linear として扱う
            var easeType = opt != null ? opt.EaseType : EaseType.Linear;

            // プリセットイージング評価
            switch (easeType)
            {
                case EaseType.Linear:
                    return linT;

                case EaseType.SineIn:
                    return 1f - Mathf.Cos((linT * Mathf.PI) / 2f);
                case EaseType.SineOut:
                    return Mathf.Sin((linT * Mathf.PI) / 2f);
                case EaseType.SineInOut:
                    return -(Mathf.Cos(Mathf.PI * linT) - 1f) / 2f;

                case EaseType.QuadIn:
                    return linT * linT;
                case EaseType.QuadOut:
                    return 1f - (1f - linT) * (1f - linT);
                case EaseType.QuadInOut:
                    return linT < 0.5f
                        ? 2f * linT * linT
                        : 1f - Mathf.Pow(-2f * linT + 2f, 2f) / 2f;

                case EaseType.CubicIn:
                    return linT * linT * linT;
                case EaseType.CubicOut:
                    {
                        float t = 1f - linT;
                        return 1f - t * t * t;
                    }
                case EaseType.CubicInOut:
                    return linT < 0.5f
                        ? 4f * linT * linT * linT
                        : 1f - Mathf.Pow(-2f * linT + 2f, 3f) / 2f;

                case EaseType.QuartIn:
                    return linT * linT * linT * linT;
                case EaseType.QuartOut:
                    {
                        float t = 1f - linT;
                        return 1f - t * t * t * t;
                    }
                case EaseType.QuartInOut:
                    return linT < 0.5f
                        ? 8f * Mathf.Pow(linT, 4f)
                        : 1f - Mathf.Pow(-2f * linT + 2f, 4f) / 2f;

                case EaseType.QuintIn:
                    return Mathf.Pow(linT, 5f);
                case EaseType.QuintOut:
                    {
                        float t = 1f - linT;
                        return 1f - Mathf.Pow(t, 5f);
                    }
                case EaseType.QuintInOut:
                    return linT < 0.5f
                        ? 16f * Mathf.Pow(linT, 5f)
                        : 1f - Mathf.Pow(-2f * linT + 2f, 5f) / 2f;

                case EaseType.ExpoIn:
                    return (linT == 0f) ? 0f : Mathf.Pow(2f, 10f * linT - 10f);
                case EaseType.ExpoOut:
                    return (linT == 1f) ? 1f : 1f - Mathf.Pow(2f, -10f * linT);
                case EaseType.ExpoInOut:
                    if (linT == 0f) return 0f;
                    if (linT == 1f) return 1f;
                    return linT < 0.5f
                        ? Mathf.Pow(2f, 20f * linT - 10f) / 2f
                        : (2f - Mathf.Pow(2f, -20f * linT + 10f)) / 2f;

                case EaseType.CircIn:
                    return 1f - Mathf.Sqrt(1f - linT * linT);
                case EaseType.CircOut:
                    return Mathf.Sqrt(1f - Mathf.Pow(linT - 1f, 2f));
                case EaseType.CircInOut:
                    return linT < 0.5f
                        ? (1f - Mathf.Sqrt(1f - 4f * linT * linT)) / 2f
                        : (Mathf.Sqrt(1f - Mathf.Pow(-2f * linT + 2f, 2f)) + 1f) / 2f;

                default:
                    return linT;
            }
        }
    }
}
