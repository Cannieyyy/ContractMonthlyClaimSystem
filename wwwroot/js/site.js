// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.addEventListener("scroll", function () {
    const navbar = document.getElementById("mainNavbar");
    if (window.scrollY > 50) {
        navbar.classList.add("scrolled");
    } else {
        navbar.classList.remove("scrolled");
    }
});

//floating for forms
document.addEventListener('DOMContentLoaded', function () {
    // handle selects and inputs inside .form-floating
    function initFloatingControls() {
        // selects
        document.querySelectorAll('.form-floating .form-select').forEach(function (sel) {
            var parent = sel.closest('.form-floating');
            function check() {
                if (sel.value && sel.value !== '') parent.classList.add('filled');
                else parent.classList.remove('filled');
            }
            // initial state
            check();
            // update on change
            sel.addEventListener('change', check);
            // also update on blur (some browsers fill or autofill)
            sel.addEventListener('blur', check);
        });

        // inputs (optional: keep label floated if user typed something)
        document.querySelectorAll('.form-floating .form-control').forEach(function (inp) {
            var parent = inp.closest('.form-floating');
            function checkInput() {
                if (inp.value && inp.value.trim() !== '') parent.classList.add('filled');
                else parent.classList.remove('filled');
            }
            checkInput();
            inp.addEventListener('input', checkInput);
            inp.addEventListener('blur', checkInput);
        });
    }

    initFloatingControls();
});

