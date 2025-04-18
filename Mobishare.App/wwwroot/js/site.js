// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
let map;
let drawingManager;
let currentPolygon = null;
let drawnPolygonWkt = null;

async function initMap() {
    // Rileva la pagina attuale dal path URL (controlla se contiene "main" o "home")
    const page = window.location.pathname.toLowerCase().includes("main") ? "Main" : "Home";

    // Importa la libreria base di Google Maps
    const { Map } = await google.maps.importLibrary("maps");

    map = new Map(document.getElementById("map"), {
        center: { lat: 45.50884930272857, lng: 8.950421885612588 },
        zoom: 15,
        styles: [
            {
                featureType: "poi.business",
                elementType: "all",
                stylers: [{ visibility: "off" }]
            }
        ]
    });

    // Se siamo sulla pagina "Main", abilita strumenti per disegnare
    if (page === "Main") {
        const { DrawingManager } = await google.maps.importLibrary("drawing");

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

        setupPolygonHandlers(); // Chiama le funzioni per gestire i poligoni
    }
}

function setupPolygonHandlers() {
    google.maps.event.addListener(drawingManager, 'polygoncomplete', (polygon) => {
        if (currentPolygon) {
            currentPolygon.setMap(null);
        }
        currentPolygon = polygon;
        updateWktAndUI();

        drawingManager.setDrawingMode(null);
        drawingManager.setOptions({ drawingControl: false });

        const path = polygon.getPath();
        google.maps.event.addListener(path, 'set_at', updateWktAndUI);
        google.maps.event.addListener(path, 'insert_at', updateWktAndUI);
        google.maps.event.addListener(path, 'remove_at', updateWktAndUI);

        polygon.setEditable(true);
        polygon.setDraggable(true);
    });

    const deleteButton = document.getElementById('deletePolygonButton');
    if (deleteButton) {
        deleteButton.addEventListener('click', () => {
            if (currentPolygon) {
                currentPolygon.setMap(null);
                currentPolygon = null;
            }

            updateWktAndUI();

            if (drawingManager) {
                drawingManager.setOptions({
                    drawingControl: true
                });
            }
        });
    }

    const sendButton = document.getElementById('sendToServerButton');
    if (sendButton) {
        sendButton.addEventListener('click', () => {
            if (!drawnPolygonWkt) {
                alert('Disegna e completa un poligono prima di inviare.');
                return;
            }

            const postUrl = '/Home/ProcessPolygonWkt';
            fetch(postUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ wkt: drawnPolygonWkt })
            })
                .then(res => res.ok ? alert("WKT inviato!") : alert("Errore durante l'invio."))
                .catch(err => alert("Errore di rete: " + err));
        });
    }
}

function updateWktAndUI() {
    const wktOutputArea = document.getElementById('wktOutput');
    const sendButton = document.getElementById('sendToServerButton');
    const deleteButton = document.getElementById('deletePolygonButton');

    if (!currentPolygon) {
        drawnPolygonWkt = null;
        if (wktOutputArea) wktOutputArea.value = '';
        if (sendButton) sendButton.style.display = 'none';
        if (deleteButton) deleteButton.style.display = 'none';
        return;
    }

    const path = currentPolygon.getPath();
    let coordinates = [];
    path.getArray().forEach((latLng) => {
        coordinates.push(`${latLng.lng()} ${latLng.lat()}`);
    });

    if (coordinates.length > 0) {
        coordinates.push(coordinates[0]);
    }

    drawnPolygonWkt = `POLYGON((${coordinates.join(', ')}))`;

    if (wktOutputArea) wktOutputArea.value = drawnPolygonWkt;
    if (sendButton) sendButton.style.display = 'inline-block';
    if (deleteButton) deleteButton.style.display = 'inline-block';
    console.log("WKT Aggiornato:", drawnPolygonWkt);
}

// Avvia la mappa
initMap();
