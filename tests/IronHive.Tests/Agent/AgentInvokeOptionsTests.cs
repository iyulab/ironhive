using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;

namespace IronHive.Tests.Agent;

/// <summary>
/// MessageRequest의 per-request 옵션이 AgentInvokeOptions를 통해 전부 도달 가능함을 강제하는 회귀 테스트.
/// 신규 MessageRequest 프로퍼티는 agent-fixed allowlist 또는 AgentInvokeOptions 중 하나에 반드시 분류되어야 한다.
/// </summary>
public class AgentInvokeOptionsTests
{
    // 에이전트 고정 속성 — IAgent 프로퍼티/invoke 인자로 도달 (의식적으로 하드코딩)
    private static readonly HashSet<string> AgentFixedProperties =
        ["Provider", "Model", "System", "Messages", "Tools"];

    [Fact]
    public void AgentInvokeOptions_Should_Cover_All_PerRequest_MessageRequest_Properties()
    {
        var optionProperties = typeof(AgentInvokeOptions)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        var requestProperties = typeof(MessageRequest)
            .GetProperties()
            .Where(p => p.CanWrite)
            .Select(p => p.Name);

        foreach (var name in requestProperties)
        {
            (AgentFixedProperties.Contains(name) || optionProperties.Contains(name))
                .Should().BeTrue(
                    $"MessageRequest.{name} must be classified as agent-fixed or exposed on AgentInvokeOptions " +
                    "(see plan: 2026-07-22-ironhive-agent-invoke-options)");
        }
    }

    [Fact]
    public void AgentInvokeOptions_Should_Not_Expose_AgentFixed_Properties()
    {
        var optionProperties = typeof(AgentInvokeOptions).GetProperties().Select(p => p.Name);

        optionProperties.Should().NotContain(AgentFixedProperties,
            "agent-fixed properties must be configured on the agent, not per-request");
    }
}
