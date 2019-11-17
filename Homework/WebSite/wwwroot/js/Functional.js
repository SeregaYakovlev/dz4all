document.addEventListener("DOMContentLoaded", function () {
    setStatusOfCheckboxOfLessonContent();
    doctypeForDevelopers();
    setListenersOfEvents();
})

function setListenersOfEvents() {
    document.getElementById("ShowLessonContent").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            ChangeDisplayStatusOfLessonContent("inline");
            setInfoToLocalStorage("DisplayStatusOfLessonContent", "on");
        }
        else {
            ChangeDisplayStatusOfLessonContent("none");
            setInfoToLocalStorage("DisplayStatusOfLessonContent", "off");
        }
    }
}

function setStatusOfCheckboxOfLessonContent() {
    var checkbox = document.getElementById("ShowLessonContent");
    var checkboxValue = getInfoFromLocalStorage("DisplayStatusOfLessonContent");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        ChangeDisplayStatusOfLessonContent("inline");
        checkbox.checked = true;
    }
    else {
        ChangeDisplayStatusOfLessonContent("none");
        checkbox.checked = false;
    }
}
function ChangeDisplayStatusOfLessonContent(param) {
    var lesson_content = document.getElementsByClassName("lesson_content");

    for (var i = 0; i < lesson_content.length; i++) {
        lesson_content[i].style.display = param;
    }
}

function doctypeForDevelopers() {
    var doctypeLinks = ["https://localhost", "https://192.168.2.15"];
    for (var i = 0; i < doctypeLinks.length; i++) {
        if (window.location.origin === doctypeLinks[i]) {
            show();
            console.log("You are using doctype for developers");
            return;
        }
    }
}

function getInfoFromLocalStorage(key) {
    var Info = localStorage.getItem(key);
    var jsonAsObj = JSON.parse(Info);
    return jsonAsObj;
}

function setInfoToLocalStorage(key, value) {
    var InfoAsJson = JSON.stringify(value);
    localStorage.setItem(key, InfoAsJson);
}