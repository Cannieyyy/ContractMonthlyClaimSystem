(function () {
    'use strict';

    // Helpers
    function escapeHtml(s) {
        if (!s) return '';
        return s.replace(/[&<>"]/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c];
        });
    }

    function formatCurrency(n) {
        if (n == null) return '';
        return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(n);
    }

    function badgeClass(status) {
        if (!status) return 'bg-secondary';
        const s = String(status).toLowerCase();
        if (s === 'pending') return 'bg-warning';
        if (s === 'Verified' || s === 'verified') return 'bg-success';
        if (s === 'rejected') return 'bg-danger';
        if (s === 'deleted') return 'bg-secondary';
        return 'bg-secondary';
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // Main
    document.addEventListener('DOMContentLoaded', function () {
        const verifyCard = document.querySelector('.verify-claims-card');
        const verifySection = document.getElementById('verifyClaimsSection');
        const closeVerifyBtn = document.getElementById('closeVerifyClaims');
        const tbody = verifySection ? verifySection.querySelector('tbody') : null;

        async function loadClaims() {
            if (!tbody) return;

            // Show loading
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Loading...</td></tr>';

            try {
                const res = await fetch('/Coordinator/GetClaimsForVerification', { method: 'GET', credentials: 'same-origin' });
                if (!res.ok) throw new Error('Failed to fetch claims');

                const payload = await res.json();
                if (!payload.success) {
                    tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Failed to load claims</td></tr>';
                    return;
                }

                const claims = payload.data || [];
                if (claims.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No claims found for your department.</td></tr>';
                    return;
                }

                // Build table rows
                let html = '';
                for (const c of claims) {
                    const claimId = c.ClaimID ?? c.claimID ?? '';
                    const employeeName = c.EmployeeName ?? c.employeeName ?? (c.employee && (c.employee.Name || c.employee.name)) ?? '';
                    const claimDate = c.ClaimDate ?? c.claimDate ?? '';
                    const hoursWorked = c.HoursWorked ?? c.hoursWorked ?? '';
                    const totalAmount = c.TotalAmount ?? c.totalAmount ?? '';
                    const documentId = c.DocumentID ?? c.documentID ?? null;
                    const documentName = c.DocumentName ?? c.documentName ?? '';
                    const status = c.Status ?? c.status ?? '';

                    const docCell = documentId
                        ? `<a href="/Dashboard/DownloadDocument/${encodeURIComponent(documentId)}" target="_blank" class="btn btn-sm btn-outline-info">View</a><div class="small text-muted mt-1">${escapeHtml(documentName)}</div>`
                        : '<span class="text-muted">No doc</span>';

                    const isDeleted = String(status).toLowerCase() === 'deleted';
                    const VerifyBtn = `<button class="btn btn-success btn-sm me-1 action-Verify" data-claim-id="${escapeHtml(String(claimId))}" ${isDeleted ? 'disabled' : ''}>Verify</button>`;
                    const rejectBtn = `<button class="btn btn-danger btn-sm action-reject" data-claim-id="${escapeHtml(String(claimId))}" ${isDeleted ? 'disabled' : ''}>Reject</button>`;

                    html += `<tr>
                        <td>${escapeHtml(String(claimId))}</td>
                        <td>${escapeHtml(employeeName)}</td>
                        <td>${escapeHtml(claimDate)}</td>
                        <td>${escapeHtml(String(hoursWorked))}</td>
                        <td>${formatCurrency(totalAmount)}</td>
                        <td>${docCell}</td>
                        <td><span class="badge ${badgeClass(status)}">${escapeHtml(String(status))}</span></td>
                        <td>${VerifyBtn}${rejectBtn}</td>
                    </tr>`;
                }

                tbody.innerHTML = html;

                // Attach Verify/reject handlers
                tbody.querySelectorAll('.action-Verify').forEach(btn => {
                    btn.addEventListener('click', function () {
                        const id = this.getAttribute('data-claim-id');
                        if (!id) return alert('Claim ID missing');
                        if (!confirm('Verify claim #' + id + '?')) return;

                        fetch('/Coordinator/VerifyClaim', {
                            method: 'POST',
                            credentials: 'same-origin',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                                'RequestVerificationToken': getAntiForgeryToken()
                            },
                            body: 'claimId=' + encodeURIComponent(id)
                        })
                            .then(r => {
                                if (r.ok) {
                                    alert('Claim Verified');
                                    loadClaims();
                                } else {
                                    alert('Failed to Verify claim');
                                }
                            })
                            .catch(e => {
                                console.error(e);
                                alert('Error approving claim');
                            });
                    });
                });

                tbody.querySelectorAll('.action-reject').forEach(btn => {
                    btn.addEventListener('click', function () {
                        const id = this.getAttribute('data-claim-id');
                        if (!id) return alert('Claim ID missing');

                        const comment = prompt('Optional comment for rejection (leave empty for none):');
                        if (comment === null) return;

                        fetch('/Coordinator/RejectClaim', {
                            method: 'POST',
                            credentials: 'same-origin',
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded',
                                'RequestVerificationToken': getAntiForgeryToken()
                            },
                            body: 'claimId=' + encodeURIComponent(id) + '&comment=' + encodeURIComponent(comment)
                        })
                            .then(r => {
                                if (r.ok) {
                                    alert('Claim rejected');
                                    loadClaims();
                                } else {
                                    alert('Failed to reject claim');
                                }
                            })
                            .catch(e => {
                                console.error(e);
                                alert('Error rejecting claim');
                            });
                    });
                });

            } catch (err) {
                console.error(err);
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Error loading claims</td></tr>';
            }
        }

        // Show/Hide sections
        if (verifyCard && verifySection) {
            verifyCard.addEventListener('click', function () {
                verifySection.style.display = 'block';
                loadClaims();
                verifySection.scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        }

        if (closeVerifyBtn && verifySection) {
            closeVerifyBtn.addEventListener('click', function () {
                verifySection.style.display = 'none';
                verifyCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
            });
        }

        // Manage Lecturers Section
        (function () {
            const manageCard = document.querySelector('.manage-lecturers-card');
            const manageSection = document.getElementById('manageLecturersSection');
            const closeManageBtn = document.getElementById('closeManageLecturers');

            if (manageCard) {
                manageCard.addEventListener('click', function () {
                    if (manageSection) {
                        manageSection.style.display = 'block';
                        manageSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                });
            }
            if (closeManageBtn && manageSection) {
                closeManageBtn.addEventListener('click', function () {
                    manageSection.style.display = 'none';
                    manageCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
                });
            }
        })();

        // Reports Section
        (function () {
            const reportsCard = document.querySelector('.reports-card');
            const reportsSection = document.getElementById('reportsSection');
            const closeReportsBtn = document.getElementById('closeReports');

            if (reportsCard) {
                reportsCard.addEventListener('click', function () {
                    if (reportsSection) {
                        reportsSection.style.display = 'block';
                        reportsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                });
            }
            if (closeReportsBtn && reportsSection) {
                closeReportsBtn.addEventListener('click', function () {
                    reportsSection.style.display = 'none';
                    reportsCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
                });
            }
        })();

        // Display current date
        (function () {
            try {
                const today = new Date();
                const options = { year: 'numeric', month: 'long', day: 'numeric' };
                const formattedDate = today.toLocaleDateString('en-US', options);
                const el = document.getElementById('currentDate');
                if (el) el.textContent = formattedDate;
            } catch (e) { }
        })();
    });
})();


document.addEventListener('DOMContentLoaded', function () {
    const tbody = document.querySelector('#verifyClaimsSection tbody');
    if (!tbody) return;

    async function loadClaimsDebug() {
        tbody.innerHTML = '<tr><td colspan="8">Loading...</td></tr>';
        try {
            const res = await fetch('/Coordinator/GetClaimsForVerification', { method: 'GET', credentials: 'same-origin' });
            const data = await res.json();
            console.log('Claims API response:', data);

            if (!data.success || !Array.isArray(data.data) || data.data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8">No claims found.</td></tr>';
                return;
            }

            tbody.innerHTML = '';
            data.data.forEach(c => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${c.ClaimID}</td>
                    <td>${c.EmployeeName}</td>
                    <td>${c.ClaimDate}</td>
                    <td>${c.HoursWorked}</td>
                    <td>${c.TotalAmount}</td>
                    <td>${c.DocumentID ? `<a href="/Dashboard/DownloadDocument/${c.DocumentID}" target="_blank">View</a>` : 'No doc'}</td>
                    <td>${c.Status}</td>
                    <td>
                        <button class="btn-Verify" data-id="${c.ClaimID}">Verify</button>
                        <button class="btn-reject" data-id="${c.ClaimID}">Reject</button>
                    </td>
                `;
                tbody.appendChild(row);
            });

        } catch (err) {
            console.error(err);
            tbody.innerHTML = '<tr><td colspan="8">Error loading claims</td></tr>';
        }
    }

    console.log(loadClaimsDebug());
});
