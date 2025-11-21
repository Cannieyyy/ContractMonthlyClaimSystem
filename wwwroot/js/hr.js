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


document.addEventListener("DOMContentLoaded", () => {
    console.log("HR JS Loaded");

    const empBtn = document.querySelector(".employee-overview-btn");
    const accBtn = document.querySelector(".manage-users-card");

    const empSec = document.getElementById("employeeOverviewSection");
    const accSec = document.getElementById("manageAccountsSection");

    function closeAll() {
        if (empSec) empSec.classList.remove("show");
        if (accSec) accSec.classList.remove("show");
    }

    if (empBtn) {
        empBtn.addEventListener("click", () => {
            console.log("Employee Overview CLICKED");
            closeAll();
            empSec.classList.add("show");
        });
    }

    

    if (accBtn) {
        accBtn.addEventListener("click", () => {
            console.log("Manage Users CLICKED");
            closeAll();
            accSec.classList.add("show");
        });
    }

    document.querySelectorAll(".close-section").forEach(btn => {
        btn.addEventListener("click", () => {
            closeAll();
            console.log("Section closed");
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const repBtn = document.querySelector('.reports-btn');
    const reportsSection = document.getElementById('reportsSection');
    const tableBody = document.getElementById('reportsTable');
    const btnSearch = document.getElementById('btnReportSearch');
    const btnReset = document.getElementById('btnReportReset');

    if (repBtn && reportsSection) {
        repBtn.addEventListener('click', () => {
            // Toggle display instead of class
            if (reportsSection.style.display === 'none' || !reportsSection.style.display) {
                reportsSection.style.display = 'block';
            } else {
                reportsSection.style.display = 'none';
            }
        });
    }

    // Close button
    reportsSection.querySelectorAll('.close-section').forEach(btn =>
        btn.addEventListener('click', () => reportsSection.style.display = 'none')
    );

    async function loadReports() {
        if (!tableBody) return;

        const dept = document.getElementById('reportFilterDepartment')?.value || null;
        const lecturer = document.getElementById('reportFilterLecturer')?.value || null;
        const monthFrom = document.getElementById('reportFilterMonthFrom')?.value || null;
        const monthTo = document.getElementById('reportFilterMonthTo')?.value || null;

        


        tableBody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Loading...</td></tr>`;

        try {
            // Build query string with only values that exist
            const params = new URLSearchParams();
            if (dept) params.append('departmentId', dept);
            if (lecturer) params.append('lecturer', lecturer);
            if (monthFrom) params.append('monthFrom', monthFrom);
            if (monthTo) params.append('monthTo', monthTo);

            const res = await fetch('/HR/GetReports?' + params.toString());
            const payload = await res.json();

            if (!payload.success || !payload.data || payload.data.length === 0) {
                tableBody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">No data found.</td></tr>`;
                return;
            } else {

                let totalHours = 0;
                let totalAmount = 0;

                // Build table rows and sum totals
                const rowsHtml = payload.data.map(r => {
                    totalHours += Number(r.totalHours ?? 0);
                    totalAmount += Number(r.totalAmount ?? 0);
                    return `
        <tr>
            <td>${r.department ?? ''}</td>
            <td>${r.lecturer ?? ''}</td>
            <td>${r.month ?? ''}</td>
            <td>${r.totalHours ?? 0}</td>
            <td>${(r.totalAmount ?? 0).toLocaleString('en-ZA', { style: 'currency', currency: 'ZAR' })}</td>
        </tr>
    `;
                }).join('');

                // Add subtotal row
                const subtotalRow = `
    <tr class="table-secondary fw-bold">
        <td colspan="3" class="text-end">Subtotal:</td>
        <td>${totalHours}</td>
        <td>${totalAmount.toLocaleString('en-ZA', { style: 'currency', currency: 'ZAR' })}</td>
    </tr>
`;

                // Output the final table
                tableBody.innerHTML = rowsHtml + subtotalRow;

            }

        } catch (err) {
            console.error(err);
            tableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">Error loading data</td></tr>`;
        }
    }

    if (btnSearch) btnSearch.addEventListener('click', loadReports);

    if (btnReset) btnReset.addEventListener('click', () => {
        document.getElementById('reportFilterDepartment').value = "";
        document.getElementById('reportFilterLecturer').value = "";
        document.getElementById('reportFilterMonth').value = "";
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Use filters to generate report</td></tr>`;
    });
});


document.addEventListener('DOMContentLoaded', () => {

    // Close Section
    document.querySelectorAll('.close-section').forEach(btn => {
        btn.addEventListener('click', () => {
            document.getElementById('manageAccountsSection').classList.remove('show');
        });
    });

    
    });

    


document.addEventListener("DOMContentLoaded", () => {

    const manageBtn = document.querySelector(".manage-accounts-btn");
    const manageSec = document.getElementById("manageAccountsSection");

    function closeAll() {
        manageSec.style.display = "none";
    }

    // Show section when button is clicked
    if (manageBtn) {
        manageBtn.addEventListener("click", () => {
            manageSec.style.display = "block";
        });
    }

    // Close button
    manageSec.querySelectorAll(".close-section").forEach(btn => {
        btn.addEventListener("click", () => {
            closeAll();
        });
    });

    

});

// Department change
document.querySelectorAll('#manageAccountsSection select.department-select')
    .forEach(sel => {
        sel.addEventListener('change', async function () {
            const row = this.closest('tr');
            const employeeId = row.getAttribute('data-employee-id');
            const departmentId = this.value;

            const proceed = await confirmChange("Are you sure you want to change the department?", "danger");
            if (!proceed) return;

            await fetch('/HR/UpdateDepartment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `employeeId=${employeeId}&departmentId=${departmentId}`
            });

            showToast('Department updated successfully!', 'success');
        });
    });

// Role change
document.querySelectorAll('#manageAccountsSection select.role-select')
    .forEach(sel => {
        sel.addEventListener('change', async function () {
            const row = this.closest('tr');
            const employeeId = row.getAttribute('data-employee-id');
            const role = this.value;

            const proceed = await confirmChange("Are you sure you want to change the role?", "danger");
            if (!proceed) return;

            await fetch('/HR/UpdateRole', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `employeeId=${employeeId}&role=${role}`
            });

            showToast('Role updated successfully!', 'success');
        });
    });

// Status toggle
document.querySelectorAll('#manageAccountsSection .status-toggle')
    .forEach(btn => {
        btn.addEventListener('click', async function () {
            const row = this.closest('tr');
            const employeeId = row.getAttribute('data-employee-id');

            const currentActive = this.classList.contains('btn-info');
            const newState = !currentActive;

            const proceed = await confirmChange(`Are you sure you want to ${newState ? 'activate' : 'deactivate'} this user?`, "danger");
            if (!proceed) return;

            await fetch('/HR/UpdateStatus', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `employeeId=${employeeId}&isActive=${newState}`
            });

            // Update UI
            if (newState) {
                this.classList.remove('btn-outline-info');
                this.classList.add('btn-info', 'text-dark');
                this.textContent = 'Activated';
            } else {
                this.classList.remove('btn-info', 'text-dark');
                this.classList.add('btn-outline-info');
                this.textContent = 'Deactivated';
            }

            showToast(`User has been ${newState ? 'activated' : 'deactivated'} successfully!`, 'success');
        });
    });


// Utility function: show toast
function showToast(message, type = 'info', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0 show`;
    toast.role = "alert";
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    document.getElementById('toastContainer').appendChild(toast);

    // Auto remove after duration
    setTimeout(() => {
        toast.classList.remove('show');
        toast.remove();
    }, duration);
}

async function confirmChange(message, type = 'info') {
    return new Promise((resolve) => {
        const modalHtml = `
        <div class="modal fade" tabindex="-1" style="z-index:1100">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content bg-dark text-light border-${type} shadow">
                    <div class="modal-body text-center py-4">
                        <i class="bi ${type === 'danger' ? 'bi-exclamation-triangle-fill' : 'bi-question-circle-fill'} display-4 text-${type} mb-3"></i>
                        <p class="fs-5">${message}</p>
                        <div class="mt-4 d-flex justify-content-center gap-3">
                            <button type="button" class="btn btn-outline-light btn-sm cancel-btn px-4">Cancel</button>
                            <button type="button" class="btn btn-${type} btn-sm confirm-btn px-4">Yes</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        `;

        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = modalHtml;
        document.body.appendChild(tempDiv);
        const modalEl = tempDiv.querySelector('.modal');
        const bsModal = new bootstrap.Modal(modalEl);
        bsModal.show();

        modalEl.querySelector('.confirm-btn').addEventListener('click', () => {
            resolve(true);
            bsModal.hide();
        });
        modalEl.querySelector('.cancel-btn').addEventListener('click', () => {
            resolve(false);
            bsModal.hide();
        });
        modalEl.addEventListener('hidden.bs.modal', () => {
            tempDiv.remove();
        });
    });
}









