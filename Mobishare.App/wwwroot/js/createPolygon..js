let map;
let drawingManager;
let selectedShape;
let polygons = [];

function initMap() {
    map = new google.maps.Map(document.getElementById('map'), {
        center: {lat: 41.9028, lng: 12.4964}, // Roma, Italia
        zoom: 8,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    // Inizializza Drawing Manager
    drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: null,
        drawingControl: false,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: ['polygon']
        },
        polygonOptions: {
            editable: true,
            draggable: true,
            fillColor: '#4285F4',
            fillOpacity: 0.3,
            strokeColor: '#4285F4',
            strokeWeight: 2
        }
    });
    
    drawingManager.setMap(map);

    // Aggiungi listener per il completamento del disegno
    google.maps.event.addListener(drawingManager, 'polygoncomplete', function(polygon) {
        selectedShape = polygon;
        polygons.push(polygon);
        drawingManager.setDrawingMode(null);
        
        // Aggiungi listener per tutti gli eventi di modifica del poligono
        // Quando un punto viene spostato
        google.maps.event.addListener(polygon.getPath(), 'set_at', function() {
            updateWKT(polygon);
        });
        
        // Quando un punto viene aggiunto
        google.maps.event.addListener(polygon.getPath(), 'insert_at', function() {
            updateWKT(polygon);
        });
        
        // Quando un punto viene rimosso
        google.maps.event.addListener(polygon.getPath(), 'remove_at', function() {
            updateWKT(polygon);
        });
        
        // Quando l'intero poligono viene trascinato
        google.maps.event.addListener(polygon, 'dragend', function() {
            updateWKT(polygon);
        });
        
        // Aggiorna subito il WKT alla creazione
        updateWKT(polygon);
        
        // Aggiungi listener per click sul poligono per selezionarlo
        google.maps.event.addListener(polygon, 'click', function() {
            selectedShape = polygon;
            updateWKT(polygon);
        });
    });

    // Pulsante per cancellare i disegni
    document.getElementById('clear-drawing').addEventListener('click', function() {
        clearSelection();
        for (let i = 0; i < polygons.length; i++) {
            polygons[i].setMap(null);
        }
        polygons = [];
        document.getElementById('wkt-output').value = '';
    });
    
    // Aggiungi listener per clic sulla mappa per deselezionare
    google.maps.event.addListener(map, 'click', function() {
        clearSelection();
    });
}

// Aggiorna il testo WKT in base al poligono
function updateWKT(polygon) {
    if (!polygon) return;
    
    const path = polygon.getPath();
    const len = path.getLength();
    let wktCoords = [];
    
    for (let i = 0; i < len; i++) {
        const coord = path.getAt(i);
        wktCoords.push(coord.lng() + ' ' + coord.lat());
    }
    
    // Chiudi il poligono aggiungendo il primo punto alla fine
    if (len > 0) {
        const firstCoord = path.getAt(0);
        wktCoords.push(firstCoord.lng() + ' ' + firstCoord.lat());
    }
    
    const wkt = 'POLYGON((' + wktCoords.join(', ') + '))';
    $('#wkt-output').val(wkt);
    document.getElementById('wkt-output').innerText = wkt;
    console.log('WKT aggiornato:', wkt); // Debug
}   

function clearSelection() {
    selectedShape = null;
}