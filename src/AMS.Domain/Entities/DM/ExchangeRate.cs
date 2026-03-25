using AMS.Domain.Entities;

namespace AMS.Domain.Entities.DM;

/// <summary>
/// Represents daily exchange rates for foreign currency conversion.
/// </summary>
public class ExchangeRate : BaseEntity
{
    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Date the exchange rate is effective.
    /// </summary>
    public DateTime RateDate { get; set; }

    /// <summary>
    /// Buy rate (exchange rate when buying foreign currency).
    /// </summary>
    public decimal BuyRate { get; set; }

    /// <summary>
    /// Sell rate (exchange rate when selling foreign currency).
    /// </summary>
    public decimal SellRate { get; set; }

    /// <summary>
    /// Mid-point rate (average of buy and sell rates).
    /// </summary>
    public decimal? MidRate { get; set; }
}
