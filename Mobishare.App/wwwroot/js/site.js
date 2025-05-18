let map;
let drawingManager;
let allCityMaps = [];
let currentPolygon = null;
let drawnPolygonWkt = null;
let mapsInitialized = false;

let editablePolygonStrokeColor = '#0056b3';
let editablePolygonFillColor = '#007bff';

let readOnlyPolygonStrokeColor = '#00b3aa';
let readOnlyPolygonFillColor = '#00fff2';

let parkingPolygonStrokeColor = '';
let parkingPolygonFillColor = '';

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
        if (map) google.maps.event.trigger(map, 'resize');
    }
}, false);

/**
 * Initialize Google Maps and Drawing Manager.
 *
 * This function loads the Google Maps library and initializes the main map and drawing manager.
 *
 * It also sets up event listeners for the drawing manager and buttons.
 *
 * It initializes the city maps to draw polygons and initializes all modals to update the WKT value for each city.
 * @returns {Promise<void>}
 */
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

/**
 * Initialize the main map with a drawing manager.
 *
 * This function sets up the map, drawing manager, and event listeners for drawing polygons.
 *
 * It also handles multiple WKT to draw all cities in the modal.
 *
 * Can save the drawn polygon to the server.
 * @param {*} Map - Map class from Google Maps.
 * @param {*} DrawingManager - DrawingManager class from Google Maps.
 * @returns {Promise<void>}
 */
async function initMainMap(Map, DrawingManager) {
    map = new Map(document.getElementById("map"), {
        center: { lat: 45.50884930272857, lng: 8.950421885612588 },
        zoom: 10,
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

    displayAllPolygons("allCities", readOnlyPolygonStrokeColor, readOnlyPolygonFillColor, map);
    displayAllPolygons("allParkingSlots", editablePolygonStrokeColor, editablePolygonFillColor, map, indexZ = 2);
    

    google.maps.event.addListener(drawingManager, 'polygoncomplete', (polygon) => {
        if (currentPolygon) currentPolygon.setMap(null);
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


/**
 * Display all polygons on the map from a WKT input element.
 * @param {string} elementId - The ID of the input element containing WKT data
 * @param {*} polygonStrokeColor - The stroke color for the polygons
 * @param {*} polygonFillColor - The fill color for the polygons
 * @param {*} outMap - The map to display the polygons on
 */
function displayAllPolygons(elementId, polygonStrokeColor, polygonFillColor, outMap, removePolygons = '', indexZ = 1)
{
    var inputElement = document.getElementById(elementId);

    if (!inputElement) return; // console.error("Element not found:", elementId);
        
    if(inputElement.value == null || inputElement.value == "") return; //console.log("No WKT data found for element:", elementId);

    allWkt = inputElement.value.split(';').filter(wkt => wkt.trim() !== '');

    if (removePolygons) 
        allWkt = allWkt.filter(wkt => wkt !== removePolygons);



    allWkt.forEach(wkt => {
        console.log("Processing city WKT:", wkt);
        const polygon = convertWKTToPolygon(
            wkt,
            polygonStrokeColor,
            polygonFillColor,
            false,
            false,
            false,
            indexZ
        );
        if (polygon) polygon.setMap(outMap);
    });
}

/**
 * Update the WKT output area and UI elements based on the current polygon.
 * 
 * This function retrieves the coordinates of the current polygon, converts them to WKT format,
 * and updates the WKT output area and button visibility accordingly.
 * 
 * If no polygon is drawn, it clears the WKT output and hides the buttons.
 */
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
    if (coordinates.length > 0) coordinates.push(coordinates[0]);
    
    drawnPolygonWkt = `POLYGON((${coordinates.join(', ')}))`;

    if (wktOutputArea) wktOutputArea.value = drawnPolygonWkt;
    if (sendButton) sendButton.style.display = 'inline-block';
    if (deleteButton) deleteButton.style.display = 'inline-block';
    console.log("WKT Updated:", drawnPolygonWkt);
}

/**
 * Setup event listeners for buttons.
 */
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

            if (drawingManager) drawingManager.setOptions({ drawingControl: true });
            
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

/**
 * Initialize city maps in modals.
 */
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

/**
 * Initialize a city map in a modal.
 * 
 * This function sets up the map, converts WKT to a polygon, and updates the WKT input field.
 * 
 * It also sets up event listeners for polygon modifications.
 * @param {*} mapContainer - The map container element.
 * @param {*} wkt - The WKT string to convert.
 * @returns 
 */
function initializeCityMap(mapContainer, wkt) {
    if (!mapContainer || !wkt) {
        console.error("Missing map container or WKT data");
        return;
    }

    const polygon = convertWKTToPolygon(wkt, indexZ = 3);
    if (!polygon) {
        console.error("Failed to convert WKT to polygon:", wkt);
        return;
    }

    const bounds = new google.maps.LatLngBounds();
    polygon.getPath().forEach(coord => bounds.extend(coord));

    const map = new google.maps.Map(mapContainer, {
        center: bounds.getCenter(),
        zoom: 13,
        styles: [{ featureType: "poi.business", elementType: "all", stylers: [{ visibility: "off" }] }],
    });

    polygon.setMap(map);
    map.fitBounds(bounds);

    // Aggiorna il campo hidden con il nuovo WKT
    const cityId = mapContainer.id.replace('map-edit-', '');
    const wktInput = document.getElementById(`editWkt-${cityId}`);

    displayAllPolygons("allCities", readOnlyPolygonStrokeColor, readOnlyPolygonFillColor, map, wkt, indexZ = 2);

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

/**
 * Convert WKT to Google Maps Polygon object.
 * @param {string} wkt - The WKT string to convert
 * @param {string} polygonStrokeColor - The stroke color for the polygon
 * @param {string} polygonFillColor - The fill color for the polygon
 * @param {boolean} isEditable - Whether the polygon is editable (default: true)
 * @param {boolean} isDraggable - Whether the polygon is draggable (default: true)
 * @param {boolean} isClickable - Whether the polygon is clickable (default: true)
 * @returns {google.maps.Polygon} - The Google Maps Polygon object
 */
function convertWKTToPolygon(
    wkt,
    polygonStrokeColor = editablePolygonStrokeColor,
    polygonFillColor = editablePolygonFillColor,
    isEditable = true,
    isDraggable = true,
    isClickable = true,
    indexZ = 1
) {
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
            strokeColor: polygonStrokeColor,
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: polygonFillColor,
            fillOpacity: 0.35,
            editable: isEditable,
            draggable: isDraggable,
            clickable: isClickable,
            zIndex: indexZ
        });
    } catch (error) {
        console.error("Error converting WKT to polygon:", error, wkt);
        return null;
    }
}