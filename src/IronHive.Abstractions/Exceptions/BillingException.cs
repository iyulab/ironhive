namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// The provider refused the request because the account behind the credential cannot pay for it — an
/// exhausted prepaid balance or spending quota, or billing that is not set up. Retrying with the same
/// credential does not succeed until the account is funded, so unlike <see cref="RateLimitException"/> this
/// is not a signal to back off and try again; it is the point where an application tells its user to top up.
/// Providers normalize their vendor-specific billing refusals to this type: HTTP 402 Payment Required (any
/// provider or gateway), OpenAI <c>insufficient_quota</c> (sent as HTTP 429), Anthropic <c>billing_error</c>
/// and its «credit balance is too low» refusal.
/// </summary>
/// <remarks>
/// The refusal is scoped to the credential's account: another provider, or the same provider under another
/// account, is not affected by it.
/// </remarks>
public class BillingException : HiveException
{
    public BillingException(string message, Exception? inner = null)
        : base(message, inner)
    { }
}
