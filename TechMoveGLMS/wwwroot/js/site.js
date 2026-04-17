// Note the 'async' keyword added here!
document.addEventListener("DOMContentLoaded", async function () {

    // --- 1. Sidebar Navigation Logic ---
    const sidebar = document.getElementById('sidebar');
    const openBtn = document.getElementById('open-sidebar');
    const closeBtn = document.getElementById('close-sidebar');

    if (openBtn && closeBtn) {
        openBtn.addEventListener('click', () => sidebar.classList.add('open'));
        closeBtn.addEventListener('click', () => sidebar.classList.remove('open'));
    }

    // Dynamic Active Nav Linking
    const currentLocation = location.pathname;
    const navLinks = document.querySelectorAll('.nav-links li a');
    navLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === currentLocation ||
            (currentLocation.length > 1 && link.getAttribute('href').startsWith(currentLocation))) {
            link.classList.add('active');
        }
    });

    // --- 2. Live Currency Converter Logic ---
    const usdInput = document.getElementById('usdInput');
    if (usdInput) { // Only run if we are on the ServiceRequest Create page
        const zarPreview = document.getElementById('zarPreview');
        const rateLabel = document.getElementById('rateLabel');
        let currentRate = 0;

        try {
            const response = await fetch('https://open.er-api.com/v6/latest/USD');
            const data = await response.json();
            currentRate = data.rates.ZAR;
            rateLabel.textContent = `Live Exchange Rate: 1 USD = ${currentRate.toFixed(2)} ZAR`;
            zarPreview.placeholder = "0.00";
        } catch (error) {
            rateLabel.textContent = "Unable to fetch live rates. Backend will calculate upon submission.";
            rateLabel.style.color = "red";
        }

        usdInput.addEventListener('input', function () {
            const usdValue = parseFloat(usdInput.value);
            if (!isNaN(usdValue) && currentRate > 0) {
                const converted = usdValue * currentRate;
                zarPreview.value = "R " + converted.toLocaleString('en-ZA', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            } else {
                zarPreview.value = "";
            }
        });
    }
});