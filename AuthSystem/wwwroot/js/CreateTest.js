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
        var submitButton = document.getElementById("submit-btn").style.display = 'flex'

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
