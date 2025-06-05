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

function endTrip() {
    isActive = false;
    document.querySelector('.pulse-dot').style.animation = 'none';
    document.querySelector('.pulse-dot').style.background = '#9ca3af';
    alert('Viaggio terminato!');
}

// Avvia il cronometro
updateTimer();
setInterval(updateTimer, 1000);

function loadRideInfo(){
    
}