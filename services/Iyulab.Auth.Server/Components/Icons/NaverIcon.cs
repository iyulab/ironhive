using Microsoft.FluentUI.AspNetCore.Components;

namespace Iyulab.Auth.Server.Components.Icons;

public class NaverIcon : Icon
{
    private const string CONTENT = "\uE8A2";

    public NaverIcon() : base(
        "naver",
        IconVariant.Regular,
        IconSize.Size24,
        CONTENT)
    {
    }
}
