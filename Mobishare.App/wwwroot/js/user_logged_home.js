let reserved = null;
let map; // Mappa globale
let vehicleMarkers = {};
let marker;
document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('vehicleReservationForm');
  form.addEventListener('submit', function (event) {
    event.preventDefault(); // Evita il reload della pagina

    const vehicleId = document.getElementById('selectedVehicleId').value;

    if (reserved != null) {
      showAlreadyReservedPopup();
      const modalEl = document.getElementById('confirmReservationModal');
      const myModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
      myModal.hide();
      return;
    }

    reserved = vehicleId;

    fetch('?handler=ReserveVehicle', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
      },
      body: `vehicleId=${encodeURIComponent(vehicleId)}`
    })
      .then(response => response.ok ? response.text() : Promise.reject('Errore nella prenotazione'))
      .then(data => {
        const myModal = bootstrap.Modal.getInstance(document.getElementById('confirmReservationModal'));
        if (myModal) {
          myModal.hide();
        }
        
        // Non rimuovere il marker, ma aggiorna il suo stile per mostrare che è prenotato
        const selectedId = document.getElementById('selectedVehicleId').value;
        if (vehicleMarkers[selectedId]) {
          updateMarkerStyle(vehicleMarkers[selectedId], true);
        }
      })
      .catch(error => {
        alert('t1' + error);
      });
  });
});


window.onload = initMap;