//otp display
document.addEventListener('DOMContentLoaded', function () {
    const roleSelect = document.getElementById('employeeRole');
    const otpContainer = document.getElementById('otpContainer');
    const otpNotifyContainer = document.getElementById('otpNotifyContainer');
    const sendOtpBtn = document.getElementById('sendOtpBtn');

    roleSelect.addEventListener('change', function () {
        const value = roleSelect.value;
        if (value === 'Coordinator' || value === 'Manager' || value === 'HR Admin') {
            otpContainer.style.display = 'block';
            otpNotifyContainer.style.display = 'block';
        } else {
            otpContainer.style.display = 'none';
            otpNotifyContainer.style.display = 'none';
        }
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const submitCard = document.getElementById('submitClaimCard');
    const claimFormSection = document.getElementById('claimFormSection');
    const cancelBtn = document.getElementById('cancelClaimBtn');
    const claimForm = document.getElementById('claimForm');

    // toggle form visibility when clicking the submit card
    submitCard.addEventListener('click', function () {
        const isVisible = window.getComputedStyle(claimFormSection).display !== 'none';
        if (!isVisible) {
            claimFormSection.style.display = 'block';
            claimFormSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        } else {
            claimFormSection.style.display = 'none';
            submitCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    });

    // cancel button hides form
    cancelBtn.addEventListener('click', function () {
        claimFormSection.style.display = 'none';
        submitCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });

    // Prevent real submit in the prototype — show a friendly confirmation instead
    claimForm.addEventListener('submit', function (e) {
        e.preventDefault();
        // example proto feedback — replace later with backend POST
        const hours = document.getElementById('hoursWorked').value || '0';
        const month = document.getElementById('workMonth').value || '(no month)';
        // brief visual feedback — replace with a nicer toast later
        alert('Prototype: Claim submitted\nHours: ' + hours + '\nMonth: ' + month);
        // reset and hide form
        claimForm.reset();
        claimFormSection.style.display = 'none';
        submitCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
});

const analyticsCard = document.getElementById('analyticsCard');
const chartContainer = document.getElementById('analyticsChartContainer');
const cancelBtn = document.getElementById('cancelAnalyticsBtn');
let chartInitialized = false;

analyticsCard.addEventListener('click', function () {
    chartContainer.style.display = 'block';

    if (!chartInitialized) {
        const ctx = document.getElementById('quickReportsChart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Hours Worked',
                    data: [35, 42, 30, 50, 45, 40],
                    backgroundColor: '#0dcaf0',
                    borderColor: '#0aa8c0',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { labels: { color: '#ffffff' } },
                    tooltip: {
                        titleColor: '#0dcaf0',
                        bodyColor: '#e0e0e0',
                        backgroundColor: '#2a2a2a'
                    }
                },
                scales: {
                    x: { ticks: { color: '#ffffff' }, grid: { color: 'rgba(255,255,255,0.1)' } },
                    y: { beginAtZero: true, ticks: { color: '#ffffff' }, grid: { color: 'rgba(255,255,255,0.1)' } }
                }
            }
        });
        chartInitialized = true;
    }

    chartContainer.scrollIntoView({ behavior: 'smooth' });
});

cancelBtn.addEventListener('click', function () {
    chartContainer.style.display = 'none';
});

const viewClaimsCard = document.getElementById('viewClaimsCard');
const viewClaimsContainer = document.getElementById('viewClaimsContainer');
const cancelViewClaimsBtn = document.getElementById('cancelViewClaimsBtn');

viewClaimsCard.addEventListener('click', function () {
    viewClaimsContainer.style.display = 'block';
    viewClaimsContainer.scrollIntoView({ behavior: 'smooth' });
});

cancelViewClaimsBtn.addEventListener('click', function () {
    viewClaimsContainer.style.display = 'none';
});


//login_register js logic
(function () {
    // Show OTP UI when privileged role is selected (Coordinator or Manager)
    const roleSelect = document.getElementById('employeeRole');
    const otpNotify = document.getElementById('otpNotifyContainer');
    const otpContainer = document.getElementById('otpContainer');
    const sendOtpBtn = document.getElementById('sendOtpBtn');
    const registerPassword = document.getElementById('registerPassword');
    const registerConfirm = document.getElementById('registerConfirmPassword');
    const registerForm = document.querySelector('#register form') || document.forms[0];

    function updateOtpVisibility() {
        const val = roleSelect.value;
        // treat 'Lecturer' as non-privileged
        if (val && val.toLowerCase() !== 'lecturer') {
            otpNotify.style.display = 'block';
            otpContainer.style.display = 'block';
        } else {
            otpNotify.style.display = 'none';
            otpContainer.style.display = 'none';
        }
    }

    if (roleSelect) {
        roleSelect.addEventListener('change', updateOtpVisibility);
        updateOtpVisibility(); // initialize on load
    }

    // Basic client-side password match check — server validation is authoritative
    if (registerConfirm && registerPassword) {
        registerConfirm.addEventListener('input', function () {
            if (registerConfirm.value !== registerPassword.value) {
                registerConfirm.setCustomValidity('Passwords do not match.');
            } else {
                registerConfirm.setCustomValidity('');
            }
        });
    }

    // Hook to the "Send OTP" button - currently placeholder (no backend yet)
    if (sendOtpBtn) {
        sendOtpBtn.addEventListener('click', function () {
            // Placeholder behaviour: show a small alert. When we implement OTP backend,
            // this should call an endpoint that triggers the OTP sending process.
            alert('OTP request sent to department manager (placeholder).');
        });
    }
})();

//display date
// Get current date
const today = new Date();

// Format options
const options = { year: 'numeric', month: 'long', day: 'numeric' };

// Format the date
const formattedDate = today.toLocaleDateString('en-US', options);

// Insert it into the span
document.getElementById('currentDate').textContent = formattedDate;

//Modal population script
    
        (function () {
                var editModalEl = document.getElementById('editClaimModal');
                var editModal = editModalEl ? new bootstrap.Modal(editModalEl) : null;

        document.querySelectorAll('.edit-claim-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var claimId = btn.getAttribute('data-claim-id');
                var hours = btn.getAttribute('data-hours');
                var month = btn.getAttribute('data-month');
                var docId = btn.getAttribute('data-doc-id');
                var docName = btn.getAttribute('data-doc-name');

                document.getElementById('editClaimID').value = claimId;
                document.getElementById('editHoursWorked').value = hours;
                document.getElementById('editWorkMonth').value = month;

                var currentDocDiv = document.getElementById('currentDocInfo');
                if (docId && docId.length > 0) {
                    currentDocDiv.innerHTML = '<a href="' + (window.location.origin + '/Dashboard/DownloadDocument/' + docId) + '" target="_blank" class="text-info">' + docName + '</a>';
                } else {
                    currentDocDiv.textContent = '— none —';
                }

                var fileInput = document.getElementById('editSupportingDoc');
                if (fileInput) fileInput.value = '';

                if (editModal) editModal.show();
            });
                });
            })();
    



