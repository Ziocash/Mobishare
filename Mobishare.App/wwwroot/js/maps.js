(async () => {
    // Funzione per caricare lo script in modo asincrono
    const loadGoogleMapsAPI = () => new Promise((resolve, reject) => {
        var a = document.createElement("script");
        const apiKey = document.getElementById('map').dataset.apiKey; // Chiave API dal data attribute

        // Parametri della query
        const params = new URLSearchParams({
            key: apiKey,
            v: "weekly",
            libraries: "places", // Aggiungi qui altre librerie se necessarie
            callback: "initMap"
        });

        // Costruisci URL per lo script
        a.src = `https://maps.googleapis.com/maps/api/js?${params.toString()}`;
        a.async = true;
        a.defer = true;
        
        // Gestione degli errori nel caricamento dello script
        a.onerror = reject;
        
        // Aggiungi lo script al documento
        document.head.appendChild(a);
    });

    // Funzione di callback per inizializzare la mappa
    const initMap = () => {
        let map;
        
        // Se la geolocalizzazione è supportata, ottieni la posizione corrente
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(position => {
                const userLatitude = position.coords.latitude;
                const userLongitude = position.coords.longitude;

                // Inizializza la mappa centrata sulla posizione corrente
                map = new google.maps.Map(document.getElementById("map"), {
                    center: { lat: userLatitude, lng: userLongitude },
                    zoom: 12
                });

                // Aggiungi un marcatore per la posizione corrente
                new google.maps.Marker({
                    position: { lat: userLatitude, lng: userLongitude },
                    map: map,
                    title: "La tua posizione"
                });

            }, () => {
                // Se la geolocalizzazione fallisce, centra la mappa su Milano
                console.error("Impossibile ottenere la posizione dell'utente.");
                map = new google.maps.Map(document.getElementById("map"), {
                    center: { lat: 45.4642, lng: 9.19 }, // Milano come fallback
                    zoom: 12
                });
            });
        }
    };

    // Carica l'API di Google Maps e chiama la funzione di inizializzazione
    try {
        await loadGoogleMapsAPI();
        initMap();  // Inizializza la mappa dopo il caricamento dell'API
        console.log("Mappa caricata con successo");
    } catch (error) {
        console.error("Errore nel caricamento delle Google Maps API:", error);
    }
})();
