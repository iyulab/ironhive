using Microsoft.FluentUI.AspNetCore.Components;

namespace Iyulab.Auth.Server.Components.Icons;

public class MicrosoftIcon : Icon
{
    private const string CONTENT = """
        <svg xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 23 23">
            <path fill="#f35325" d="M1 1h10v10H1z"/>
            <path fill="#81bc06" d="M12 1h10v10H12z"/>
            <path fill="#05a6f0" d="M1 12h10v10H1z"/>
            <path fill="#ffba08" d="M12 12h10v10H12z"/>
       </svg>
       """;

    public MicrosoftIcon() : base(
        "microsoft",
        IconVariant.Regular,
        IconSize.Size20,
        CONTENT)
    {
    }
}