// wwwroot/js/coordinator.js
document.addEventListener('DOMContentLoaded', function () {
    const verifyCard = document.querySelector('.verify-claims-card');
    const verifySection = document.getElementById('verifyClaimsSection');
    const closeBtn = document.getElementById('closeVerifyClaims');
    const tbody = verifySection ? verifySection.querySelector('tbody') : null;

    async function loadClaims() {
        if (!tbody) return;

        tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Loading...</td></tr>';

        try {
            const res = await fetch('/Coordinator/GetClaimsForVerification', { method: 'GET', credentials: 'same-origin' });
            if (!res.ok) throw new Error('Failed to fetch claims: ' + res.status);

            const payload = await res.json();
            if (!payload.success) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Failed to load claims</td></tr>';
                return;
            }

            const claims = payload.data;
            if (!claims || claims.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-muted">No claims found for your department.</td></tr>';
                return;
            }

            // Build rows
            let html = '';
            for (let i = 0; i < claims.length; i++) {
                const c = claims[i];

                // document link cell (uses camelCase JSON)
                const docCell = c.documentID
                    ? `<a href="/Dashboard/DownloadDocument/${c.documentID}" target="_blank" class="btn btn-sm btn-outline-info">View</a><div class="small text-muted mt-1">${escapeHtml(c.documentName || '')}</div>`
                    : '<span class="text-muted">No doc</span>';

                const isDeleted = (c.status && c.status.toLowerCase() === 'deleted');
                const VerifyBtn = `<button class="btn btn-success btn-sm me-1 action-Verify" data-claim-id="${c.claimID}" ${isDeleted ? 'disabled' : ''}>Verify</button>`;
                const rejectBtn = `<button class="btn btn-danger btn-sm action-reject" data-claim-id="${c.claimID}" ${isDeleted ? 'disabled' : ''}>Reject</button>`;

                html += `<tr>
                    <td>${c.claimID}</td>
                    <td>${escapeHtml(c.employeeName)}</td>
                    <td>${escapeHtml(c.claimDate)}</td>
                    <td>${escapeHtml(c.hoursWorked)}</td>
                    <td>${formatCurrency(c.totalAmount)}</td>
                    <td>${docCell}</td>
                    <td><span class="badge ${badgeClass(c.status)}">${escapeHtml(c.status)}</span></td>
                    <td>${VerifyBtn}${rejectBtn}</td>
                </tr>`;
            }

            tbody.innerHTML = html;

            attachActionHandlers();

        } catch (err) {
            console.error(err);
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Error loading claims</td></tr>';
        }
    }

    function attachActionHandlers() {
        document.querySelectorAll('.action-Verify').forEach(btn => {
            btn.removeEventListener('click', VerifyClick); // safe remove
            btn.addEventListener('click', VerifyClick);
        });

        document.querySelectorAll('.action-reject').forEach(btn => {
            btn.removeEventListener('click', rejectClick);
            btn.addEventListener('click', rejectClick);
        });
    }

    function VerifyClick() {
        const id = this.getAttribute('data-claim-id');
        if (!confirm('Verify claim #' + id + '?')) return;

        fetch('/Coordinator/VerifyClaim', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: 'claimId=' + encodeURIComponent(id)
        }).then(r => {
            if (r.ok) {
                alert('Claim Verified');
                loadClaims();
            } else {
                r.text().then(t => alert('Failed to Verify claim: ' + t));
            }
        }).catch(e => { console.error(e); alert('Error approving claim'); });
    }

    function rejectClick() {
        const id = this.getAttribute('data-claim-id');
        const comment = prompt('Optional comment for rejection (leave empty for none):');
        if (comment === null) return;

        fetch('/Coordinator/RejectClaim', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: 'claimId=' + encodeURIComponent(id) + '&comment=' + encodeURIComponent(comment || '')
        }).then(r => {
            if (r.ok) {
                alert('Claim rejected');
                loadClaims();
            } else {
                r.text().then(t => alert('Failed to reject claim: ' + t));
            }
        }).catch(e => { console.error(e); alert('Error rejecting claim'); });
    }

    // helpers
    function escapeHtml(s) {
        if (!s) return '';
        return s.replace(/[&<>"]/g, function (c) {
            return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c];
        });
    }
    function formatCurrency(n) {
        if (n == null) return '';
        return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(n);
    }
    function badgeClass(status) {
        if (!status) return 'bg-secondary';
        const s = status.toLowerCase();
        if (s === 'pending') return 'bg-warning';
        if (s === 'Verified' || s === 'verified') return 'bg-success';
        if (s === 'rejected') return 'bg-danger';
        if (s === 'deleted') return 'bg-secondary';
        return 'bg-secondary';
    }
    function getAntiForgeryToken() {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // show section and load when card clicked
    if (verifyCard && verifySection) {
        verifyCard.addEventListener('click', function () {
            verifySection.style.display = 'block';
            loadClaims();
            verifySection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    }

    if (closeBtn && verifySection) {
        closeBtn.addEventListener('click', function () {
            verifySection.style.display = 'none';
            verifyCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
        });
    }

    // current date (keeps it here)
    const today = new Date();
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    const formattedDate = today.toLocaleDateString('en-US', options);
    const dateSpan = document.getElementById('currentDate');
    if (dateSpan) dateSpan.textContent = formattedDate;
});
