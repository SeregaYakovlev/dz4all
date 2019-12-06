document.getElementById("infoVkAuthorization").style.display = "inline";
checkAuthorization();

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
}

function GetUser(id) {
    VK.api("users.get", { 'user_ids': id, 'v': "5.95" }, function (data) {
        var name = data.response[0].first_name;
        var surname = data.response[0].last_name;
        SendToServerAboutUser(name, surname);
    });
}

function show() {
    document.getElementById("content").style.display = "inline";
    document.getElementById("vkAuthorizer").style.display = "none";
    document.getElementById("infoVkAuthorization").style.display = "none";
    document.getElementById("siteInfo").style.display = "none";
}

function SendToServerAboutUser(name, surname) {
    var xhr = new XMLHttpRequest();
    var host = window.location.origin + "/Shared" + "/UserReceiver";
    var requestBody = name + " " + surname;
    xhr.open("POST", host, true);
    xhr.send(requestBody);
}