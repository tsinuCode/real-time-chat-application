window.ChatApp = window.ChatApp || {};

(function (app) {
    const MOBILE_BREAKPOINT = 992;

    app.isMobile = function () {
        return window.innerWidth < MOBILE_BREAKPOINT;
    };

    app.showListView = function () {
        const dashboard = document.getElementById("chatDashboard");
        if (!dashboard) return;
        dashboard.classList.remove("show-chat");
        dashboard.classList.add("show-list");
        document.dispatchEvent(new CustomEvent("chat:reset"));
    };

    app.showChatView = function () {
        if (!app.isMobile()) return;
        const dashboard = document.getElementById("chatDashboard");
        if (!dashboard) return;
        dashboard.classList.remove("show-list");
        dashboard.classList.add("show-chat");
    };

    document.addEventListener("DOMContentLoaded", function () {
        const backButton = document.getElementById("mobileBackButton");
        if (backButton) {
            backButton.addEventListener("click", function () {
                app.showListView();
            });
        }

        window.addEventListener("resize", function () {
            if (!app.isMobile()) {
                const dashboard = document.getElementById("chatDashboard");
                dashboard?.classList.remove("show-chat", "show-list");
            }
        });
    });
})(window.ChatApp);
