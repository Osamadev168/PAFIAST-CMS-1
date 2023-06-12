function calculateTotalPercentage() {
    var totalPercentage = 0;
    document.getElementById('testName').value;
    var percentageInputs = document.querySelectorAll('#subjects-container input[type="number"]:enabled');

    percentageInputs.forEach(function (input) {
        if (input.value) {
            totalPercentage += parseInt(input.value);
        }
    });
    var submitButton = document.getElementById("submit-btn");

    if (totalPercentage != 100) {
        submitButton.setAttribute('disabled', 'disabled');
        if (totalPercentage > 100) {
            document.getElementById('total-percentage').classList.add('total-percentage-err')
        }
    } else {
        submitButton.removeAttribute('disabled');
        document.getElementById('total-percentage').classList.remove('total-percentage-err')

    }

    return totalPercentage;
}
function updateTotalPercentage() {
    var totalPercentageElement = document.getElementById('totalPercentage');
    var totalPercentage = calculateTotalPercentage();
    totalPercentageElement.textContent = totalPercentage;
}
updateTotalPercentage();
document.querySelectorAll('#subjects-container input[type="number"]').forEach(function (input) {
    input.addEventListener('input', updateTotalPercentage);
});
document.querySelectorAll('#subjects-container input[type="checkbox"]').forEach(function (checkbox) {
    checkbox.addEventListener('click', function () {
        var percentageInputId = "percentage_" + checkbox.value;
        var percentageInput = document.getElementById(percentageInputId);
        percentageInput.disabled = !checkbox.checked;
        if (!checkbox.checked) {
            percentageInput.value = "";
        }

        updateTotalPercentage();
    });
});
function handleCheckboxClick(e) {
    var checkbox = e.target;
    var subjectId = checkbox.getAttribute("data-subject-id");
    if (checkbox.checked) {
        fetch('/Test/GetNumberOfQuestions?subjectId=' + subjectId, {
            method: 'GET',
        })
            .then(response => response.json())
            .then(data => {
                var questionCountElement = document.getElementById('question-count-' + subjectId);
                questionCountElement.textContent = data.count + ' Question(s) Available';

                var percentageInputId = "percentage_" + subjectId;
                var percentageInput = document.getElementById(percentageInputId);


                percentageInput.addEventListener('keypress', function () {
                    if (data.count < parseInt(percentageInput.value)) {
                        document.getElementById("err-message-" + subjectId).textContent = 'Not enough questions available in selected subject! (Please add more questions!)';

                        checkbox.checked = false;
                        percentageInput.disabled = true;
                        percentageInput.value = "";
                        updateTotalPercentage()
                    }
                    calculateTotalPercentage();
                });
                calculateTotalPercentage();
            })
            .catch(error => {
                console.log(error);
            });
    } else {
        var questionCountElement = document.getElementById('question-count-' + subjectId);
        questionCountElement.textContent = '';
        var percentageInputId = "percentage_" + subjectId;
        var percentageInput = document.getElementById(percentageInputId);

        percentageInput.disabled = true;
        percentageInput.value = "";

        calculateTotalPercentage();
    }
}
var checkboxes = document.querySelectorAll('input[type="checkbox"][name="selectedSubjectIds"]');
checkboxes.forEach(function (checkbox) {
    checkbox.addEventListener('click', handleCheckboxClick);
});

function copyTestLink(event) {
    var testLink = event.target.dataset.testLink;
    navigator.clipboard.writeText("http://localhost:5173" + testLink)
        .then(function () {
            alert("Test link copied to clipboard!");
        })
        .catch(function (error) {
            console.error("Failed to copy test link: ", error);
        });
}

var copyButtons = document.querySelectorAll(".copy-button");
copyButtons.forEach(function (button) {
    button.addEventListener("click", function (event) {
        event.preventDefault();
        copyTestLink(event);
    });
});
var calendarForms = document.querySelectorAll('#calendars-container form');

calendarForms.forEach(function (form) {
    var startTimeInput = form.querySelector('input[name="startTime"]');
    var testIdInput = form.querySelector('input[name="testId"]');
    var endTimeElement = form.querySelector('#endTime');

    startTimeInput.addEventListener('input', function () {
        var testId = testIdInput.value;
        var startTime = startTimeInput.value;
        fetch(`/Test/GetTestEndTime?testId=${testId}&startTime=${startTime}`)
            .then(function (response) {
                return response.text();
            })
            .then(function (data) {
                endTimeElement.innerHTML = "The end time for this calendar will be " + "<strong>" + data + "</strong>";
            })
            .catch(function (error) {
                console.error(error);
            });
    });
});

document.getElementById('duration').addEventListener('input', () => {
    var durationInput = document.getElementById('duration').value;
    var durationHoursElement = document.getElementById('durationHours');
    var hours = Math.floor(durationInput / 60);
    var minutes = durationInput % 60;
    var timeString = hours + " hours " + minutes + " minutes";
    var durationError = document.getElementById('durationError');
    var timeSpanInput = parseInt(document.getElementById('timeSpan').value);

    durationHoursElement.innerHTML = timeString;
    if (timeSpanInput < durationInput) {
        durationError.innerHTML = 'Timespan cannot be less than duration';

    }
    else {
        durationError.innerHTML = "";
    }
    
})
document.getElementById('timeSpan').addEventListener('input', () => {
    var timeSpanInput = parseInt(document.getElementById('timeSpan').value);
    var timeSpanHoursElement = document.getElementById('timeSpanHours');
    var hours = Math.floor(timeSpanInput / 60).toPrecision();
    var minutes = timeSpanInput % 60;
    var timeString = hours + " hours " + minutes + " minutes";
    timeSpanHoursElement.innerHTML = timeString;

    var durationInput = parseInt(document.getElementById('duration').value);
    var durationError = document.getElementById('durationError');
    if (timeSpanInput < durationInput) {
        durationError.innerHTML = 'Timespan cannot be less than duration';

    } else {
        durationError.innerHTML = "";
    }
    
});

var testNameInput = document.getElementById('testName').value
document.getElementById('testName').addEventListener('blur', () => {



    var testName = document.getElementById('testName').value;
    var submitButton = document.getElementById('submit-btn');
    if (testName !== "") {
        fetch('/Test/CheckTestName?testName=' + testName)
            .then(response => {
                if (response.ok) {
                    return response.text();
                } else {
                    throw new Error('Error validating Test Name');
                }
            })
            .then(data => {
                console.log(data)
                if (data === "true") {
                    document.getElementById('testNameError').textContent = 'Test name not avialable!';
                    document.getElementById('testNameError').style.color = 'red';

                } else if (data === "false") {
                    submitButton.style.backgroundColor = 'red';
                    document.getElementById('testNameError').textContent = 'A great test name indeed!';
                    document.getElementById('testNameError').style.color = 'green';
                    submitButton.style.backgroundColor = '';

                }
            })
            .catch(error => {
                document.getElementById('testNameError').textContent = 'An error occurred: ' + error.message;
            }); 

    }



}); 


