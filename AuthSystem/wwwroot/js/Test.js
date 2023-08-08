function updateAttemptedCount() {
    var sectionTabs = document.querySelectorAll('.nav-link[data-bs-toggle="tab"]');
    var totalAttemptedQuestions = 0;

    sectionTabs.forEach(tab => {
        var sectionId = tab.getAttribute('data-bs-target').replace('#nav-', '');

        var attemptedQuestions = document.querySelectorAll('#section-' + sectionId + ' .options input:checked');
        var attemptedCount = attemptedQuestions.length;
        totalAttemptedQuestions += attemptedCount;
        var totalQuestions = document.querySelectorAll('#section-' + sectionId + ' .options input');
        var totalCount = totalQuestions.length / 4;
        document.getElementById('section-' + sectionId).setAttribute('data-attempted-count', attemptedCount);
        tab.innerHTML = sectionId + ' <span class="badge rounded-pill bg-success">' + attemptedCount + `/${totalCount}` + '</span>';
    });

    document.getElementById('total-questions').innerText = totalAttemptedQuestions + ' Question(s) attempted so far';
}

function saveUserResponse(element) {
    var url = window.location.href;
    var testId = url.split("/").pop().split("?")[0];
    var questionId = element.name.replace("answers[", "").replace("]", "");
    var selectedAnswer = element.value;
    document.getElementById("answer_" + questionId).value = selectedAnswer;

    var data = {};
    data[questionId] = selectedAnswer;
    fetch('/Test/SaveUserResponse?testId=' + testId, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data),
    })
        .then(response => {
            if (response.ok) {
                console.log('User response saved successfully!');
            } else {
                console.error('Error saving user response:', response.statusText);
            }
        })
        .catch(error => {
            console.error('Error saving user response:', error);
        });

    updateAttemptedCount();
}

function fetchUserResponses() {
    var url = window.location.href;
    var testId = url.split("/").pop().split("?")[0];
    console.log(testId)
    fetch('/Test/FetchUserResponses?testId=' + testId)
        .then(response => response.json())
        .then(data => {
            data.forEach(item => {
                var questionId = item.questionId;
                var userResponse = item.userResponse;
                var radioId = questionId + userResponse;

                var radioElement = document.getElementById(radioId);
                if (radioElement) {
                    radioElement.checked = true;

                    document.getElementById("answer_" + questionId).value = userResponse;
                } else {
                    console.error('Radio element not found:', radioId);
                }
            });
            updateAttemptedCount();
        })
        .then(() => {
            var radioButtons = document.getElementsByClassName('options');
            for (var i = 0; i < radioButtons.length; i++) {
                radioButtons[i].dispatchEvent(new Event('change'));
            }
        })
        .catch(error => {
            console.error('Error fetching user responses:', error);
        });
}
function getTestName() {
    var url = window.location.href;
    var testId = url.split("/").pop().split("?")[0];
    fetch('/Test/GetTestName?testId=' + testId).then(response => response.json()).then(name => {
        document.getElementById('test-name').innerText = name
    });
}
function getTestDuration() {
    var url = window.location.href;
    var testId = url.split("/").pop().split("?")[0];
    const urlParams = new URLSearchParams(url.substring(url.indexOf('?')));
    const cId = urlParams.get("C_Id");
    fetch(`/Test/GetTestDuration?testId=${testId}&C_Id=${cId}`)
        .then(response => response.json())
        .then(remainingMinutes => {
            var hours = Math.floor(remainingMinutes / 60);
            var minutes = Math.floor(remainingMinutes % 60);
            var seconds = 0;
            console.log(remainingMinutes)
            var timer = setInterval(function () {
                $('#test-duration').text(hours + " H " + minutes + " M " + seconds + " S");
                seconds--;
                if (seconds < 0) {
                    if (minutes > 0) {
                        minutes--;
                        seconds = 59;
                    } else if (hours > 0) {
                        hours--;
                        minutes = 59;
                        seconds = 59;
                    }
                    else if (minutes < 2) {
                        document.getElementById('test-duration').style.backgroundColor = 'red';
                    }
else {
                        clearInterval(timer);
                        $('#test-duration').text('Test is finished');
                    }
                }
            }, 1000);
        });
}
function SaveStartTime() {
    var url = window.location.href;
    var testId = url.split("/").pop().split("?")[0];
    fetch('/Test/SaveStartTime?testId=' + testId).then(() => {
    })
}
window.addEventListener('load', function () {
    SaveStartTime();
    getTestDuration();
    getTestName();
    fetchUserResponses();
});
// Function to request full screen
function requestFullScreen(element) {
    if (element.requestFullscreen) {
        element.requestFullscreen();
    } else if (element.mozRequestFullScreen) {
        element.mozRequestFullScreen();
    } else if (element.webkitRequestFullscreen) {
        element.webkitRequestFullscreen();
    } else if (element.msRequestFullscreen) {
        element.msRequestFullscreen();
    }
}

// Attach a click event to the fullScreenButton
document.getElementById('fullScreenButton').addEventListener('click', function () {
    // Request full screen when the button is clicked
    requestFullScreen(document.documentElement);
});