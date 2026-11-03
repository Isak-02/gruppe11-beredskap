(function () {
    document.querySelectorAll("[data-wizard]").forEach(initWizard);

    function initWizard(form) {
        var panels = Array.prototype.slice.call(form.querySelectorAll("[data-wizard-panel]"));
        if (!panels.length) return;

        var progress = form.querySelector("[data-wizard-progress]");
        var status = form.querySelector("[data-wizard-status]");
        var error = form.querySelector("[data-wizard-error]");
        var backBtn = form.querySelector("[data-wizard-back]");
        var nextBtn = form.querySelector("[data-wizard-next]");
        var submitBtn = form.querySelector("[data-wizard-submit]");
        var harForsoktSende = false;
        var index = panels.findIndex(function (panel) {
            return panel.querySelector(".field-validation-error");
        });
        if (index < 0) index = 0;

        var crumbs = panels.map(function (panel, i) {
            var button = document.createElement("button");
            button.type = "button";
            button.textContent = (i + 1) + ". " + (panel.dataset.wizardLabel || "");
            button.addEventListener("click", function () {
                if (panels[i].hidden) return;
                if (i !== index) show(i, true);
            });
            progress.appendChild(button);
            return button;
        });

        function nesteSynlige(fra, steg) {
            var i = fra + steg;
            while (i >= 0 && i < panels.length) {
                if (!panels[i].hidden) return i;
                i += steg;
            }
            return -1;
        }

        function sisteSynligeIndex() {
            for (var i = panels.length - 1; i >= 0; i--) {
                if (!panels[i].hidden) return i;
            }
            return 0;
        }

        function show(i, moveFocus) {
            if (panels[i].hidden) {
                var hopp = nesteSynlige(i, 1);
                if (hopp < 0) hopp = nesteSynlige(i, -1);
                if (hopp < 0) return;
                i = hopp;
            }
            index = i;
            panels.forEach(function (panel, n) {
                panel.classList.toggle("is-active", n === index);
            });
            oppdaterSteg();
            backBtn.hidden = nesteSynlige(index, -1) < 0;
            var last = index === sisteSynligeIndex();
            nextBtn.hidden = last;
            submitBtn.hidden = !last;
            if (!harForsoktSende && error) error.hidden = true;

            var panel = panels[index];
            if (panel.dataset.wizardSummary === "true") fyllOversikt();
            panel.dispatchEvent(new CustomEvent("wizard:shown", { bubbles: true }));

            if (!moveFocus) return;
            var focusable = panel.querySelector("input:not([type=hidden]), select, textarea");
            if (focusable) focusable.focus();
        }

        function fail(message) {
            if (!error) return;
            error.hidden = false;
            error.textContent = message;
        }

        function manglerIPanel(panel) {
            if (panel.hidden || panel.dataset.wizardSummary === "true") return false;

            var requiredBits = panel.querySelectorAll("[data-step-required]");
            for (var i = 0; i < requiredBits.length; i++) {
                var el = requiredBits[i];
                var emptyNumber = el.type === "number" && (el.value === "" || Number(el.value) <= 0);
                var emptyText = el.type !== "number" && el.value.trim() === "";
                if (emptyNumber || emptyText) return true;
            }

            if (panel.dataset.wizardMap === "true") {
                var lat = panel.querySelector("[data-wizard-lat]");
                if (!lat || lat.value === "") return true;
            }

            return false;
        }

        function erFullfort(panel) {
            if (panel.dataset.wizardSummary === "true") {
                return panels.every(function (annen) {
                    return annen === panel || !manglerIPanel(annen);
                });
            }
            return !manglerIPanel(panel);
        }

        function manglerNoen() {
            return panels.some(manglerIPanel);
        }

        function oppdaterSteg() {
            var antall = 0;
            var gjeldende = 0;
            var nr = 0;
            panels.forEach(function (panel) {
                if (!panel.hidden) antall++;
            });
            crumbs.forEach(function (button, n) {
                var skjult = panels[n].hidden;
                button.hidden = skjult;
                if (!skjult) {
                    nr++;
                    button.textContent = nr + ". " + (panels[n].dataset.wizardLabel || "");
                    if (n === index) gjeldende = nr;
                }
                var ferdig = erFullfort(panels[n]);
                var mangler = !skjult && panels[n].dataset.wizardSummary !== "true" && !ferdig;
                button.classList.toggle("is-current", n === index);
                button.classList.toggle("is-done", ferdig && n !== index);
                button.classList.toggle("is-missing", harForsoktSende && mangler);
            });
            if (status) {
                status.textContent = "Steg " + gjeldende + " av " + antall;
            }
            if (harForsoktSende && error) {
                if (manglerNoen()) {
                    error.hidden = false;
                    error.textContent = "Du må fylle ut alle punktene";
                } else {
                    error.hidden = true;
                }
            }
        }

        function validate(panel) {
            var requiredBits = panel.querySelectorAll("[data-step-required]");
            for (var i = 0; i < requiredBits.length; i++) {
                var el = requiredBits[i];
                var emptyNumber = el.type === "number" && (el.value === "" || Number(el.value) <= 0);
                var emptyText = el.type !== "number" && el.value.trim() === "";
                if (emptyNumber || emptyText) {
                    fail(el.dataset.stepMessage || "Feltet må fylles ut.");
                    el.focus();
                    return false;
                }
            }

            if (panel.dataset.wizardMap === "true") {
                var lat = panel.querySelector("[data-wizard-lat]");
                if (!lat || lat.value === "") {
                    fail(panel.dataset.stepMessage || "Tegn en markør eller sirkel på kartet for å velge sted.");
                    return false;
                }
            }

            if (error) error.hidden = true;
            return true;
        }

        backBtn.addEventListener("click", function () {
            var forrige = nesteSynlige(index, -1);
            if (forrige >= 0) show(forrige, true);
        });

        nextBtn.addEventListener("click", function () {
            if (!validate(panels[index])) return;
            var neste = nesteSynlige(index, 1);
            if (neste >= 0) show(neste, true);
        });

        form.addEventListener("wizard:refresh", function () {
            if (panels[index].hidden) {
                var hopp = nesteSynlige(index, 1);
                if (hopp < 0) hopp = nesteSynlige(index, -1);
                if (hopp >= 0) show(hopp, false);
                return;
            }
            oppdaterSteg();
            backBtn.hidden = nesteSynlige(index, -1) < 0;
            var last = index === sisteSynligeIndex();
            nextBtn.hidden = last;
            submitBtn.hidden = !last;
        });

        form.addEventListener("input", function () {
            if (!harForsoktSende && error) error.hidden = true;
            oppdaterSteg();
        });
        form.addEventListener("change", function () {
            if (!harForsoktSende && error) error.hidden = true;
            oppdaterSteg();
        });

        form.addEventListener("keydown", function (e) {
            if (e.key !== "Enter" || e.target.tagName === "TEXTAREA") return;
            if (nesteSynlige(index, 1) < 0) return;
            e.preventDefault();
            nextBtn.click();
        });

        form.addEventListener("submit", function (e) {
            harForsoktSende = true;
            if (manglerNoen()) {
                e.preventDefault();
                var forste = panels.findIndex(manglerIPanel);
                if (forste >= 0) show(forste, true);
                if (panels[index].dataset.wizardSummary === "true") fyllOversikt();
                if (error) {
                    error.hidden = false;
                    error.textContent = "Du må fylle ut alle punktene";
                }
            }
        });

        function formatTid(value) {
            if (!value) return "";
            var date = new Date(value);
            if (isNaN(date.getTime())) return value;
            var dag = String(date.getDate()).padStart(2, "0");
            var mnd = String(date.getMonth() + 1).padStart(2, "0");
            var tid = String(date.getHours()).padStart(2, "0") + ":" + String(date.getMinutes()).padStart(2, "0");
            return dag + "." + mnd + "." + date.getFullYear() + " kl. " + tid;
        }

        function lesVerdi(el) {
            if (el.type === "datetime-local") return formatTid(el.value);
            return el.value.trim();
        }

        function oversiktsrad(label, value, panelIndex) {
            var row = document.createElement("div");
            row.className = "wizard-summary-row";
            if (harForsoktSende && (value === "Ikke fylt ut" || value === "Ikke valgt")) {
                row.classList.add("is-missing");
            }
            var dt = document.createElement("dt");
            dt.textContent = label;
            var dd = document.createElement("dd");
            dd.textContent = value;
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-outline-primary wizard-summary-edit";
            btn.textContent = "Endre";
            btn.addEventListener("click", function () {
                show(panelIndex, true);
            });
            row.appendChild(dt);
            row.appendChild(dd);
            row.appendChild(btn);
            return row;
        }

        function fyllOversikt() {
            var list = form.querySelector("[data-wizard-summary-list]");
            if (!list) return;
            list.replaceChildren();
            panels.forEach(function (panel, panelIndex) {
                if (panel.hidden || panel.dataset.wizardSummary === "true") return;
                if (panel.dataset.wizardMap === "true") {
                    var tekst = panel.querySelector("#valgtStedTekst");
                    var sted = tekst && tekst.classList.contains("text-success")
                        ? tekst.textContent.replace("Valgt posisjon: ", "")
                        : "Ikke valgt";
                    list.appendChild(oversiktsrad("Sted", sted, panelIndex));
                    return;
                }
                panel.querySelectorAll("[data-summary]").forEach(function (el) {
                    var verdi = lesVerdi(el);
                    list.appendChild(oversiktsrad(el.dataset.summary, verdi || "Ikke fylt ut", panelIndex));
                });
            });
        }

        show(index, false);
    }
})();
