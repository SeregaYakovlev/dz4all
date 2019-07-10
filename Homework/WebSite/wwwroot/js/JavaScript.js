window.onerror = function (message, url, lineNumber) {
    var browser = navigator.userAgent;
    var msg = "Сообщение: " + message + "\n" + "Браузер: " + browser + "\n(" + url + ":" + lineNumber + ")";
    SendToServerAboutError(msg);
}

document.addEventListener("DOMContentLoaded", function () {
    ifCookiesEnabled();
    checkAutorization();
    set_checkbox();
    root();
    hideEmptyTables();
})

function ifCookiesEnabled() {
    console.log("ifCookiesEnabled");
    var cookiesEnabled = navigator.cookieEnabled;
    console.log(cookiesEnabled);
    if (!cookiesEnabled) {
        var para = document.createElement("p");
        para.innerHTML = `Файлы cookies выключены. Это нарушает работу сайта.`;
        document.getElementsByTagName("header")[0].appendChild(para);
        para.className = "announcement orange";
    }
}

function show() {
    document.getElementById("content").style.display = "inline";
    document.getElementById("vkAutorizer").style.display = "none";
    document.getElementById("infoVkAutorization").style.display = "none";
}

function SetCookie(name, value, expires) {
    console.log("SetCookie");
    var now = new Date();
    var AddedDate = now.setDate(now.getDate() + expires); // Здесь now переставляется на месяц из-за now.setDate
    var date = new Date(AddedDate);
    document.cookie = "" + name + "=" + encodeURIComponent(value) + ";" + "path=/;" + "expires=" + date.toUTCString();
}

function DeleteCookie(name) {
    console.log("DeleteCookie");
    var date = new Date(0);
    document.cookie = "" + name + "="; "path=/"; "expires=" + date.toUTCString();
}

function getCookie(name) {
    var matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}

function checkAutorization() {
    console.log("checkAutorization");
    VK.Auth.getLoginStatus(function (response) {
        var status = response.status;
        if (status !== "connected") return;
        else {
            var id = response.session.mid;
            GetUser(id);
            show();
        }
    });
}

function GetUser(id) {
    VK.api("users.get", { 'user_ids': id, 'v': "5.95" }, function (data) {
        var name = data.response[0].first_name;
        var surname = data.response[0].last_name;
        SendToServerAboutUser(name, surname);
    });
}

function lessonContent(param) {
    var checkbox = document.getElementById("ShowLessonContent");
    var lesson_content = document.getElementsByClassName("lesson_content");
    var br = document.getElementsByClassName("br");

    for (var i = 0; i < lesson_content.length; i++) {
        lesson_content[i].style.display = param;
    }
    for (var k = 0; k < br.length; k++) {
        br[k].style.display = param;
    }

    if (param === "inline") {
        checkbox.checked = true;
    }
    else {
        checkbox.checked = false;
    }
}

function set_checkbox() {
    var checkboxValue = getCookie("checkbox");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        lessonContent("inline");
    }
    else {
        lessonContent("none");
    }
    document.getElementById("ShowLessonContent").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            lessonContent("inline");
            DeleteCookie("checkbox");
            SetCookie("checkbox", "on", 30);
        }
        else {
            lessonContent("none");
            DeleteCookie("checkbox");
            SetCookie("checkbox", "off", 30);
        }
    }
}

function root() {
    var doctypeLinks = ["http://localhost:5000", "http://192.168.2.15:5000", "http://192.168.1.60:5000"];
    for (var i = 0; i < doctypeLinks.length; i++) {
        if (window.location.origin === doctypeLinks[i]) {
            show();
            alert("That computer is allowed in root");
        }
    }
}

function hideEmptyTables() {
    var table = document.getElementsByTagName("table");
    for (var i = 0; i < table.length; i++) {
        if (table[i].innerHTML === "<tbody><tr></tr></tbody>") {
            table[i].style.display = "none";
        }
    }
}

function SendToServerAboutError(error) {
    console.log(error);
    var xhr = new XMLHttpRequest();
    var host = window.location.origin + "/ServicePages/JSErrors";
    xhr.open("POST", host, true);
    xhr.send(error);
}

function SendToServerAboutUser(name, surname) {
    // Post-запрос серверу при авторизации, он идет вместе с файлами Cookies.
    var xhr = new XMLHttpRequest();
    var host = window.location.origin;
    var requestBody = name + " " + surname + " " + host;
    xhr.open("POST", host, true);
    xhr.send(requestBody);
}