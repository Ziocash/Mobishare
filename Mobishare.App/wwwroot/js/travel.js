// Cronometro
let startTime;
let timerInterval;
let costUpdateInterval;

// Inizializza quando la pagina è caricata
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOMContentLoaded - rideData:', window.rideData);

    if (window.rideData) {
        startTime = new Date(window.rideData.startDateTime);
        console.log('Start time:', startTime);
        console.log('Cost per minute (string):', window.rideData.costPerMinute);
        console.log('Cost per minute (parsed):', parseFloat(window.rideData.costPerMinute));

        startTimer();
        startCostUpdater();
    } else {
        console.error('window.rideData is not available');
    }
});

function startTimer() {
    updateTimer();
    timerInterval = setInterval(updateTimer, 1000);
}

function updateTimer() {
    const now = new Date();
    const elapsed = now - startTime;

    const minutes = Math.floor(elapsed / 60000);
    const seconds = Math.floor((elapsed % 60000) / 1000);

    const timerElement = document.getElementById('timer');
    if (timerElement) {
        timerElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
}

function startCostUpdater() {
    console.log('Starting cost updater');
    updateCostLocally(); // Prima chiamata immediata
    costUpdateInterval = setInterval(updateCostLocally, 1000); // Aggiorna ogni secondo per testing
}

function updateCostLocally() {
    if (!startTime || !window.rideData.costPerMinute) {
        console.error('Missing data for local cost calculation');
        return;
    }

    const now = new Date();
    const elapsedMinutes = (now - startTime) / (1000 * 60);
    console.log('Calculating cost - elapsed minutes:', elapsedMinutes);
    
    let costPerMinute = parseFloat(window.rideData.costPerMinute);
    let currentCost;
    const fixedCostVehicle = {
        Bike: 5,
        Scooter: 7,
        EBike: 6
    };
    let type = window.rideData?.vehicleType; // ad esempio "Bike"
    let FixedCost = fixedCostVehicle[type];
    debugger;
    console.log('Fixed cost for vehicle type', type, ':', FixedCost);
    if (elapsedMinutes >= 30)
        currentCost = FixedCost + (elapsedMinutes * costPerMinute);
    else
        currentCost = FixedCost;

    console.log('Elapsed minutes:', elapsedMinutes.toFixed(4));
    console.log('Cost per minute:', costPerMinute);
    console.log('Calculated cost:', currentCost.toFixed(4));

    const costElement = document.getElementById('costValue');
    if (costElement) {
        costElement.textContent = `€${Math.max(0, currentCost).toFixed(2)}`;
        console.log('Updated cost display to:', `€${Math.max(0, currentCost).toFixed(2)}`);
    } else {
        console.error('costValue element not found');
    }



}

// Funzione per terminare il viaggio
function endTrip() {
    console.log('Ending trip');

    // Ferma i timer
    if (timerInterval) {
        clearInterval(timerInterval);
    }
    if (costUpdateInterval) {
        clearInterval(costUpdateInterval);
    }

    // Mostra il modal
    const modalElement = document.getElementById('endTripModal');
    if (modalElement && typeof bootstrap !== 'undefined') {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    } else {
        console.error('Modal element or Bootstrap not found');
    }
}

// Gestione conferma fine viaggio
document.addEventListener('DOMContentLoaded', function () {
    const confirmButton = document.getElementById('confirmEndTrip');
    if (confirmButton) {
        confirmButton.addEventListener('click', function () {
            const tripName = document.getElementById('tripName').value;
            document.getElementById('tripNameInput').value = tripName;
            document.getElementById('endTripForm').submit();
        });
    }
});

// Inizializza la mappa
function initMaps() {
    console.log('Maps initialized');
}