let socket = null;

function connect() {
    socket = new WebSocket("ws://localhost:8080");

    socket.onopen = function(e) {
        log("Connected to server");
    };

    socket.onmessage = function(event) {
        log(`Data received: ${event.data}`);
    };

    socket.onclose = function(event) {
        if (event.wasClean) {
            log(`Connection closed cleanly`);
        } else {
            log('Connection died');
        }
    };

    socket.onerror = function(error) {
        log(`Error: ${error.message}`);
    };
}

function sendMessage() {
    const message = document.getElementById("messageInput").value;
    socket.send(message);
    log(`Sent: ${message}`);
}

function log(message) {
    const chatLog = document.getElementById("chatLog");
    chatLog.value += message + '\n';
}

document.getElementById("sendButton").addEventListener("click", sendMessage);

// Establish connection on popup load
connect();
