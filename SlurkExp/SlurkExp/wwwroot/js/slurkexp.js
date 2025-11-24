//-------------------------------------------------------------------------------------------------------
// SignalR Chathub

var path = window.location.pathname;

if (path.slice(-1) == '/') {
    path = path.slice(0, -1);
}

var url = "/agentHub";
console.log(url);

const connection = new signalR.HubConnectionBuilder()
    .withUrl(url)
    .build();

connection.on("ReceiveMessage", (user, message) => {
    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const encodedMsg = user + ": " + msg;
    const li = document.createElement("li");
    li.textContent = getDate() + encodedMsg;
    document.getElementById("messagesList").prepend(li);
});

connection.on("ReceiveSignal", (user, message) => {
    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const li = document.createElement("li");
    li.textContent = getDate() + msg;
    document.getElementById("messagesList").prepend(li);
});

connection.on("ReceivePing", (user, message) => {
    console.log("PingPong");
    connection.invoke("Pong", 'javascriptclient', message).catch(err => console.error(err.toString()));
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById("messageInput").focus();

document.getElementById("sendButton").addEventListener("click", event => {
    //const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;
    document.getElementById("messageInput").value = '';
    connection.invoke("SendMessage", 'unitylablive', message).catch(err => console.error(err.toString()));
    document.getElementById("messageInput").focus();
    event.preventDefault();
});

// Execute a function when the user releases a key on the keyboard
document.getElementById("messageInput").addEventListener("keyup", function (event) {
    event.preventDefault();
    if (event.keyCode === 13) {
        document.getElementById("sendButton").click();
    }
});

function getDate() {
    let date = new Date();
    let options = { hour: "2-digit", minute: "2-digit", second: "2-digit" };
    return "[" + date.toLocaleTimeString("de-de", options) + "] ";
}