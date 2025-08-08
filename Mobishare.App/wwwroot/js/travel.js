 // Cronometro
let startTime = new Date(Date.now()); // 30 minuti fa
let isActive = true;

function updateTimer() {
    if (!isActive) return;

    const now = new Date();
    const elapsed = Math.floor((now - startTime) / 1000);
    
    const hours = Math.floor(elapsed / 3600);
    const minutes = Math.floor((elapsed % 3600) / 60);
    const seconds = elapsed % 60;

    let timeString;
    if (hours > 0) {
        timeString = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    } else {
        timeString = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    document.getElementById('timer').textContent = timeString;
}

// Avvia il cronometro
updateTimer();
setInterval(updateTimer, 1000);

function loadRideInfo(){
    
}

function endTrip() {
    // Ferma il timer
    isActive = false;
    document.querySelector('.pulse-dot').style.animation = 'none';
    document.querySelector('.pulse-dot').style.background = '#9ca3af';
    
    // Mostra il modal
    const modal = new bootstrap.Modal(document.getElementById('endTripModal'));
    modal.show();
}

// Aggiungi l'event listener per il pulsante di conferma
document.addEventListener('DOMContentLoaded', function() {
    const confirmButton = document.getElementById('confirmEndTrip');
    const tripNameField = document.getElementById('tripName');
    const tripNameInput = document.getElementById('tripNameInput');
    const endTripForm = document.getElementById('endTripForm');
    
    if (confirmButton) {
        confirmButton.addEventListener('click', function() {
            // Copia il nome del viaggio nel form nascosto
            tripNameInput.value = tripNameField.value;
            
            // Invia il form
            endTripForm.submit();
        });
    }
});