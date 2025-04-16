// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
let map;
let drawingManager;
let currentPolygon = null; // Riferimento al poligono attualmente disegnato/modificato
let drawnPolygonWkt = null; // Memorizza l'output WKT corrente

async function initMap() {
    // Importa le librerie necessarie: 'maps' e 'drawing'
    const { Map } = await google.maps.importLibrary("maps");
    const { DrawingManager } = await google.maps.importLibrary("drawing");

    // Inizializza la mappa (come nel tuo codice originale)
    map = new Map(document.getElementById("map"), {
        center: { lat: 45.50884930272857, lng: 8.950421885612588 },
        zoom: 15,
        styles: [{ featureType: "poi.business", elementType: "all", stylers: [{ visibility: "off" }] }]
    });

    // Inizializza il Drawing Manager
    drawingManager = new DrawingManager({
        drawingControl: true,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: [google.maps.drawing.OverlayType.POLYGON]
        },
        polygonOptions: {
            editable: true,
            draggable: true,
            fillColor: '#007bff',
            strokeColor: '#0056b3',
            strokeWeight: 2
        }
    });
    drawingManager.setMap(map);

    // Listener per quando un poligono è completato
    google.maps.event.addListener(drawingManager, 'polygoncomplete', (polygon) => {
        if (currentPolygon) {
            currentPolygon.setMap(null); // Rimuovi vecchio poligono
        }
        currentPolygon = polygon; // Salva riferimento nuovo

        updateWktAndUI(); // Aggiorna WKT e stato UI (inclusi i pulsanti)

        // Disabilita disegno e controlli
        drawingManager.setDrawingMode(null);
        drawingManager.setOptions({ drawingControl: false });

        // Listeners per modifiche al poligono
        const path = polygon.getPath();
        google.maps.event.addListener(path, 'set_at', updateWktAndUI);
        google.maps.event.addListener(path, 'insert_at', updateWktAndUI);
        google.maps.event.addListener(path, 'remove_at', updateWktAndUI);

        polygon.setEditable(true);
        polygon.setDraggable(true);
    });

    // Funzione helper per calcolare WKT e aggiornare l'UI (textarea e pulsanti)
    function updateWktAndUI() {
        const wktOutputArea = document.getElementById('wktOutput');
        const sendButton = document.getElementById('sendToServerButton');
        const deleteButton = document.getElementById('deletePolygonButton'); // Riferimento al pulsante elimina

        if (!currentPolygon) {
            // Stato: Nessun poligono presente
            drawnPolygonWkt = null;
            if (wktOutputArea) wktOutputArea.value = '';
            if (sendButton) sendButton.style.display = 'none';
            if (deleteButton) deleteButton.style.display = 'none'; // Nascondi pulsante elimina
            return;
        }

        // Stato: Poligono presente
        const path = currentPolygon.getPath();
        let coordinates = [];
        path.getArray().forEach((latLng) => {
            coordinates.push(`${latLng.lng()} ${latLng.lat()}`);
        });
        if (coordinates.length > 0) {
            coordinates.push(coordinates[0]); // Chiudi anello
        }
        drawnPolygonWkt = `POLYGON((${coordinates.join(', ')}))`;

        if (wktOutputArea) wktOutputArea.value = drawnPolygonWkt;
        if (sendButton) sendButton.style.display = 'inline-block'; // Mostra pulsante invia
        if (deleteButton) deleteButton.style.display = 'inline-block'; // Mostra pulsante elimina
        console.log("WKT Aggiornato:", drawnPolygonWkt);
    }

    // --- Logica per il Pulsante Elimina Poligono ---
    const deleteButton = document.getElementById('deletePolygonButton');
    if (deleteButton) {
        deleteButton.addEventListener('click', () => {
            if (currentPolygon) {
                currentPolygon.setMap(null); // Rimuovi poligono dalla mappa
                currentPolygon = null; // Cancella riferimento
                console.log("Poligono rimosso dalla mappa.");
            }

            // Resetta lo stato chiamando la funzione di aggiornamento UI
            // (che nasconderà i pulsanti, pulirà la textarea, ecc.)
            updateWktAndUI();

            // Riabilita gli strumenti di disegno per permettere la creazione di un nuovo poligono
            if (drawingManager) {
                drawingManager.setOptions({
                    drawingControl: true // Mostra di nuovo i controlli (es. icona poligono)
                });
                // Opzionale: puoi rimettere direttamente in modalità disegno poligono
                // drawingManager.setDrawingMode(google.maps.drawing.OverlayType.POLYGON);
            }
        });
    } else {
        console.warn("Elemento con ID 'deletePolygonButton' non trovato.");
    }

    // --- Logica per il Pulsante Invia al Server (invariata) ---
    const sendButton = document.getElementById('sendToServerButton');
    if (sendButton) {
        sendButton.addEventListener('click', () => {
            if (!drawnPolygonWkt) {
                alert('Disegna e completa un poligono prima di inviare.');
                return;
            }
            const postUrl = '/Home/ProcessPolygonWkt'; // !! MODIFICA QUESTO URL !!
            console.log(`Invio WKT a ${postUrl}:`, drawnPolygonWkt);
            fetch(postUrl, { /* ... opzioni fetch ... */ })
                .then(response => { /* ... gestione risposta ... */ })
                .catch(error => { /* ... gestione errore ... */ });
        });
    } else {
        console.warn("Elemento con ID 'sendToServerButton' non trovato.");
    }

} // Fine della funzione initMap

// Chiama initMap per avviare
initMap();