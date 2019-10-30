/* window.onerror должен быть первым. Иначе, если будет ошибка до window.onerror,
то он не инициализируется, и на сервер ничего отправлено не будет. */
/* Не все комментарии можно удалять. Некоторые содержат выключенный функционал */

window.onerror = function (message, url, lineNumber) {
    var browser = BrowserDetect.browser + " " + BrowserDetect.version;
    var OS = BrowserDetect.OS;
    console.log("Сообщение: " + message + "\n" + "Браузер: " + browser + "\n" + "OS: " + OS + "\n(" + url + ":" + lineNumber + ")");
    var msg = ("msg: " + message + "\r\n" + "browser: " + browser + "\r\n" + "OS: " + OS + "\r\n(" + url + ":" + lineNumber + ")");
    SendToServerAboutError(msg);
}

document.addEventListener("DOMContentLoaded", function () {
    checkAuthorization();
    setStatusOfCheckboxOfLessonContent();
    //setStatusOfCheckboxOfHomeworkNotifications();
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

    /*document.getElementById("ShowHomeworkNotifications").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            ChangeDisplayStatusOfHomeworkNotifications("inline", "");
            setInfoToLocalStorage("DisplayStatusOfHomeworkNotifications", "on");
        }
        else {
            ChangeDisplayStatusOfHomeworkNotifications("none", "none");
            setInfoToLocalStorage("DisplayStatusOfHomeworkNotifications", "off");
        }
    }*/
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
    var br = document.getElementsByClassName("br");

    for (var i = 0; i < lesson_content.length; i++) {
        lesson_content[i].style.display = param;
    }
    for (var k = 0; k < br.length; k++) {
        br[k].style.display = param;
    }
}

function setStatusOfCheckboxOfHomeworkNotifications() {
    var checkbox = document.getElementById("ShowHomeworkNotifications");
    var checkboxValue = getInfoFromLocalStorage("DisplayStatusOfHomeworkNotifications");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        ChangeDisplayStatusOfHomeworkNotifications("inline", "");
        checkbox.checked = true;
    }
    else {
        ChangeDisplayStatusOfHomeworkNotifications("none", "none");
        checkbox.checked = false;
    }
}
function ChangeDisplayStatusOfHomeworkNotifications(param1, param2) {
    var homeworkNotifications = document.querySelectorAll(".dzAdded, .dzChanged, .dzDeleted");
    var delSubjectCells = document.querySelectorAll(".delSubjectCell");

    for (var i = 0; i < homeworkNotifications.length; i++) {
        homeworkNotifications[i].style.display = param1;
    }
    for (var k = 0; k < delSubjectCells.length; k++) {
        delSubjectCells[k].style.display = param2;
    }
}

function checkAuthorization() {
    VK.Auth.getLoginStatus(function (response) {
        var status = response.status;
        if (status !== "connected") return;
        else {
            var id = response.session.mid;
            GetUser(id);
            show();
        }
    });
    function GetUser(id) {
        VK.api("users.get", { 'user_ids': id, 'v': "5.95" }, function (data) {
            var name = data.response[0].first_name;
            var surname = data.response[0].last_name;
            SendToServerAboutUser(name, surname);
        });
    }
}

function doctypeForDevelopers() {
    var doctypeLinks = ["http://localhost:5000", "http://192.168.2.15:5000"];
    for (var i = 0; i < doctypeLinks.length; i++) {
        if (window.location.origin === doctypeLinks[i]) {
            show();
            console.log("You are using doctype for developers");
            return;
        }
    }
}

function show() {
    document.getElementById("content").style.display = "inline";
    document.getElementById("vkAuthorizer").style.display = "none";
    document.getElementById("infoVkAuthorization").style.display = "none";
    document.getElementById("siteInfo").style.display = "none";
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

function SendToServerAboutError(error) {
    var xhr = new XMLHttpRequest();
    var host = window.location.origin + "/Shared/JSErrors";
    xhr.open("POST", host, true);
    xhr.send(error);
}

function SendToServerAboutUser(name, surname) {
    var xhr = new XMLHttpRequest();
    var host = window.location.origin;
    var requestBody = name + " " + surname;
    xhr.open("POST", host, true);
    xhr.send(requestBody);
}

