$(document).ready(function () {
    // Verifica se l'alert è presente
    if ($('#successAlert').length) {
        console.log("Alert trovato, impostando la chiusura...");

        // Chiude l'alert dopo 3 secondi
        setTimeout(function () {
            console.log("3 secondi passati, chiudo l'alert...");
            $('#successAlert').alert('close'); // Usa jQuery per chiudere l'alert
        }, 3000); // 3000 millisecondi = 3 secondi
    } else {
        console.log("Alert non trovato.");
    }
});
