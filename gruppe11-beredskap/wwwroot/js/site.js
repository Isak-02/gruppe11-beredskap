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

            if (forStorVerdi(panel)) return true;

            return false;
        }

        // Finner første felt i steget med data-step-max der verdien er større enn maks. Gir null hvis alt er innenfor.
        // Brukes for lastekapasitet (maks 60 tonn) og radius (maks 50 000 meter).
        function forStorVerdi(panel) {
            var felter = panel.querySelectorAll("[data-step-max]");
            for (var i = 0; i < felter.length; i++) {
                var el = felter[i];
                if (el.value !== "" && Number(el.value) > Number(el.dataset.stepMax)) return el;
            }
            return null;
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

            var forStor = forStorVerdi(panel);
            if (forStor) {
                fail(forStor.dataset.stepMaxMessage || "Verdien er for stor.");
                if (forStor.type !== "hidden") forStor.focus();
                return false;
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
            if (e.target.closest && e.target.closest(".sokvalg")) return;
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

/* Nedtrekkslister som kan åpnes og søkes i. Selecten blir liggende, så skjemaet fortsatt sender verdien. */
(function () {
    var aapen = null;

    document.querySelectorAll("select.form-select").forEach(lagSokvalg);

    document.addEventListener("mousedown", function (e) {
        if (!aapen || aapen.contains(e.target)) return;
        aapen.lukk();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && aapen) aapen.lukk(true);
    });

    function lagSokvalg(select) {
        if (select.multiple || select.closest(".sokvalg")) return;

        var rot = document.createElement("div");
        rot.className = "sokvalg";
        select.parentNode.insertBefore(rot, select);
        rot.appendChild(select);
        select.classList.add("sokvalg-original");
        select.tabIndex = -1;

        var knapp = document.createElement("button");
        knapp.type = "button";
        knapp.className = "sokvalg-knapp";
        knapp.setAttribute("aria-haspopup", "listbox");
        knapp.setAttribute("aria-expanded", "false");

        var verdi = document.createElement("span");
        verdi.className = "sokvalg-verdi";

        var hint = document.createElement("span");
        hint.className = "sokvalg-hint";
        hint.appendChild(sokIkon());
        hint.appendChild(document.createTextNode("Søk"));

        var pil = document.createElement("span");
        pil.className = "sokvalg-pil";
        pil.setAttribute("aria-hidden", "true");
        pil.appendChild(pilIkon());

        knapp.appendChild(verdi);
        knapp.appendChild(hint);
        knapp.appendChild(pil);

        var panel = document.createElement("div");
        panel.className = "sokvalg-panel";
        panel.hidden = true;

        var sokFelt = document.createElement("label");
        sokFelt.className = "sokvalg-sok";
        sokFelt.appendChild(sokIkon());

        var sok = document.createElement("input");
        sok.type = "search";
        sok.placeholder = "Søk i listen";
        sok.autocomplete = "off";
        sok.setAttribute("aria-label", "Søk i listen");
        sokFelt.appendChild(sok);

        var liste = document.createElement("ul");
        liste.className = "sokvalg-liste";
        liste.setAttribute("role", "listbox");

        var tom = document.createElement("p");
        tom.className = "sokvalg-tom";
        tom.textContent = "Ingen treff";
        tom.hidden = true;

        panel.appendChild(sokFelt);
        panel.appendChild(liste);
        panel.appendChild(tom);
        rot.appendChild(knapp);
        rot.appendChild(panel);

        var label = select.id ? document.querySelector('label[for="' + CSS.escape(select.id) + '"]') : null;
        if (label) {
            if (!label.id) label.id = select.id + "-etikett";
            knapp.setAttribute("aria-labelledby", label.id);
            sok.setAttribute("aria-labelledby", label.id);
        }

        var aktiv = -1;

        function apne() {
            if (select.disabled) return;
            if (aapen && aapen !== api) aapen.lukk();
            panel.hidden = false;
            rot.classList.add("er-apen");
            knapp.setAttribute("aria-expanded", "true");
            sok.value = "";
            tegn("");
            aapen = api;
            sok.focus();
        }

        function lukk(fokusTilbake) {
            panel.hidden = true;
            rot.classList.remove("er-apen");
            knapp.setAttribute("aria-expanded", "false");
            if (aapen === api) aapen = null;
            if (fokusTilbake) knapp.focus();
        }

        function oppdaterVerdi() {
            var valgt = select.options[select.selectedIndex];
            var tomVerdi = !valgt || valgt.value === "";
            verdi.textContent = valgt ? valgt.text : "";
            verdi.classList.toggle("er-tom", tomVerdi);
            knapp.disabled = select.disabled;
        }

        function synlige() {
            return Array.prototype.filter.call(liste.children, function (rad) {
                return !rad.hidden;
            });
        }

        function markerAktiv(neste, rull) {
            var rader = synlige();
            if (!rader.length) {
                aktiv = -1;
                return;
            }
            if (neste < 0) neste = rader.length - 1;
            if (neste >= rader.length) neste = 0;
            aktiv = neste;
            rader.forEach(function (rad, i) {
                rad.classList.toggle("is-aktiv", i === aktiv);
            });
            var boks = rader[aktiv].getBoundingClientRect();
            var ramme = liste.getBoundingClientRect();
            if (boks.top < ramme.top) liste.scrollTop -= ramme.top - boks.top;
            else if (boks.bottom > ramme.bottom) liste.scrollTop += boks.bottom - ramme.bottom;
        }

        function tegn(q) {
            var sokeord = q.trim().toLocaleLowerCase("nb");
            liste.replaceChildren();
            Array.prototype.forEach.call(select.options, function (opt) {
                if (opt.disabled) return;
                if (sokeord && opt.text.toLocaleLowerCase("nb").indexOf(sokeord) === -1) return;

                var rad = document.createElement("li");
                var valg = document.createElement("button");
                valg.type = "button";
                valg.className = "sokvalg-valg";
                valg.setAttribute("role", "option");
                valg.setAttribute("aria-selected", opt.value === select.value ? "true" : "false");
                valg.textContent = opt.text;
                valg.addEventListener("mousedown", function (e) {
                    e.preventDefault();
                });
                valg.addEventListener("click", function () {
                    select.value = opt.value;
                    select.dispatchEvent(new Event("change", { bubbles: true }));
                    oppdaterVerdi();
                    lukk(true);
                });
                rad.appendChild(valg);
                liste.appendChild(rad);
            });
            tom.hidden = liste.children.length > 0;
            var valgtRad = Array.prototype.findIndex.call(liste.children, function (rad) {
                return rad.firstChild.getAttribute("aria-selected") === "true";
            });
            markerAktiv(valgtRad >= 0 ? valgtRad : 0, false);
        }

        knapp.addEventListener("click", function () {
            if (panel.hidden) apne();
            else lukk(true);
        });

        knapp.addEventListener("keydown", function (e) {
            if (e.key === "ArrowDown" || e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                e.stopPropagation();
                apne();
                return;
            }
            if (e.key.length === 1 && !e.metaKey && !e.ctrlKey && !e.altKey) {
                e.preventDefault();
                e.stopPropagation();
                apne();
                sok.value = e.key;
                tegn(e.key);
            }
        });

        sok.addEventListener("input", function () {
            tegn(sok.value);
        });

        sok.addEventListener("keydown", function (e) {
            var rader = synlige();
            if (e.key === "ArrowDown") {
                e.preventDefault();
                markerAktiv(aktiv + 1, true);
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                markerAktiv(aktiv - 1, true);
            } else if (e.key === "Enter") {
                e.preventDefault();
                e.stopPropagation();
                if (rader[aktiv]) rader[aktiv].firstChild.click();
            } else if (e.key === "Escape") {
                e.stopPropagation();
                lukk(true);
            }
        });

        select.addEventListener("focus", function () {
            knapp.focus();
            if (panel.hidden) apne();
        });

        select.addEventListener("change", oppdaterVerdi);

        var observer = new MutationObserver(function () {
            oppdaterVerdi();
            if (!panel.hidden) tegn(sok.value);
        });
        observer.observe(select, { childList: true });

        var api = { contains: function (node) { return rot.contains(node); }, lukk: lukk };
        oppdaterVerdi();
    }

    function sokIkon() {
        var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svg.setAttribute("viewBox", "0 0 16 16");
        svg.setAttribute("aria-hidden", "true");
        svg.innerHTML = '<circle cx="7" cy="7" r="4.5" fill="none" stroke="currentColor" stroke-width="1.6"></circle><path d="M10.5 10.5 L14 14" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path>';
        return svg;
    }

    function pilIkon() {
        var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svg.setAttribute("viewBox", "0 0 16 16");
        svg.setAttribute("aria-hidden", "true");
        svg.innerHTML = '<path d="M3 6 L8 11 L13 6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"></path>';
        return svg;
    }
})();

/* Klikk på kolonnetittelen: først høy til lav (pil ned), så lav til høy (pil opp), så tilbake til opprinnelig rekkefølge. */
(function () {
    var prioritet = { "akutt": 4, "høy": 3, "middels": 2, "lav": 1 };

    document.querySelectorAll("table").forEach(initTabellSort);

    function initTabellSort(table) {
        var hode = table.tHead && table.tHead.rows[0];
        var kropp = table.tBodies[0];
        if (!hode || !kropp || !kropp.rows.length) return;

        var rader = Array.prototype.map.call(kropp.rows, function (rad, i) {
            rad.dataset.sortOpprinnelig = String(i);
            return rad;
        });

        Array.prototype.forEach.call(hode.cells, function (th, indeks) {
            if (!th.textContent.trim()) return;

            var knapp = document.createElement("button");
            knapp.type = "button";
            knapp.className = "tabell-sort";
            knapp.textContent = th.textContent.trim();

            var pil = document.createElement("span");
            pil.className = "tabell-sort-pil";
            pil.setAttribute("aria-hidden", "true");
            knapp.appendChild(pil);

            th.textContent = "";
            th.appendChild(knapp);
            th.setAttribute("aria-sort", "none");

            knapp.addEventListener("click", function () {
                var neste = nesteRetning(th.getAttribute("aria-sort"));
                Array.prototype.forEach.call(hode.cells, function (annen) {
                    annen.classList.remove("er-synkende", "er-stigende");
                    if (annen.querySelector(".tabell-sort")) annen.setAttribute("aria-sort", "none");
                });

                if (!neste) {
                    rader.sort(function (a, b) {
                        return Number(a.dataset.sortOpprinnelig) - Number(b.dataset.sortOpprinnelig);
                    });
                } else {
                    th.classList.add(neste === "descending" ? "er-synkende" : "er-stigende");
                    th.setAttribute("aria-sort", neste);
                    var retning = neste === "descending" ? "desc" : "asc";
                    rader.sort(function (a, b) {
                        return sammenlign(a, b, indeks, retning);
                    });
                }

                rader.forEach(function (rad) {
                    kropp.appendChild(rad);
                });
            });
        });
    }

    function nesteRetning(na) {
        if (na === "none") return "descending";
        if (na === "descending") return "ascending";
        return null;
    }

    function sammenlign(a, b, indeks, retning) {
        var va = lesVerdi(celleTekst(a, indeks));
        var vb = lesVerdi(celleTekst(b, indeks));
        var opprinnelig = Number(a.dataset.sortOpprinnelig) - Number(b.dataset.sortOpprinnelig);
        if (va.tom && vb.tom) return opprinnelig;
        if (va.tom) return 1;
        if (vb.tom) return -1;

        var diff = va.type === "tall" && vb.type === "tall"
            ? va.n - vb.n
            : va.tekst.localeCompare(vb.tekst, "nb", { numeric: true, sensitivity: "base" });
        if (diff === 0) return opprinnelig;
        return retning === "desc" ? -diff : diff;
    }

    function celleTekst(rad, indeks) {
        var celle = rad.cells[indeks];
        return celle ? celle.textContent : "";
    }

    function lesVerdi(tekst) {
        var ren = tekst.replace(/\s+/g, " ").trim();
        var lav = ren.toLocaleLowerCase("nb");
        if (ren === "" || ren === "-" || ren === "–") {
            return { tom: true, type: "tekst", tekst: "" };
        }

        if (Object.prototype.hasOwnProperty.call(prioritet, lav)) {
            return { tom: false, type: "tall", n: prioritet[lav], tekst: lav };
        }

        var tid = lesTidspunkt(ren);
        if (tid !== null) return { tom: false, type: "tall", n: tid, tekst: lav };

        var tall = ren.match(/^(-?\d+(?:[.,]\d+)?)(?:\s*(?:tonn|t))?$/i);
        if (tall) {
            return { tom: false, type: "tall", n: parseFloat(tall[1].replace(",", ".")), tekst: lav };
        }

        return { tom: false, type: "tekst", tekst: lav };
    }

    function lesTidspunkt(tekst) {
        var us = tekst.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})(?:\s+(\d{1,2}):(\d{2})(?::(\d{2}))?\s*(AM|PM))?$/i);
        if (us) {
            var time = +(us[4] || 0);
            if (us[7]) {
                var ap = us[7].toUpperCase();
                if (ap === "PM" && time < 12) time += 12;
                if (ap === "AM" && time === 12) time = 0;
            }
            return tidsstempel(+us[3], +us[1] - 1, +us[2], time, +(us[5] || 0), +(us[6] || 0));
        }

        var no = tekst.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})(?:\s+(?:kl\.\s*)?(\d{1,2}):(\d{2}))?$/);
        if (no) {
            return tidsstempel(+no[3], +no[2] - 1, +no[1], +(no[4] || 0), +(no[5] || 0), 0);
        }

        return null;
    }

    function tidsstempel(ar, mnd, dag, time, min, sek) {
        var dato = new Date(ar, mnd, dag, time, min, sek);
        return isNaN(dato.getTime()) ? null : dato.getTime();
    }
})();
