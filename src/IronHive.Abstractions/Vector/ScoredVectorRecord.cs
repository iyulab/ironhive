namespace IronHive.Abstractions.Vector;

/// <summary>
/// 유사도 점수(score)를 포함하는 벡터 정보를 나타내는 클래스입니다.
/// </summary>
public class ScoredVectorRecord : VectorRecord
{
    /// <summary>
    /// 벡터 유사도에 대한 점수입니다.
    /// (Current): 코사인 유사도에 따라 0 에서 1 사이의 값으로 1에 가까울수록 유사도가 높음을 나타냅니다.
    /// </summary>
    public required float Score { get; set; }
}
