document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById('sidebar');
    const openBtn = document.getElementById('open-sidebar');
    const closeBtn = document.getElementById('close-sidebar');

    // Mobile Sidebar Toggle
    if (openBtn && closeBtn) {
        openBtn.addEventListener('click', () => {
            sidebar.classList.add('open');
        });

        closeBtn.addEventListener('click', () => {
            sidebar.classList.remove('open');
        });
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
});