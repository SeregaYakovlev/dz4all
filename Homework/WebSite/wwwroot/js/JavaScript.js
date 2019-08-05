window.onerror = function (message, url, lineNumber) {
    var browser = navigator.userAgent;
    console.log("Сообщение: " + message + "\n" + "Браузер: " + browser + "\n(" + url + ":" + lineNumber + ")");
    var msg = "Сообщение: " + message + "\r\n" + "Браузер: " + browser + "\r\n(" + url + ":" + lineNumber + ")";
    SendToServerAboutError(msg);
}

document.addEventListener("DOMContentLoaded", function () {
    ifCookiesEnabled();
    checkAutorization();
    options();
    setCheckboxLessonContent();
    setCheckboxHomeworkNotifications();
    setCheckboxDoneHomework();
    //root();
    hideEmptyElements();
})

function setClickEventListener() {
    var table = document.getElementsByTagName("table");
    var row;
    var cell;
    var isHomework;
    for (var i = 0; i < table.length; i++) {
        for (var k = 1; k < table[i].rows.length; k++) {
            row = table[i].rows[k];
            for (var j = 0; j < row.cells.length; j++) {
                cell = row.cells[j];
                isHomework = cell.querySelector(".homework");
                if (!cell.classList.contains("delSubjectCell") && cell.textContent !== "" && isHomework != null) {
                    cell.onclick = function () {
                        setHomeworkAsDoneOrDelete(this);
                    }
                }
            }
        }
    }
}

function removeClickEventListener() {
    var table = document.getElementsByTagName("table");
    var row;
    var cell;
    for (var i = 0; i < table.length; i++) {
        for (var k = 1; k < table[i].rows.length; k++) {
            row = table[i].rows[k];
            for (var j = 0; j < row.cells.length; j++) {
                cell = row.cells[j];
                cell.onclick = null;
            }
        }
    }
}

function CalculateCellAsString(cell, selectors) {
    var cellElements = cell.querySelectorAll(selectors);
    var cellAsString = "";
    for (var n = 0; n < cellElements.length; n++) {
        cellAsString += cellElements[n].innerHTML;
    };
    return cellAsString;
}

function setHomeworkAsDoneOrDelete(cell) {
    console.log("Click on");
    console.log(cell);
    var tdAsString = CalculateCellAsString(cell, ".subject, .homework");
    if (!cell.classList.contains("HomeworkDone")) {
        cell.classList.add("HomeworkDone");
        var array = getInfoFromLocalStorage("HomeworkDone") || new Array();
        array.push(tdAsString);
        setInfoToLocalStorage("HomeworkDone", array);
    }
    else {
        cell.classList.remove("HomeworkDone");
        var localStorageArray = getInfoFromLocalStorage("HomeworkDone");
        for (var element = 0; element < localStorageArray.length; element++) {
            if (localStorageArray[element] !== tdAsString) continue;
            else {
                localStorageArray.splice(element, 1);
                console.log(localStorageArray);
                setInfoToLocalStorage("HomeworkDone", localStorageArray);
            }
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

function showDoneHomework() {
    console.time("showDoneHomework");
    var table = document.getElementsByTagName("table");
    var row;
    var cell;
    var storageArray = getInfoFromLocalStorage("HomeworkDone");
    var cellAsString;
    if (storageArray != null) {
        for (var i = 0; i < table.length; i++) {
            for (var k = 1; k < table[i].rows.length; k++) {
                row = table[i].rows[k];
                for (var j = 0; j < row.cells.length; j++) {
                    cell = row.cells[j];
                    cellAsString = CalculateCellAsString(cell, ".subject, .homework");
                    for (var storageIndex = 0; storageIndex < storageArray.length; storageIndex++) {
                        if (storageArray[storageIndex] === cellAsString) {
                            cell.classList.add("HomeworkDone");
                        }
                    }
                }
            }
        }
    }
    console.timeEnd("showDoneHomework");
}

function options() {
    document.getElementById("options").onclick = function () {
        var options = document.getElementById("form");
        var visible = options.style.display;
        if (visible !== "inline") {
            options.style.display = "inline";
        }
        else {
            options.style.display = "none";
        }
    }
}

function ifCookiesEnabled() {
    console.log("ifCookiesEnabled");
    var cookiesEnabled = navigator.cookieEnabled;
    console.log(cookiesEnabled);
    if (!cookiesEnabled) {
        var para = document.createElement("p");
        para.innerHTML = `Файлы cookies выключены. Некоторые функции могут не работать.`;
        document.getElementsByTagName("header")[0].appendChild(para);
        para.className = "announcement";
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

function setCheckboxLessonContent() {
    var checkboxValue = getCookie("checkboxLessonContent");
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
            DeleteCookie("checkboxLessonContent");
            SetCookie("checkboxLessonContent", "on", 30);
        }
        else {
            lessonContent("none");
            DeleteCookie("checkboxLessonContent");
            SetCookie("checkboxLessonContent", "off", 30);
        }
    }
}

function homeworkNotifications(param1, param2) {
    var checkbox = document.getElementById("ShowHomeworkNotifications");
    var homeworkNotifications = document.querySelectorAll(".dzAdded, .dzChanged, .dzDeleted");
    var delSubjectCells = document.querySelectorAll(".delSubjectCell");

    for (var i = 0; i < homeworkNotifications.length; i++) {
        homeworkNotifications[i].style.display = param1;
    }
    for (var k = 0; k < delSubjectCells.length; k++) {
        delSubjectCells[k].style.display = param2;
    }

    if (param1 === "inline") {
        checkbox.checked = true;
    }
    else {
        checkbox.checked = false;
    }
}

function setCheckboxHomeworkNotifications() {
    var checkboxValue = getCookie("checkboxHomeworkNotifications");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        homeworkNotifications("inline", "");
    }
    else {
        homeworkNotifications("none", "none");
    }
    document.getElementById("ShowHomeworkNotifications").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            homeworkNotifications("inline", "");
            DeleteCookie("checkboxHomeworkNotifications");
            SetCookie("checkboxHomeworkNotifications", "on", 30);
        }
        else {
            homeworkNotifications("none", "none");
            DeleteCookie("checkboxHomeworkNotifications");
            SetCookie("checkboxHomeworkNotifications", "off", 30);
        }
    }
}

