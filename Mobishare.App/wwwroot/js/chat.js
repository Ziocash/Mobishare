const chatBox = document.getElementById("chatBox");
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

const conKernel = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();    

let currentBotMessage = null;
let currentBotTimeout = null;
let currentBotDateTime = null;
let typingIndicator = null;
let isRedirecting = false;

// Funzione per mostrare l'indicatore di typing
function showTypingIndicator() {
    if (typingIndicator) return; // Se già presente, non crearne un altro

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

// Funzione per nascondere l'indicatore di typing
function hideTypingIndicator() {
    if (typingIndicator) {
        typingIndicator.remove();
        typingIndicator = null;
    }
}


conKernel.on("RedirectTo", (route) => {
    if (isRedirecting) return;
    isRedirecting = true;

    // Nascondi l'indicatore di typing
    hideTypingIndicator();

    // Aggiungi un messaggio di sistema
    const systemMsg = document.createElement("div");
    systemMsg.classList.add("system-message");
    systemMsg.innerHTML = `
        Reindirizzamento alla pagina ${route.split('/').pop()}...
        <br />
        <small><em>${new Date().toLocaleTimeString()}</em></small>
    `;
    chatBox.appendChild(systemMsg);
    chatBox.scrollTop = chatBox.scrollHeight;

    // Ritardo per permettere all'utente di leggere il messaggio
    setTimeout(() => {
        window.location.href = route;
    }, 1500);
});

connection.on("ReceiveMessage", (user, message, dateTime) => {
    if (user === "MobishareBot") {
        if (!currentBotMessage) {
            // Nascondi l'indicatore di typing appena arriva il primo token
            hideTypingIndicator();

            currentBotMessage = document.createElement("div");
            currentBotMessage.classList.add("ai-message");
            currentBotMessage.innerHTML = `<span class="content"></span>`;
            chatBox.appendChild(currentBotMessage);
            currentBotDateTime = dateTime;

            currentBotBuffer = "";  // Buffer per il messaggio HTML
        }

        currentBotBuffer += message;

        // Aggiorna il contenuto HTML interpretato
        const contentSpan = currentBotMessage.querySelector(".content");
        contentSpan.innerHTML = currentBotBuffer;

        // Se ci sono altri token in arrivo, resettiamo il timeout
        if (currentBotTimeout) clearTimeout(currentBotTimeout);

        // Impostiamo un timeout per chiudere il messaggio dopo 1 secondo di inattività
        currentBotTimeout = setTimeout(() => {
            const timeElem = document.createElement("small");
            timeElem.innerHTML = `<br /><em>${currentBotDateTime}</em>`;
            currentBotMessage.appendChild(timeElem);
            currentBotMessage = null;
            currentBotTimeout = null;
            currentBotDateTime = null;
            currentBotBuffer = null;
        }, 500); // timeout = tempo massimo tra token consecutivi
    } else {
        // messaggi utente come prima
        const userMsg = document.createElement("div");
        userMsg.classList.add("user-message");
        userMsg.innerHTML = `
        ${message}
        <br />
        <small><em>${dateTime}</em></small>
        `;
        chatBox.appendChild(userMsg);
        currentBotMessage = null;

        // Mostra l'indicatore di typing dopo aver mostrato il messaggio utente
        showTypingIndicator();
    }
    chatBox.scrollTop = chatBox.scrollHeight;
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById("userInput").addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

function sendMessage() {
    const input = document.getElementById("userInput");
    const msg = input.value.trim();
    const id = document.getElementById("conversationId").value;

    // Reset stato redirect
    isRedirecting = false;
    if (!msg) return;

    connection.invoke("SendMessage", id, msg).catch(err => {
        console.error(err.toString());
        // Nascondi l'indicatore in caso di errore
        hideTypingIndicator();
    });

    input.value = "";
    currentBotMessage = null; // resetta ogni volta che invii
}