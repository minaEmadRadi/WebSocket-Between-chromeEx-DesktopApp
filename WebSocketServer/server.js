const WebSocket = require('ws');
const readline = require('readline');

const wss = new WebSocket.Server({ port: 8080 });

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

wss.on('connection', function connection(ws) {
  console.log('Client connected');

  ws.on('message', function incoming(message) {
    console.log(`Received: ${message}`);
  });

  ws.on('close', function() {
    console.log('Client disconnected');
  });
});

function sendMessage() {
  rl.question('Enter message to send: ', (msg) => {
    wss.clients.forEach(function each(client) {
      if (client.readyState === WebSocket.OPEN) {
        client.send(msg);
      }
    });
    sendMessage(); // Prompt for another message
  });
}

console.log('WebSocket server started on ws://localhost:8080');
sendMessage(); // Start the message prompt
