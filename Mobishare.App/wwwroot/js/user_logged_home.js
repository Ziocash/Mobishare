document.addEventListener('DOMContentLoaded', function() {
    const code = document.getElementById('bikeCode');
    const btn = document.getElementById('startTravelBtn');
    const errorMessage = document.getElementById('errorMessage');
    

    // Inizializza il messaggio di errore come nascosto
    const hub = new signalR.HubConnectionBuilder().withUrl("/VehicleHub").build();
    hub.start().then(() => {
        console.log("Connessione al SignalR Hub stabilita.");
    }
    ).catch(err => console.error(err));
    hub.on("ReceivesVehicles", (message) => {
        console.log("Messaggio ricevuto:", message);
        // Qui puoi gestire il messaggio ricevuto dal server
    }
    );

    
    btn.addEventListener('click', function() {
        const vehicleCode = code.value.trim();
        
        if (!isNaN(vehicleCode) && vehicleCode !== "") {
            errorMessage.style.display = 'none';
            code.value = '';
        } else {
            errorMessage.style.display = 'block';
        }
    });
    
    code.addEventListener('input', function() {
        errorMessage.style.display = 'none';
    });
});

//------------------------------------------------------------------------------------------------------

function initMap() {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function (position) {
          const userLatLng = {
            lat: position.coords.latitude,
            lng: position.coords.longitude
          };

          const map = new google.maps.Map(document.getElementById('map'), {
            center: userLatLng,
            zoom: 18,
            mapTypeId: 'roadmap'
          });

          const marker = new google.maps.Marker({
            position: userLatLng,
            map: map,
            title: "Sei qui!"
            /*
            icon: {
                url: "https://maps.google.com/mapfiles/ms/icons/red-dot.png", // esempio
                scaledSize: new google.maps.Size(30, 30) // opzionale: ridimensiona
            }*/
          });
          
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: userLatLng }, function (results, status) {
        if (status === 'OK') {
            if (results[0]) {
            locationLabel.innerText = results[0].formatted_address;
            } else {
            locationLabel.innerText = "Indirizzo non trovato.";
            }
        } else {
            console.error('Geocoder fallito per: ' + status);
            locationLabel.innerText = "Errore nel recupero dell’indirizzo.";
        }
        });
        },
        function (error) {
          document.getElementById('locationLabel').innerText = "Accesso alla posizione negato.";
          console.error("Errore geolocalizzazione:", error.message);
        }
      );
    } else {
      document.getElementById('locationLabel').innerText = "Geolocalizzazione non supportata.";
    }
  }

  window.onload = initMap;