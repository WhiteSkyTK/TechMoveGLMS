// Note: 'async' is used here for the live currency fetch
document.addEventListener("DOMContentLoaded", async function () {

    // =========================================================
    // 1. Sidebar Navigation Logic
    // =========================================================
    const sidebar = document.getElementById('sidebar');
    const openBtn = document.getElementById('open-sidebar');
    const closeBtn = document.getElementById('close-sidebar');

    if (openBtn) {
        openBtn.addEventListener('click', () => sidebar.classList.add('open'));
    }
    if (closeBtn) {
        closeBtn.addEventListener('click', () => sidebar.classList.remove('open'));
    }

    // Dynamic Active Nav Linking — highlights current section
    const currentPath = location.pathname;
    document.querySelectorAll('.nav-links li a').forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href');
        if (
            href === currentPath ||
            (href !== '/' && currentPath.toLowerCase().startsWith(href.toLowerCase()))
        ) {
            link.classList.add('active');
        }
    });

    // =========================================================
    // 2. Live Currency Converter (ServiceRequests/Create)
    // =========================================================
    const usdInput = document.getElementById('usdInput');
    if (usdInput) {
        const zarPreview = document.getElementById('zarPreview');
        const rateLabel = document.getElementById('rateLabel');
        let currentRate = 0;

        try {
            const response = await fetch('https://open.er-api.com/v6/latest/USD');
            if (!response.ok) throw new Error('API returned non-OK status');
            const data = await response.json();
            currentRate = data.rates.ZAR;
            rateLabel.textContent = `Live Rate: 1 USD = R ${currentRate.toFixed(4)} ZAR`;
            rateLabel.style.color = '#10b981';
        } catch (error) {
            rateLabel.textContent = 'Unable to fetch live rate. The server will calculate on submission.';
            rateLabel.style.color = '#ef4444';
        }

        usdInput.addEventListener('input', function () {
            const usdValue = parseFloat(usdInput.value);
            if (!isNaN(usdValue) && usdValue >= 0 && currentRate > 0) {
                const converted = usdValue * currentRate;
                zarPreview.value = 'R ' + converted.toLocaleString('en-ZA', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            } else {
                zarPreview.value = '';
            }
        });
    }

    // =========================================================
    // 3. PDF Upload Drop Zone (Contracts/Create)
    // =========================================================
    const dropZone = document.getElementById('drop-zone');
    if (dropZone) {
        const fileInput = document.getElementById('fileInput');
        const fileNameDisplay = document.getElementById('file-name-display');
        const fileError = document.getElementById('file-error');

        // Clicking anywhere on the zone triggers the file picker
        dropZone.addEventListener('click', function (e) {
            if (e.target !== fileInput) {
                fileInput.click();
            }
        });

        // Drag-over visual feedback
        dropZone.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.add('drag-over');
        });

        dropZone.addEventListener('dragleave', function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.remove('drag-over');
        });

        // Drop handler — validate and show file name
        dropZone.addEventListener('drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.remove('drag-over');

            const droppedFiles = e.dataTransfer.files;
            if (droppedFiles.length > 0) {
                // Assign dropped files to the real input so the form submits them
                const dataTransfer = new DataTransfer();
                dataTransfer.items.add(droppedFiles[0]);
                fileInput.files = dataTransfer.files;
                handleFileSelection(droppedFiles[0]);
            }
        });

        // Manual file picker selection
        fileInput.addEventListener('change', function () {
            if (fileInput.files.length > 0) {
                handleFileSelection(fileInput.files[0]);
            }
        });

        function handleFileSelection(file) {
            const ext = file.name.split('.').pop().toLowerCase();

            if (ext !== 'pdf') {
                fileError.classList.remove('hidden');
                fileNameDisplay.classList.add('hidden');
                fileNameDisplay.textContent = '';
                fileInput.value = ''; // clear invalid file
                return;
            }

            // Valid PDF
            fileError.classList.add('hidden');
            const sizeMB = (file.size / 1024 / 1024).toFixed(2);
            fileNameDisplay.innerHTML = `<i class="fa-solid fa-file-pdf"></i> ${file.name} <span style="color:#6b7280;">(${sizeMB} MB)</span>`;
            fileNameDisplay.classList.remove('hidden');
        }
    }

});