namespace SimpleAuctions.Rules;

public static class BidRules
{
    public static string? Validate(decimal amount)
    {
        if (decimal.Round(amount, 2) != amount)
            return "Amount must have at most two decimal places.";
        return null;
    }
}
