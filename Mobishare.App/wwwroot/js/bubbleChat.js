const bubble = document.getElementById("chatBubble"); 
const chat = document.getElementById("chatWindow"); 
function toggleChat() { 
    if (chat.style.display === "none") 
    { 
        chat.style.display = "flex"; 
    } 
    else 
    { 
        chat.style.display = "none"; 
    } 
} 

bubble.addEventListener("click", toggleChat);