function homeworkDone(param) {
    var checkbox = document.getElementById("ShowDoneHomework");
    var elementsDoneHomework = document.getElementsByClassName("HomeworkDone");

    if (param === "remove") {
        removeClickEventListener();
        for (var i = 0; i < elementsDoneHomework.length; i++) {
            elementsDoneHomework[i].classList.remove("HomeworkDone");
            i--;
        }
    }
    else if (param === "add") {
        showDoneHomework();
        setClickEventListener();
    }
    if (param === "add") {
        checkbox.checked = true;
    }
    else {
        checkbox.checked = false;
    }
}

function setCheckboxDoneHomework() {
    var checkboxValue = getCookie("checkboxDoneHomework");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        homeworkDone("add");
    }
    else {
        homeworkDone("remove");
    }
    document.getElementById("ShowDoneHomework").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            homeworkDone("add");
            DeleteCookie("checkboxDoneHomework");
            SetCookie("checkboxDoneHomework", "on", 30);
        }
        else {
            homeworkDone("remove");
            DeleteCookie("checkboxDoneHomework");
            SetCookie("checkboxDoneHomework", "off", 30);
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

function hideEmptyElements() {
    hideTables();
    deleteEmptyPaintedElements();
    function deleteEmptyPaintedElements() {
        var delElements = document.getElementsByClassName("painted");
        var delElement;
        for (var x = 0; x < delElements.length; x++) {
            delElement = delElements[x];
            if (delElement.textContent === "") {
                delElement.classList.remove("painted");
                x--;
            }
        }
    }
    function hideTables() {
        var tables = document.getElementsByTagName("table");

        for (var b = 0; b < tables.length; b++) {
            var isTableEmpty = true;
            for (var m = 0; m < tables[b].rows.length; m++) {
                var isRowEmpty = checkIfCellsAreEmpty(tables[b].rows[m]);
                if (!isRowEmpty) isTableEmpty = false;
                else {
                    tables[b].rows[m].style.display = "none";
                }
            }
            if (isTableEmpty) {
                tables[b].style.display = "none";
            }
        }
    }

    function checkIfCellsAreEmpty(row) {
        var cells = row.cells;
        var isCellEmpty = false;
        for (var j = 0; j < cells.length; j++) {
            if (cells[j].innerHTML !== '') {
                return isCellEmpty;
            }
        }
        return !isCellEmpty;
    }
}


function SendToServerAboutError(error) {

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