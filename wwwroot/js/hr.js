// ------------------------------
// HR Dashboard JavaScript
// ------------------------------

document.addEventListener('DOMContentLoaded', function () {

    const tableBody = document.getElementById('claimsTable');
    const btnSearch = document.getElementById('btnSearch');
    const btnReset = document.getElementById('btnReset');
    const btnExportBatch = document.getElementById('btnExportBatch');

    const filterDepartment = document.getElementById('filterDepartment');
    const filterStatus = document.getElementById('filterStatus');
    const filterMonth = document.getElementById('filterMonth');
    const filterLecturer = document.getElementById('filterLecturer');

    const invoiceBody = document.getElementById('invoiceBody');
    let invoiceModal = new bootstrap.Modal(document.getElementById('invoiceModal'));


    // -----------------------------
    // Load Claims from Server
    // -----------------------------
    async function loadClaims() {

        tableBody.innerHTML = `
            <tr>
                <td colspan="9" class="text-center text-muted">Loading...</td>
            </tr>`;

        const query = new URLSearchParams({
            departmentId: filterDepartment.value || "",
            status: filterStatus.value || "",
            month: filterMonth.value || "",
            lecturer: filterLecturer.value || ""
        });

        try {
            const res = await fetch('/HR/GetFilteredClaims?' + query.toString());
            if (!res.ok) throw new Error("Failed to fetch claims");

            const payload = await res.json();
            if (!payload.success) {
                tableBody.innerHTML = `
                    <tr><td colspan="9" class="text-center text-danger">Failed to load data.</td></tr>`;
                return;
            }

            const claims = payload.data;

            if (!claims || claims.length === 0) {
                tableBody.innerHTML = `
                    <tr><td colspan="9" class="text-center text-muted">No claims found for selected filters.</td></tr>`;
                return;
            }

            let html = "";
            claims.forEach(c => {

                const docCell = c.documentID
                    ? `<a href="/Dashboard/DownloadDocument/${c.documentID}" target="_blank" class="btn btn-sm btn-outline-info">View</a>`
                    : `<span class="text-muted">None</span>`;

                html += `
                    <tr>
                        <td>${c.claimID}</td>
                        <td>${escapeHtml(c.lecturerName)}</td>
                        <td>${escapeHtml(c.departmentName)}</td>
                        <td>${c.claimDate}</td>
                        <td>${c.hoursWorked}</td>
                        <td>${formatCurrency(c.totalAmount)}</td>
                        <td><span class="badge ${badgeClass(c.status)}">${escapeHtml(c.status)}</span></td>
                        <td>${docCell}</td>
                        <td>
                            <button class="btn btn-sm btn-info view-invoice"
                                data-claim-id="${c.claimID}">
                                Invoice
                            </button>
                        </td>
                    </tr>`;
            });

            tableBody.innerHTML = html;

            // reattach invoice handlers
            attachInvoiceButtons();

        } catch (err) {
            console.error(err);
            tableBody.innerHTML = `
                <tr><td colspan="9" class="text-center text-danger">Server error loading claims.</td></tr>`;
        }
    }


    // ---------------------------------
    // Invoice Modal Loader
    // ---------------------------------
    function attachInvoiceButtons() {
        document.querySelectorAll('.view-invoice').forEach(btn => {
            btn.onclick = async function () {
                const id = btn.getAttribute('data-claim-id');

                invoiceBody.innerHTML = `<p class="text-center text-muted">Loading invoice...</p>`;

                try {
                    const res = await fetch('/HR/GetInvoice/' + id);
                    const payload = await res.json();

                    if (!payload.success) {
                        invoiceBody.innerHTML = `<p class="text-danger text-center">Failed to load invoice</p>`;
                        return;
                    }

                    const inv = payload.data;

                    invoiceBody.innerHTML = `
                        <div class="p-3">
                            <h4 class="text-info mb-3">Invoice #${inv.claimID}</h4>

                            <p><strong>Lecturer:</strong> ${escapeHtml(inv.lecturerName)}</p>
                            <p><strong>Department:</strong> ${escapeHtml(inv.departmentName)}</p>
                            <p><strong>Date Submitted:</strong> ${inv.claimDate}</p>
                            <p><strong>Hours Worked:</strong> ${inv.hoursWorked}</p>
                            <p><strong>Hourly Rate:</strong> ${formatCurrency(inv.hourlyRate)}</p>
                            <p><strong>Total Amount:</strong> ${formatCurrency(inv.totalAmount)}</p>

                            <hr/>

                            <p><strong>Supporting Document:</strong><br>
                                ${inv.documentID
                            ? `<a href="/Dashboard/DownloadDocument/${inv.documentID}"
                                          class="btn btn-outline-info btn-sm" target="_blank">View</a>`
                            : `<span class="text-muted">None</span>`}
                            </p>
                        </div>
                    `;

                    // show modal
                    invoiceModal.show();

                    // set PDF download link
                    document.getElementById('btnDownloadInvoice').onclick = function () {
                        window.location.href = '/HR/DownloadInvoicePDF/' + id;
                    };

                } catch (err) {
                    invoiceBody.innerHTML = `<p class="text-danger text-center">Error loading invoice</p>`;
                }
            };
        });
    }


    // ---------------------------------
    // Helper Functions
    // ---------------------------------
    function escapeHtml(text) {
        if (!text) return "";
        return text.replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('en-ZA', {
            style: 'currency',
            currency: 'ZAR'
        }).format(amount);
    }

    function badgeClass(status) {
        if (!status) return "bg-secondary";
        const s = status.toLowerCase();
        if (s === "pending") return "bg-warning";
        if (s === "verified") return "bg-info";
        if (s === "approved") return "bg-success";
        if (s === "rejected") return "bg-danger";
        return "bg-secondary";
    }


    // ---------------------------------
    // Search / Reset / Batch Export
    // ---------------------------------

    btnSearch.addEventListener('click', loadClaims);

    btnReset.addEventListener('click', () => {
        filterDepartment.value = "";
        filterStatus.value = "";
        filterMonth.value = "";
        filterLecturer.value = "";
        loadClaims();
    });

    btnExportBatch.addEventListener('click', () => {
        const query = new URLSearchParams({
            departmentId: filterDepartment.value || "",
            status: filterStatus.value || "",
            month: filterMonth.value || "",
            lecturer: filterLecturer.value || ""
        });

        window.location.href = '/HR/ExportBatchPDF?' + query.toString();
    });


    // Auto-load when page starts
    tableBody.innerHTML = `
        <tr><td colspan="9" class="text-center text-muted">Use filters to view claims.</td></tr>
    `;

});
