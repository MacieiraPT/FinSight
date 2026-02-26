(function () {
    const root = document.documentElement;
    const body = document.body;

    // Theme (light/dark) with localStorage
    const themeToggle = document.getElementById("themeToggle");
    const savedTheme = localStorage.getItem("theme");
    if (savedTheme === "light" || savedTheme === "dark") {
        root.setAttribute("data-theme", savedTheme);
    }

    function updateThemeIcon() {
        const theme = root.getAttribute("data-theme") || "light";
        if (themeToggle) themeToggle.textContent = theme === "dark" ? "☀️" : "🌙";
    }
    updateThemeIcon();

    if (themeToggle) {
        themeToggle.addEventListener("click", () => {
            const current = root.getAttribute("data-theme") || "light";
            const next = current === "dark" ? "light" : "dark";
            root.setAttribute("data-theme", next);
            localStorage.setItem("theme", next);
            updateThemeIcon();

            window.gdUpdateCharts();
        });
    }

    // Sidebar (mobile)
    const btnOpen = document.getElementById("btnSidebarOpen");
    const btnClose = document.getElementById("btnSidebarClose");

    function closeSidebar() {
        body.classList.remove("sidebar-open");
    }
    function openSidebar() {
        body.classList.add("sidebar-open");
    }

    if (btnOpen) btnOpen.addEventListener("click", openSidebar);
    if (btnClose) btnClose.addEventListener("click", closeSidebar);

    // Clicking overlay closes sidebar
    document.addEventListener("click", (e) => {
        if (!body.classList.contains("sidebar-open")) return;
        const sidebar = document.querySelector(".app-sidebar");
        const isClickInside = sidebar && sidebar.contains(e.target);
        const isOpenBtn = btnOpen && btnOpen.contains(e.target);
        if (!isClickInside && !isOpenBtn) closeSidebar();
    });

    // Tiny toast helper (Bootstrap toast)
    window.gdToast = function (message, type) {

        const host = document.getElementById("toastHost");
        if (!host || !window.bootstrap) return;

        const bg = type === "success" ? "text-bg-success"
            : type === "warning" ? "text-bg-warning"
                : type === "danger" ? "text-bg-danger"
                    : "text-bg-secondary";

        const el = document.createElement("div");
        el.className = `toast align-items-center ${bg} border-0`;
        el.role = "status";
        el.ariaLive = "polite";
        el.ariaAtomic = "true";

        // Glow apenas para danger
        if (type === "danger") {
            el.style.boxShadow = `
            0 0 0 1px rgba(255, 0, 0, 0.7),
            0 0 8px rgba(255, 0, 0, 0.6),
            0 0 18px rgba(255, 0, 0, 0.4)
        `;
        }

        el.innerHTML = `
      <div class="d-flex">
        <div class="toast-body fw-semibold">${message}</div>
        <button type="button"
                class="btn-close btn-close-white me-2 m-auto"
                data-bs-dismiss="toast"
                aria-label="Close">
        </button>
      </div>
    `;

        host.appendChild(el);

        const toast = new bootstrap.Toast(el, { delay: 4000 });
        toast.show();

        el.addEventListener("hidden.bs.toast", () => el.remove());
    };

    window.gdGetChartColors = function () {

        const theme = document.documentElement.getAttribute("data-theme");

        if (theme === "dark") {
            return {
                text: "#e5e7eb",
                grid: "rgba(255,255,255,0.08)",
                tooltipBg: "#1f2937"
            };
        }

        return {
            text: "#374151",
            grid: "rgba(0,0,0,0.08)",
            tooltipBg: "#ffffff"
        };
    };


    // Sempre que muda o tema → atualizar gráficos
    window.gdUpdateCharts = function () {

        if (!window.Chart) return;

        const colors = window.gdGetChartColors();

        Chart.defaults.color = colors.text;
        Chart.defaults.borderColor = colors.grid;

        Chart.defaults.plugins.legend.labels.color = colors.text;

        Chart.defaults.plugins.tooltip.backgroundColor = colors.tooltipBg;
        Chart.defaults.plugins.tooltip.titleColor = colors.text;
        Chart.defaults.plugins.tooltip.bodyColor = colors.text;
        Chart.defaults.plugins.tooltip.borderColor = colors.grid;
        Chart.defaults.plugins.tooltip.borderWidth = 1;

        Chart.instances.forEach(c => c.update());
    };

    // Spinner nos botões submit
    document.addEventListener("submit", function (e) {

        const form = e.target;
        const btn = form.querySelector("button[type=submit]");

        if (!btn) return;
        if (btn.classList.contains("btn-loading")) return;

        const originalText = btn.innerHTML;

        btn.classList.add("btn-loading");
        btn.dataset.originalText = originalText;

        btn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2"
              role="status"
              aria-hidden="true"></span>
        Aguarda...
    `;
    });

    document.addEventListener("DOMContentLoaded", function () {

        const hasErrors =
            document.querySelector(".validation-summary-errors") ||
            document.querySelector(".field-validation-error");

        if (!hasErrors) return;

        const loadingBtns = document.querySelectorAll(".btn-loading");

        loadingBtns.forEach(btn => {
            btn.disabled = false;
            btn.classList.remove("btn-loading");

            if (btn.dataset.originalText) {
                btn.innerHTML = btn.dataset.originalText;
            }
        });
    });

})();
