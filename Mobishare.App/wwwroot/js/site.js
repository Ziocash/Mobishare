// Main map initialization
let map;
let drawingManager;
let currentPolygon = null;
let drawnPolygonWkt = null;
let mapsInitialized = false;

// Wait for Google Maps API to load completely before initializing maps
async function initMaps() {
    if (mapsInitialized) return;
    
    try {
        const { Map } = await google.maps.importLibrary("maps");
        const { DrawingManager } = await google.maps.importLibrary("drawing");
        
        // Initialize main map
        await initMainMap(Map, DrawingManager);
        
        // Initialize city maps in modals
        initCityMaps();
        
        mapsInitialized = true;
        console.log("All maps initialized successfully");
    } catch (error) {
        console.error("Error initializing maps:", error);
    }
}

async function initMainMap(Map, DrawingManager) {
    map = new Map(document.getElementById("map"), {
        center: { lat: 45.50884930272857, lng: 8.950421885612588 },
        zoom: 15,
        styles: [{ featureType: "poi.business", elementType: "all", stylers: [{ visibility: "off" }] }]
    });

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

    // Setup event listeners for buttons
    setupEventListeners();
}

function updateWktAndUI() {
    const wktOutputArea = document.getElementById('wktOutput');
    const sendButton = document.getElementById('sendToServerButton');
    const deleteButton = document.getElementById('deletePolygonButton');

    if (!currentPolygon) {
        drawnPolygonWkt = null;
        if (wktOutputArea) wktOutputArea.value = '';
        if (deleteButton) deleteButton.style.display = 'inline-block';
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
    console.log("WKT Updated:", drawnPolygonWkt);
}

function setupEventListeners() {
    const deleteButton = document.getElementById('deletePolygonButton');
    if (deleteButton) {
        deleteButton.addEventListener('click', (e) => {
            e.preventDefault(); // Prevent form submission
            if (currentPolygon) {
                currentPolygon.setMap(null);
                currentPolygon = null;
                console.log("Polygon removed from map.");
            }

            updateWktAndUI();

            if (drawingManager) {
                drawingManager.setOptions({ drawingControl: true });
            }
        });
    }

    const sendButton = document.getElementById('sendToServerButton');
    if (sendButton) {
        sendButton.addEventListener('click', () => {
            if (!drawnPolygonWkt) {
                alert('Draw and complete a polygon before sending.');
                return;
            }
            const postUrl = '/Home/ProcessPolygonWkt';
            console.log(`Sending WKT to ${postUrl}:`, drawnPolygonWkt);
            // Fetch implementation...
        });
    }
}

// Function to initialize maps in city modals
 function initCityMaps() {
    // Set up listeners for modal show events
    document.querySelectorAll('.modal[data-wkt]').forEach(modal => {
        modal.addEventListener('shown.bs.modal', function() {
            console.log("Modal shown, initializing map");
            const mapId = this.id.replace('editModal-', 'map-edit-');
            const mapContainer = this.querySelector(`#${mapId}`);
            const wktValue = this.getAttribute('data-wkt');
            
            console.log("Map container:", mapId);
            console.log("WKT value:", wktValue);
            
            if (mapContainer && wktValue && !mapContainer.dataset.initialized) {
                try {
                    initializeCityMap(mapContainer, wktValue);
                    mapContainer.dataset.initialized = "true";
                    console.log(`Map initialized for ${mapId}`);
                } catch (error) {
                    console.error("Error initializing city map:", error);
                }
            }
        });
    });
}

function initializeCityMap(mapContainer, wkt) {
    if (!mapContainer || !wkt) {
        console.error("Missing map container or WKT data");
        return;
    }

    const polygon = convertWKTToPolygon(wkt);
    if (!polygon) {
        console.error("Failed to convert WKT to polygon:", wkt);
        return;
    }

    const bounds = new google.maps.LatLngBounds();
    polygon.getPath().forEach(coord => bounds.extend(coord));

    const map = new google.maps.Map(mapContainer, {
        center: bounds.getCenter(),
        zoom: 13,
    });

    polygon.setMap(map);
    map.fitBounds(bounds);

    // Aggiorna il campo hidden con il nuovo WKT
    const cityId = mapContainer.id.replace('map-edit-', '');
    const wktInput = document.getElementById(`editCityWkt-${cityId}`);
    
    function updateWktFromPolygon() {
        const path = polygon.getPath();
        const coords = [];
        path.forEach(latLng => {
            coords.push(`${latLng.lng()} ${latLng.lat()}`);
        });
        if (coords.length > 0) coords.push(coords[0]); // chiudi il poligono
        const newWkt = `POLYGON((${coords.join(', ')}))`;
        if (wktInput) {
            wktInput.value = newWkt;
        }
        console.log("Updated modal WKT:", newWkt);
    }

    // Listener per modifiche manuali
    const path = polygon.getPath();
    google.maps.event.addListener(path, 'set_at', updateWktFromPolygon);
    google.maps.event.addListener(path, 'insert_at', updateWktFromPolygon);
    google.maps.event.addListener(path, 'remove_at', updateWktFromPolygon);
    google.maps.event.addListener(polygon, 'dragend', updateWktFromPolygon);

    // Trigger resize nel caso non venga visualizzato correttamente
    setTimeout(() => {
        google.maps.event.trigger(map, 'resize');
        map.fitBounds(bounds);
    }, 100);
}


function convertWKTToPolygon(wkt) {
    try {
        const match = wkt.match(/POLYGON\s*\(\(\s*(.*?)\s*\)\)/i);
        if (!match) {
            console.error("Invalid WKT format:", wkt);
            return null;
        }

        const coordStr = match[1];
        const coordPairs = coordStr.split(',').map(pair => {
            const [lng, lat] = pair.trim().split(/\s+/).map(parseFloat);
            return new google.maps.LatLng(lat, lng);
        });

        return new google.maps.Polygon({
            paths: coordPairs,
            strokeColor: "#007bff",
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: "#007bff",
            fillOpacity: 0.35,
            editable: true, // <== permette di spostare i vertici
            draggable: true  // <== permette di trascinare il poligono
        });
    } catch (error) {
        console.error("Error converting WKT to polygon:", error, wkt);
        return null;
    }
}

// Initialize maps when DOM is loaded
document.addEventListener("DOMContentLoaded", function() {
    console.log("DOM loaded, initializing maps");
    initMaps();
    
    // Add global listener for all modals
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('shown.bs.modal', function() {
            console.log(`Modal shown: ${this.id}`);
        });
    });
});

// Additional helper to ensure maps are resized correctly when modal is shown
document.addEventListener('shown.bs.modal', function (event) {
    const modal = event.target;
    const mapContainer = modal.querySelector('.city-map');
    if (mapContainer && mapContainer.dataset.initialized === "true") {
        const map = mapContainer.__gm_map;
        if (map) {
            google.maps.event.trigger(map, 'resize');
        }
    }
}, false);