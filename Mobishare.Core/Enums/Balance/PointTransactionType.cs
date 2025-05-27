namespace Mobishare.Core.Enums.Points;

public enum PointTransactionType
{
    Earned,     // Punti guadagnati (es. completamento corsa, invito amici)
    Redeemed,   // Punti spesi (es. sconto su corsa)
    Expired,    // Punti scaduti per inattività
    Adjusted    // Correzioni manuali o bonus
}