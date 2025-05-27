namespace Mobishare.Core.Enums.Balance;

public enum CreditTransactionType
{
    Deposit,       // Ricarica fondi nel wallet
    RidePayment,   // Pagamento corsa o noleggio
    Refund,        // Rimborso per corsa annullata o errore
    BonusCredit    // Crediti gratuiti (es. promozioni, inviti)
}
