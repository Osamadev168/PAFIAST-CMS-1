var remainingSeconds;

self.addEventListener('message', function (event) {
    if (event.data === 'start') {
        remainingSeconds = parseInt(event.data.remainingSeconds);
        startTimer();
    } else if (event.data === 'stop') {
        self.close();
    }
});

function startTimer() {
    var timerInterval = setInterval(function () {
        remainingSeconds--;

        if (remainingSeconds < 0) {
            clearInterval(timerInterval);
            self.postMessage('finished');
        } else {
            self.postMessage(remainingSeconds);
        }
    }, 1000);
}
