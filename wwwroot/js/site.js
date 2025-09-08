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
        if (value === 'Coordinator' || value === 'Manager') {
            otpContainer.style.display = 'block';
            otpNotifyContainer.style.display = 'block';
        } else {
            otpContainer.style.display = 'none';
            otpNotifyContainer.style.display = 'none';
        }
    });

    // Placeholder for sending OTP
    sendOtpBtn.addEventListener('click', function () {
        alert("An OTP would be sent to your department manager now.");
        // later, this will call your backend endpoint to send OTP
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