document.addEventListener('DOMContentLoaded', function () {
  const code = document.getElementById('bikeCode');
  const btn = document.getElementById('startTravelBtn');
  const errorMessage = document.getElementById('errorMessage');

  const hub = new signalR.HubConnectionBuilder().withUrl("/VehicleHub").build();
  hub.start()
    .then(() => console.log("Connessione al SignalR Hub stabilita."))
    .catch(err => console.error(err));

  hub.on("ReceiveVehiclePositionUpdate", (vehicle) => {
    console.log("Messaggio ricevuto:", vehicle);

    const id = vehicle.vehicleId.toString();
    const newPosition = { lat: vehicle.latitude, lng: vehicle.longitude };
    const status = vehicle.status;
    const isReserved = status === "Reserved";

    if (vehicleMarkers[id]) {
      const marker = vehicleMarkers[id];
      const currentPos = marker.getPosition();

      // Aggiorna la posizione se cambiata
      if (currentPos.lat() !== newPosition.lat || currentPos.lng() !== newPosition.lng) {
        marker.setPosition(newPosition);
      }
      
      // Aggiorna lo stile del marker in base al nuovo status
      updateMarkerStyle(marker, isReserved);
    } else {
      // Determina l'icona e lo stile in base allo status
      const markerIcon = getMarkerIcon(isReserved);
      
      // Crea marker separato per ogni veicolo
      const marker = new google.maps.Marker({
        position: newPosition,
        map: map,
        title: isReserved ? `Veicolo ${id} (Prenotato)` : `Veicolo ${id}`,
        icon: markerIcon,
        opacity: isReserved ? 0.8 : 1.0
      });

      // Salva il marker in una mappa con la chiave ID
      vehicleMarkers[id] = marker;

      // Listener separato per ogni marker - solo per veicoli liberi
      if (!isReserved) {
        marker.addListener('click', () => {
          document.getElementById('selectedVehicleId').value = id;
          document.getElementById('vehicleIdDisplay').innerText = id;

          const myModal = new bootstrap.Modal(document.getElementById('confirmReservationModal'));
          myModal.show();
        });
      } else {
        // Per i veicoli prenotati, mostra solo un messaggio informativo
        marker.addListener('click', () => {
          alert(`Veicolo ${id} è attualmente prenotato e non disponibile.`);
        });
      }
    }
  });




  // Funzioni helper per gestire gli stili dei marker
  function getMarkerIcon(isReserved) {
    if (isReserved) {
      // Crea un'icona SVG per i veicoli prenotati (con uno stile diverso)
      const reservedIcon = {
        path: 'M12 2L13.09 8.26L20 9L13.09 9.74L12 16L10.91 9.74L4 9L10.91 8.26L12 2Z', // Una forma di stella
        fillColor: '#FF6B6B', // Rosso per indicare "prenotato"
        fillOpacity: 0.8,
        strokeColor: '#FFFFFF',
        strokeWeight: 2,
        scale: 2
      };
      return reservedIcon;
    } else {
      return {
        url: "/img/vehicle/icon/bicycle.png",
        scaledSize: new google.maps.Size(40, 40)
      };
    }
  }

  function updateMarkerStyle(marker, isReserved) {
    // Aggiorna l'opacità e l'icona in base al nuovo status
    marker.setOpacity(isReserved ? 0.8 : 1.0);
    marker.setIcon(getMarkerIcon(isReserved));
    
    // Aggiorna il titolo
    const vehicleId = marker.getTitle().match(/\d+/)[0];
    marker.setTitle(isReserved ? `Veicolo ${vehicleId} (Prenotato)` : `Veicolo ${vehicleId}`);
    
    // Rimuovi tutti i listener esistenti e aggiungi quello appropriato
    google.maps.event.clearListeners(marker, 'click');
    
    if (!isReserved) {
      marker.addListener('click', () => {
        document.getElementById('selectedVehicleId').value = vehicleId;
        document.getElementById('vehicleIdDisplay').innerText = vehicleId;
        const myModal = new bootstrap.Modal(document.getElementById('confirmReservationModal'));
        myModal.show();
      });
    } else {
      marker.addListener('click', () => {
        alert(`Veicolo ${vehicleId} è attualmente prenotato e non disponibile.`);
      });
    }
  }

  btn.addEventListener('click', function () {
    const vehicleCode = code.value.trim();
    if (!isNaN(vehicleCode) && vehicleCode !== "") {
      errorMessage.style.display = 'none';
      code.value = '';
    } else {
      errorMessage.style.display = 'block';
    }
  });

  code.addEventListener('input', function () {
    errorMessage.style.display = 'none';
  });

  // Salva map in una variabile accessibile al listener
  window.setMapReference = function (m) {
    map = m;
  };


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

        map = new google.maps.Map(document.getElementById('map'), {
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


        map.addListener("idle", () => {
          const bounds = map.getBounds();
          const zoom = map.getZoom();

          Object.values(vehicleMarkers).forEach(marker => {
            const isInsideBounds = bounds.contains(marker.getPosition());
            const isVisible = isInsideBounds && zoom >= 12;
            marker.setVisible(isVisible);
          });
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

//------------------------------------------------------------------------------------------------------
function hidePopup() {
  const popup = document.getElementById('mapPopup');
  
  popup.style.display = 'none';
}

function showPopup() {
    debugger;
  const popup = document.getElementById('mapPopup');

  popup.style.display = 'flex';

  startTimer();
}

let timer_conn = null;

function deleteReservation() {
  const popup = document.getElementById('mapPopup');

  timer_conn.stop();

  popup.style.display = 'none';

  freeVehicle(reserved);
  reserved = null;
}

async function startTimer() {
  if (typeof signalR === "undefined") {
    alert("SignalR non è stato caricato correttamente.");
    return;
  }
  timer_conn = new signalR.HubConnectionBuilder()
    .withUrl("/timerHub")
    .build();

  timer_conn.on("ReceiveTime", seconds => {
    const min = Math.floor(seconds / 60);
    const sec = Math.floor(seconds % 60).toString().padStart(2, '0');
    document.getElementById("timerDisplay").textContent = `${min}:${sec}`;
    console.log('Test secondi: ', seconds);
    
    if (parseInt(seconds) === 0) {
      freeVehicle(reserved);
      reserved = null;
    }
  });

  timer_conn.start()
    .then(() => timer_conn.invoke("StartTimer"))
    .catch(err => console.error(err.toString()));
}

function freeVehicle(id) {
  fetch('?handler=FreeVehicle', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
      'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
    },
    body: `vehicleId=${encodeURIComponent(id)}`
  })
    .then(response => response.ok ? response.text() : Promise.reject('Errore nella liberazione'))
    .then(data => {
      /* Inserire messaggio per dire che la prenotazione è cancellata */
      hidePopup();
      // Non rimuovere il marker, ma ripristina il suo stile per mostrare che è di nuovo libero
      if (vehicleMarkers[id]) {
        updateMarkerStyle(vehicleMarkers[id], false);
      }
    })
    .catch(error => {
      console.log(error);
    });
}

let alreadyReservedTimeout = null;
let alreadyReservedInterval = null;


function showAlreadyReservedPopup() {
  const popup = document.getElementById('alreadyReservedPopup');
  const progressBar = document.getElementById('alreadyReservedProgress');
  popup.style.display = 'flex';
  progressBar.style.width = '100%';

  let duration = 10; // secondi
  let elapsed = 0;

  // Aggiorna la barra ogni 100ms
  alreadyReservedInterval = setInterval(() => {
    elapsed += 0.1;
    let percent = Math.max(0, 100 - (elapsed / duration) * 100);
    progressBar.style.width = percent + "%";
  }, 100);

  // Nascondi dopo 5 secondi
  alreadyReservedTimeout = setTimeout(() => {
    hideAlreadyReservedPopup();
  }, duration * 1000);
}

function hideAlreadyReservedPopup() {
  document.getElementById('alreadyReservedPopup').style.display = 'none';
  // Ferma la progress bar e resetta
  clearTimeout(alreadyReservedTimeout);
  clearInterval(alreadyReservedInterval);
  document.getElementById('alreadyReservedProgress').style.width = '100%';
}

