document.addEventListener("DOMContentLoaded", () => {
    const button = document.getElementById("loadSlotsFromApi");
    const status = document.getElementById("apiStatus");
    const body = document.getElementById("apiSlotsBody");

    if (!button || !status || !body) {
        return;
    }

    button.addEventListener("click", async () => {
        button.disabled = true;
        status.textContent = "Loading parking slots from the REST API...";

        try {
            const response = await fetch("/api/slots", {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            const result = await response.json();

            if (!response.ok || !result.success) {
                throw new Error(
                    result.message || "The API request failed."
                );
            }

            body.innerHTML = "";

            if (!Array.isArray(result.data) || result.data.length === 0) {
                body.innerHTML = `
                    <tr>
                        <td colspan="4" class="pm-empty">
                            No parking slots were returned by the API.
                        </td>
                    </tr>`;
            } else {
                result.data.forEach(slot => {
                    const row = document.createElement("tr");

                    row.innerHTML = `
                        <td>${escapeHtml(slot.slotId)}</td>
                        <td>${escapeHtml(slot.slotNumber)}</td>
                        <td>${formatVehicleType(slot.slotType)}</td>
                        <td>${escapeHtml(slot.status)}</td>
                    `;

                    body.appendChild(row);
                });
            }

            status.textContent =
                `API integration successful — ${result.count} slot(s) loaded from /api/slots.`;
        } catch (error) {
            console.error(error);

            body.innerHTML = `
                <tr>
                    <td colspan="4" class="pm-empty">
                        Unable to load slot data from the API.
                    </td>
                </tr>`;

            status.textContent =
                error.message || "Unable to load slot data from the API.";
        } finally {
            button.disabled = false;
        }
    });

    function formatVehicleType(value) {
        if (value === "2_wheeler") {
            return "2 Wheeler";
        }

        if (value === "4_wheeler") {
            return "4 Wheeler";
        }

        return escapeHtml(value ?? "");
    }

    function escapeHtml(value) {
        const div = document.createElement("div");
        div.textContent = value ?? "";
        return div.innerHTML;
    }
});
