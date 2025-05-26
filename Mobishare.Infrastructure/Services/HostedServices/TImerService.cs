using Microsoft.AspNetCore.SignalR; // Per usare SignalR e comunicare in tempo reale con i client
using System.Timers; // Per usare il timer di sistema

// Definisce un servizio che gestisce un timer condiviso tramite SignalR
public class TimerService
{
    private readonly IHubContext<TimerHub> _hubContext; // Permette di inviare messaggi ai client SignalR
    private System.Timers.Timer? _timer; // Il timer che scandisce il tempo
    private string? _ownerConnectionId; // L'ID della connessione che possiede il timer
    private DateTime _startTime; // L'istante in cui il timer è stato avviato
    private readonly TimeSpan _duration = TimeSpan.FromSeconds(60); // Durata totale del timer (60 secondi)

    // Costruttore: riceve il contesto dell'hub SignalR
    public TimerService(IHubContext<TimerHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Avvia il timer per una specifica connessione
    public async Task Start(string connectionId)
    {
        // Se il timer è già in uso da un altro utente, ignora la richiesta
        if (_ownerConnectionId != null && _ownerConnectionId != connectionId)
            return;

        _ownerConnectionId = connectionId; // Salva l'ID della connessione proprietaria
        _startTime = DateTime.UtcNow; // Salva il tempo di avvio

        _timer?.Stop(); // Ferma un eventuale timer già attivo
        _timer = new System.Timers.Timer(1000); // Crea un nuovo timer che scatta ogni secondo (1000 ms)
        _timer.Elapsed += async (s, e) => // Evento chiamato ogni secondo
        {
            var remaining = _duration - (DateTime.UtcNow - _startTime); // Calcola il tempo rimanente
            if (remaining <= TimeSpan.Zero) // Se il tempo è scaduto
            {
                _timer.Stop(); // Ferma il timer
                 Disconnect(connectionId); // Disconnette il timer se il tempo non è scaduto
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveTime", 0); // Invia 0 al client
            }
            else
            {
               
                // Invia il tempo rimanente (in secondi, arrotondato per eccesso) al client
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveTime", Math.Ceiling(remaining.TotalSeconds));
            }
        };
        _timer.Start(); // Avvia il timer
    }

    // Disconnette il timer se la connessione proprietaria si disconnette
    public void Disconnect(string connectionId)
    {
        if (_ownerConnectionId == connectionId) // Solo il proprietario può fermare il timer
        {
            _timer?.Stop(); // Ferma il timer
            _timer = null; // Libera la risorsa
            _ownerConnectionId = null; // Libera la proprietà
        }
    }
}