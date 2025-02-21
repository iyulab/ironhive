namespace Raggle.Abstractions;

public interface IServiceModelConverter
{
    /// <summary>
    /// 입력 데이터를 파싱하여 서비스 식별자와 모델 식별자로 분리합니다.
    /// </summary>
    (string, string) Parse(string identifier);

    /// <summary>
    /// 서비스 식별자와 모델 식별자를 조합하여 식별자를 생성합니다.
    /// </summary>
    string Format(string service, string model);
}
