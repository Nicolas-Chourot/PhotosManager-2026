function StartNotificationsHandler() {
    //alert("Notifications Handler installed");
    Notification.requestPermission().then((permission) => {
        console.log("Global Panel Refresh Rate :", GPRR, " seconds");

        setInterval(function () {
            $.ajax({
                url: "/Notifications/Pop",
                success: message => {
                    if (message != null) {
                        var icon = "/Content/UI-Images/PhotoCloudLogo.png";
                        var title = "PhotosManager";
                        var body = message;
                        if (permission === "granted")
                            new Notification(title, { body, icon });
                    }
                }
            })
        }, GPRR * 1000);
    });
}