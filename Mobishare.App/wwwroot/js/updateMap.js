// Quando un modale viene mostrato
document.querySelectorAll('[id^="editModal-"]').forEach(modal => {
    modal.addEventListener('shown.bs.modal', async function () {
        const modalId = modal.id.split('-')[1];
        const mapContainerId = `map-edit-${modalId}`;
        const wkt = document.querySelector(`#editModal-${modalId}`).getAttribute('data-wkt');

        const { Map } = await google.maps.importLibrary("maps");

        console.log("Inizializzazione mappa per il modale:", mapContainerId);
        
        const map = new Map(document.getElementById(mapContainerId), {
            center: { lat: 45.50884930272857, lng: 8.950421885612588 }, // default
            zoom: 15,
            styles: [{ featureType: "poi.business", elementType: "all", stylers: [{ visibility: "off" }] }]
        });

        if (wkt && wkt.startsWith("POLYGON")) {
            const coordinates = parseWktPolygon(wkt);

            const polygon = new google.maps.Polygon({
                paths: coordinates,
                strokeColor: "#FF0000",
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: "#FF0000",
                fillOpacity: 0.35,
                map: map
            });

            const bounds = new google.maps.LatLngBounds();
            coordinates.forEach(coord => bounds.extend(coord));
            map.fitBounds(bounds);
        }
    });
});

function parseWktPolygon(wkt) {
    const coordText = wkt.match(/\(\((.*?)\)\)/)[1];
    return coordText.split(',').map(pair => {
        const [lng, lat] = pair.trim().split(' ').map(Number);
        return { lat: lat, lng: lng };
    });
}