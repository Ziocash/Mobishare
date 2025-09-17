if (typeof marked === 'undefined') {
    console.error("ERRORE CRITICO: La libreria marked.js non è stata caricata!");
    alert("ERRORE: marked.js non trovato. La formattazione non funzionerà.");
} else {
    console.log("OK: La libreria marked.js è stata caricata correttamente.");
}

const chatBox = document.getElementById("chatBox");
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

// Variabili per gestire lo streaming dei messaggi del bot
let currentBotMessage = null;
let currentBotTimeout = null;
let currentBotDateTime = null;
let currentBotBuffer = "";

// Variabile per l'indicatore di "sta scrivendo"
let typingIndicator = null;

// Funzione per mostrare l'indicatore "sta scrivendo"
function showTypingIndicator() {
    if (typingIndicator) return; // Se già presente, non far nulla

    typingIndicator = document.createElement("div");
    typingIndicator.classList.add("ai-message", "typing-indicator");
    typingIndicator.innerHTML = `
        <div class="typing-dots">
            <span></span>
            <span></span>
            <span></span>
        </div>
    `;
    chatBox.appendChild(typingIndicator);
    chatBox.scrollTop = chatBox.scrollHeight;
}

// Funzione per nascondere l'indicatore "sta scrivendo"
function hideTypingIndicator() {
    if (typingIndicator) {
        typingIndicator.remove();
        typingIndicator = null;
    }
}

// Gestore per i messaggi ricevuti dal server
connection.on("ReceiveMessage", (user, message, dateTime) => {
    if (user === "MobishareBot") {
        // Se è il primo pezzo di messaggio in arrivo...
        if (!currentBotMessage) {
            hideTypingIndicator(); // Nascondi l'indicatore non appena l'IA risponde

            currentBotMessage = document.createElement("div");
            currentBotMessage.classList.add("ai-message");
            currentBotMessage.innerHTML = `<span class="content"></span>`; // Contenitore per il testo
            chatBox.appendChild(currentBotMessage);

            currentBotDateTime = dateTime;
            currentBotBuffer = ""; // Resetta il buffer
        }

        // Aggiungi il nuovo pezzo di messaggio al buffer
        currentBotBuffer += message;

        const contentSpan = currentBotMessage.querySelector(".content");

        // --- PUNTO CHIAVE ---
        // 1. Converti il buffer da Markdown a HTML usando marked.js
        // 2. Pulisci l'HTML risultante per sicurezza usando DOMPurify
        const cleanHtml = DOMPurify.sanitize(marked.parse(currentBotBuffer));
        contentSpan.innerHTML = cleanHtml;
        // --- FINE PUNTO CHIAVE ---

        // Resetta il timer di finalizzazione del messaggio
        if (currentBotTimeout) clearTimeout(currentBotTimeout);

        // Dopo un breve periodo di inattività, finalizza il messaggio aggiungendo l'orario
        currentBotTimeout = setTimeout(() => {
            const timeElem = document.createElement("small");
            timeElem.innerHTML = `<br /><em>${currentBotDateTime}</em>`;
            currentBotMessage.appendChild(timeElem);

            // Resetta le variabili di stato per il prossimo messaggio
            currentBotMessage = null;
            currentBotTimeout = null;
            currentBotDateTime = null;
            currentBotBuffer = "";
        }, 500); // 0.5 secondi di attesa massima tra un pezzo e l'altro

    } else { // Se il messaggio è dell'utente
        const userMsg = document.createElement("div");
        userMsg.classList.add("user-message");
        
        // Pulisci anche il messaggio dell'utente per sicurezza
        const cleanUserMessage = DOMPurify.sanitize(message);
        
        userMsg.innerHTML = `
        <p>
            ${cleanUserMessage}
        </p>
        <small><em>${dateTime}</em></small>
        `;
        chatBox.appendChild(userMsg);
        
        // Dopo aver mostrato il messaggio dell'utente, mostra che il bot sta "pensando"
        showTypingIndicator();
    }
    
    // Scrolla automaticamente alla fine della chat
    chatBox.scrollTop = chatBox.scrollHeight;
});

// Avvia la connessione a SignalR
connection.start().catch(err => console.error(err.toString()));

// Gestisce l'invio del messaggio con il tasto Invio
document.getElementById("userInput").addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault(); // Evita di andare a capo
        sendMessage();
    }
});

// Funzione per inviare un messaggio al server
function sendMessage() {
    const input = document.getElementById("userInput");
    const msg = input.value.trim();
    const id = document.getElementById("conversationId").value;

    if (!msg) return; // Non inviare messaggi vuoti

    connection.invoke("SendMessage", id, msg).catch(err => {
        console.error(err.toString());
        hideTypingIndicator(); // Nascondi l'indicatore anche in caso di errore
    });

    input.value = ""; // Svuota l'input
    currentBotMessage = null; // Resetta lo stato del messaggio del bot
}