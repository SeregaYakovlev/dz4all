document.addEventListener("DOMContentLoaded", function () {
    host();
    ifCookiesEnabled();
    checkCookie();
    checkbox();
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
    SendToServer();
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

function checkCookie() {
    console.log("checkCookie");
    var user = getCookie("user");
    if (user === undefined || user === null || user === "") {
        // ЭТОТ КОД ИЗ-ЗА КОТОРОГО ПАДАЕТ ВЕБ-СЕРВЕР!!!
        /*VK.Auth.login(function (response) {
            console.log(response);
            if (response.session !== null) {
                var name = response.session.user.first_name;
                var surname = response.session.user.last_name;
                var id = response.session.user.id;
                SetCookie("user", name + ' ' + surname + ' ' + id, 30);
                show();
            }
        })*/
    }
    else {
        //Обновляем куки
        var space = " ";
        DeleteCookie("user");
        console.log(user);
        var array = user.split(space);
        var id = array[2];
        VK.api("users.get", { 'user_ids': id, 'version': "5.95" }, function (data) {
            var name = data.response[0].first_name;
            var surname = data.response[0].last_name;
            SetCookie("user", name + ' ' + surname + ' ' + id, 30);
        });
        show();
    }
}

function SendToServer() {
    // Post-запрос серверу при авторизации, он идет вместе с файлами Cookies.
    var xhr = new XMLHttpRequest();
    var host = window.location.origin;
    xhr.open("POST", host, true);
    xhr.send();
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

    try {
        if (param === "inline") {
            checkbox.checked = true;
        }
        else {
            checkbox.checked = false;
        }
    }
    catch (e) {
        console.error("CONSOLE ERROR " + e);
    }
}

function checkbox() {
    var checkboxValue = getCookie("checkbox");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        lessonContent("inline");
    }
    else if (checkboxValue === "off") {
        lessonContent("none");
    }
    try {
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
    catch (e) {
        console.error("CONSOLE ERROR " + e);
    }
}

function root() {
    var doctypeLinks = ["http://localhost:5000", "http://192.168.2.15:5000"];
    for (var i = 0; i < doctypeLinks.length; i++) {
        if (window.location.origin === doctypeLinks[i]) {
            show();
            alert("That computer is allowed in root");
        }
    }
}

function host() {
    var host = window.location.origin;
    DeleteCookie("host");
    SetCookie("host", host, 30);
}

function hideEmptyTables() {
    var table = document.getElementsByTagName("table");
    for (var i = 0; i < table.length; i++) {
        if (table[i].innerHTML === "<tbody><tr></tr></tbody>") {
            table[i].style.display = "none";
        }
    }
}