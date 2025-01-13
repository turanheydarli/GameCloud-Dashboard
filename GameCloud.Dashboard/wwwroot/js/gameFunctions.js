// Game Functions Management
class GameFunctionsManager {
    constructor() {
        this.refreshInterval = 30000; // 30 seconds
        this.refreshTimer = null;
        this.initializeEventListeners();
        this.startAutoRefresh();
    }

    // Header Management
    addHeaderRow() {
        const container = document.getElementById('headersContainer');
        const template = document.getElementById('headerRowTemplate');
        const clone = template.content.cloneNode(true);
        container.appendChild(clone);
    }

    removeHeaderRow(button) {
        button.closest('.input-group').remove();
    }

    // Modal Management
    populateModal(data) {
        const form = document.getElementById('functionForm');
        form.reset();

        // Clear existing headers
        document.getElementById('headersContainer').innerHTML = '';

        // Set basic fields
        document.getElementById('functionModalTitle').textContent = 'Edit Function';
        document.getElementById('submitButtonText').textContent = 'Save Changes';

        // Set form fields
        Object.keys(data).forEach(key => {
            const input = form.querySelector(`[name="${key}"]`);
            if (input) {
                if (input.type === 'checkbox') {
                    input.checked = data[key];
                } else {
                    input.value = data[key];
                }
            }
        });

        // Handle timeout conversion
        if (data.timeout) {
            const seconds = parseInt(data.timeout.replace(/\D/g, ''));
            form.querySelector('[name="TimeoutSeconds"]').value = seconds;
        }

        // Add header rows if they exist
        if (data.headers) {
            Object.entries(data.headers).forEach(([key, value]) => {
                this.addHeaderRow();
                const lastRow = document.querySelector('#headersContainer .input-group:last-child');
                lastRow.querySelector('[name="Headers[].Key"]').value = key;
                lastRow.querySelector('[name="Headers[].Value"]').value = value;
            });
        }
    }

    // Event Handlers
    // Event Handlers
    async handleEditClick(e) {
        e.preventDefault();
        const functionId = e.currentTarget.dataset.functionId;

        try {
            // Use proper Razor Pages handler
            const response = await fetch(`?handler=Function&functionId=${functionId}`, {
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (!response.ok) throw new Error('Failed to fetch function details');

            const data = await response.json();
            this.populateModal(data);
            new bootstrap.Modal(document.getElementById('functionModal')).show();
        } catch (error) {
            console.error('Error:', error);
        }
    }
    handleFormSubmit(e) {
        e.preventDefault();
        const form = e.currentTarget;
        const formData = new FormData(form);
        const isEdit = formData.get('Id') ? true : false;

        // Create and submit the form
        const submitForm = document.createElement('form');
        submitForm.method = 'post';
        // Add handler name to URL
        submitForm.action = `${window.location.pathname}?handler=${isEdit ? 'Update' : ''}`;

        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            submitForm.appendChild(token.cloneNode());
        }

        // Process headers to correct format
        const headerRows = document.querySelectorAll('#headersContainer .input-group');
        headerRows.forEach((row, index) => {
            const key = row.querySelector('[name="Headers[].Key"]').value;
            const value = row.querySelector('[name="Headers[].Value"]').value;
            if (key && value) {
                formData.append(`Headers[${key}]`, value);
            }
        });

        // Convert timeout to correct format
        const timeoutSeconds = formData.get('TimeoutSeconds');
        formData.set('Timeout', `PT${timeoutSeconds}S`);
        formData.delete('TimeoutSeconds');

        // Convert FormData to hidden inputs
        formData.forEach((value, key) => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = value;
            submitForm.appendChild(input);
        });

        document.body.appendChild(submitForm);
        submitForm.submit();
    }

    handleDeleteClick(e) {
        e.preventDefault();
        if (!confirm('Are you sure you want to delete this function?')) return;

        const form = document.createElement('form');
        form.method = 'post';
        // Add handler name to URL
        form.action = `${window.location.pathname}?handler=Delete`;

        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            form.appendChild(token.cloneNode());
        }

        // Add functionId
        form.appendChild(this.createHiddenInput('functionId', e.currentTarget.dataset.functionId));

        document.body.appendChild(form);
        form.submit();
    }

    handleToggle(functionId, enabled) {
        const form = document.createElement('form');
        form.method = 'post';
        // Add handler name to URL
        form.action = `${window.location.pathname}?handler=Toggle`;

        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            form.appendChild(token.cloneNode());
        }

        // Add toggle data
        form.appendChild(this.createHiddenInput('functionId', functionId));
        form.appendChild(this.createHiddenInput('isEnabled', enabled));

        document.body.appendChild(form);
        form.submit();
    }
    createHiddenInput(name, value) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        return input;
    }

    // Table Refresh
    async refreshTable() {
        try {
            const response = await fetch(window.location.href);
            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');

            // Update table content
            const newTable = doc.querySelector('.table-responsive');
            document.querySelector('.table-responsive').innerHTML = newTable.innerHTML;

            // Update stats cards
            const newStats = doc.querySelectorAll('.card-animate');
            document.querySelectorAll('.card-animate').forEach((card, index) => {
                card.innerHTML = newStats[index].innerHTML;
            });

            this.initializeEventListeners();
        } catch (error) {
            console.error('Error refreshing table:', error);
        }
    }

    startAutoRefresh() {
        this.refreshTimer = setInterval(() => this.refreshTable(), this.refreshInterval);
    }

    // Event Listener Initialization
    initializeEventListeners() {
        // Initialize tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.forEach(el => new bootstrap.Tooltip(el));

        // Edit buttons
        document.querySelectorAll('.edit-function').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleEditClick(e));
        });

        // Delete buttons
        document.querySelectorAll('.delete-function').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleDeleteClick(e));
        });

        // Form submission
        const form = document.getElementById('functionForm');
        if (form) {
            form.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }

        // Refresh button
        const refreshBtn = document.getElementById('refreshTable');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.refreshTable();
                clearInterval(this.refreshTimer);
                this.startAutoRefresh();
            });
        }
    }
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gameFunctionsManager = new GameFunctionsManager();
});