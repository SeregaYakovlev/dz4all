/* window.onerror должен быть первым. Иначе, если будет ошибка до window.onerror,
то он не инициализируется, и на сервер ничего отправлено не будет. */

window.onerror = function (message, url, lineNumber) {
    var browser = BrowserDetect.browser + " " + BrowserDetect.version;
    var OS = BrowserDetect.OS;
    console.log("Сообщение: " + message + "\n" + "Браузер: " + browser + "\n" + "OS: " + OS + "\n(" + url + ":" + lineNumber + ")");
    var msg = ("msg: " + message + "\r\n" + "browser: " + browser + "\r\n" + "OS: " + OS + "\r\n(" + url + ":" + lineNumber + ")");
    SendToServerAboutError(msg);
}

document.addEventListener("DOMContentLoaded", function () {
    checkAuthorization();
    setIconOfOptionsAsClickable();
    setStatusOfCheckboxOfLessonContent();
    setStatusOfCheckboxOfHomeworkNotifications();
    setStatusOfCheckboxOfDoneHomework();
    doctypeForDevelopers();
    hideEmptyElements();
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

    document.getElementById("ShowHomeworkNotifications").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            ChangeDisplayStatusOfHomeworkNotifications("inline", "");
            setInfoToLocalStorage("DisplayStatusOfHomeworkNotifications", "on");
        }
        else {
            ChangeDisplayStatusOfHomeworkNotifications("none", "none");
            setInfoToLocalStorage("DisplayStatusOfHomeworkNotifications", "off");
        }
    }

    document.getElementById("ShowDoneHomework").onchange = function () {
        var checked = this.checked;
        if (checked === true) {
            ChangeDisplayStatusOfDoneHomework("add");
            setInfoToLocalStorage("DisplayStatusOfDoneHomework", "on");
        }
        else {
            ChangeDisplayStatusOfDoneHomework("remove");
            setInfoToLocalStorage("DisplayStatusOfDoneHomework", "off");
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

function setStatusOfCheckboxOfDoneHomework() {
    var checkbox = document.getElementById("ShowDoneHomework");
    var checkboxValue = getInfoFromLocalStorage("DisplayStatusOfDoneHomework");
    if (checkboxValue === null || checkboxValue === undefined || checkboxValue === "" || checkboxValue === "on") {
        ChangeDisplayStatusOfDoneHomework("add");
        checkbox.checked = true;
    }
    else {
        ChangeDisplayStatusOfDoneHomework("remove");
        checkbox.checked = false;
    }
}
function ChangeDisplayStatusOfDoneHomework(param) {
    var elementsDoneHomework = document.getElementsByClassName("DoneHomework");

    if (param === "remove") {
        for (var i = 0; i < elementsDoneHomework.length; i++) {
            elementsDoneHomework[i].classList.remove("DoneHomework");
            i--;
        }
        setOrRemoveClickEventListenerOfDoneHomework("remove");
    }
    else if (param === "add") {
        showDoneHomework();
        setOrRemoveClickEventListenerOfDoneHomework("set");
    }

    function showDoneHomework() {
        var table = document.getElementsByTagName("table");
        var row;
        var cell;
        var cellAsString;
        var storageArray = getInfoFromLocalStorage("DoneHomework");
        if (storageArray != null) {
            var ArrayExtraElements = []; // массив лишних элементов
            ArrayExtraElements = storageArray.slice(0); // просто копируется storageArray
            for (var i = 0; i < table.length; i++) {
                for (var k = 1; k < table[i].rows.length; k++) {
                    row = table[i].rows[k];
                    for (var j = 0; j < row.cells.length; j++) {
                        cell = row.cells[j];
                        cellAsString = CalculateCellAsString(cell, ".subject, .homework");
                        for (var storageIndex = 0; storageIndex < storageArray.length; storageIndex++) {
                            if (storageArray[storageIndex] === cellAsString) {
                                cell.classList.add("DoneHomework");
                                /* Здесь удаляются элементы, которые понадобились.
                                 * Берется один элемент массива лишних элементов,
                                 * и по всем элементам массива localStorage проверяется,
                                 * равен ли он какому-нибудь из них.
                                 * Поскольку индексы у массивов для одних и тех же элементов разные,
                                 * то нужно два цикла, чтобы их найти. */
                                for (var extraIndex = 0; extraIndex < ArrayExtraElements.length; extraIndex++) {
                                    if (storageArray[storageIndex] === ArrayExtraElements[extraIndex]) {
                                        ArrayExtraElements.splice(extraIndex, 1);
                                        // один элемент с индексом [extraIndex];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /* Это нужно для удаления из хранилища старых элементов от прошлых недель, которых уже нет.
             * Удаляется по той же схеме, что и выше. */
            for (var storageInd = 0; storageInd < storageArray.length; storageInd++) {
                for (var extraInd = 0; extraInd < ArrayExtraElements.length; extraInd++) {
                    if (storageArray[storageInd] === ArrayExtraElements[extraInd]) {
                        storageArray.splice(storageInd, 1);
                        storageInd--; // уменьшается длина storageArray, уменьшается и индекс.
                    }
                }
            }
            setInfoToLocalStorage("DoneHomework", storageArray);
        }
    }
}
function setHomeworkAsDoneOrDeleteIfClick(cell) {
    var cellAsString = CalculateCellAsString(cell, ".subject, .homework");
    if (!cell.classList.contains("DoneHomework")) {
        cell.classList.add("DoneHomework");
        var array = getInfoFromLocalStorage("DoneHomework") || new Array();
        array.push(cellAsString);
        setInfoToLocalStorage("DoneHomework", array);
    }
    else {
        cell.classList.remove("DoneHomework");
        var localStorageArray = getInfoFromLocalStorage("DoneHomework");
        for (var element = 0; element < localStorageArray.length; element++) {
            if (localStorageArray[element] !== cellAsString) continue;
            else {
                localStorageArray.splice(element, 1);
                setInfoToLocalStorage("DoneHomework", localStorageArray);
            }
        }
    }
}
function setOrRemoveClickEventListenerOfDoneHomework(param) {
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
                if (!cell.classList.contains("delSubjectCell") && isHomework != null) {
                    if (param === "set") {
                        cell.onclick = function () {
                            setHomeworkAsDoneOrDeleteIfClick(this);
                        }
                    }
                    else if (param === "remove") {
                        cell.onclick = null;
                    }
                }
            }
        }
    }
}
function CalculateCellAsString(cell, selectors) {
    var cellElements = cell.querySelectorAll(selectors);
    var cellAsString = "";
    var element;
    var elementAsString;
    for (var n = 0; n < cellElements.length; n++) {
        element = cellElements[n];
        if (element.classList.contains("subject")) {
            elementAsString = element.innerHTML.replace(":", "");
        }
        else if (element.classList.contains("homework")) {
            elementAsString = element.innerHTML.replace("Дз: ", "");
        }
        cellAsString += elementAsString;
    };
    return cellAsString;
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

function hideEmptyElements() {
    hideTables();
    deleteEmptyPaintedElements();

    function deleteEmptyPaintedElements() {
        /* Для чего эта функция нужна:
         * Дело в том, что все ячейки строки удаленных предметов(delObjects in Index.cshtml) с домашкой желтые.
         * Но ячейки могут быть пустыми. => Эта функция убирает пустые желтые ячейки,
         * чтобы желтым цветом подсвечивались только удаленные предметы с домашкой.*/

        var delElements = document.getElementsByClassName("paintedDelSubjectCell");
        var delElement;
        for (var x = 0; x < delElements.length; x++) {
            delElement = delElements[x];
            if (delElement.textContent === "") {
                delElement.classList.remove("paintedDelSubjectCell");
                x--; // уменьшается длина delElements, уменьшается и индекс.
            }
        }
    }

    function hideTables() {
        /* Эта функция нужна для следущего:
         * Если у меня есть пустые json-файлы, то на странице образуется пустые места.
         * Эта функция их убирает. Заодно, при выключенной опции HomeworkNotifications
         * убираются пустые строки, предназначенные для удаленных предметов с домашкой,
         * которые на странице все равно есть при наличии этих предметов */

        var tables = document.getElementsByTagName("table");
        for (var b = 0; b < tables.length; b++) {
            var isTableEmpty = true;
            for (var m = 0; m < tables[b].rows.length; m++) {
                var isRowEmpty = checkIfCellsAreEmpty(tables[b].rows[m]);
                if (!isRowEmpty) isTableEmpty = false;
                else {
                    // здесь заодно убираются те самые пустые строки.
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

function setIconOfOptionsAsClickable() {
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

function show() {
    document.getElementById("content").style.display = "inline";
    document.getElementById("vkAuthorizer").style.display = "none";
    document.getElementById("infoVkAuthorization").style.display = "none";
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
    var host = window.location.origin + "/ServicePages/JSErrors";
    xhr.open("POST", host, true);
    xhr.send(error);
}

function SendToServerAboutUser(name, surname) {
    var xhr = new XMLHttpRequest();
    var host = window.location.origin;
    var requestBody = name + " " + surname + " " + host;
    xhr.open("POST", host, true);
    xhr.send(requestBody);
}

