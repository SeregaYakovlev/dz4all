document.addEventListener("DOMContentLoaded", function () {
    checkCookie();
    checkbox();
    root();
})
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
    var url = "http://94.188.19.60";
    xhr.open("POST", url, true);
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

    if (param === "inline") {
        checkbox.checked = true;
    }
    else {
        checkbox.checked = false;
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
    var doctypeLinks = ["http://localhost:5000/", "http://192.168.2.15:5000/"];
    for (var i = 0; i < doctypeLinks.length; i++) {
        if (window.location.href === doctypeLinks[i]) {
            show();
            alert("That computer is allowed in root");
        }
    }
    return;
}