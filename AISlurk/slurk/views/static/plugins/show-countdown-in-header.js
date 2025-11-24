// delayed execution to ensure socket.io connection is alive
//
setTimeout(function() {
    socket.on("client_broadcast",
      function (payload) {
        if (payload['type'] != 'time_left_chatroom') return;
        if (payload['room'] != self_room) return;
        const time_left = payload['time_left'];
        const minutes = Math.floor(time_left / 60);
        const seconds = time_left % 60;
        var msg = "";
        if (minutes > 0) {
            msg = `${minutes} minutes and ${seconds} seconds left`;
        } else if (seconds > 0) {
            msg = `${seconds} seconds left`;
        }
        else {
            msg = '';
        }
        $("#subtitle")[0].innerText = msg;
      }
    )
}, 500)
