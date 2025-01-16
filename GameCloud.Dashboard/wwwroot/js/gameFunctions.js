class GameFunctionsManager {
    constructor() {
        this.refreshInterval = 30000;
        this.refreshTimer = null;
        this.initializeEventListeners();
        this.startAutoRefresh();
    }

    // Form & Header Management
    addHeaderRow() {
        const container = document.getElementById('headersContainer');
        const template = document.getElementById('headerRowTemplate');
        if (!container || !template) return;

        const clone = template.content.cloneNode(true);
        container.appendChild(clone);
        this.updateHeaderIndexes();
    }

    removeHeaderRow(button) {
        const row = button.closest('.input-group');
        if (row) {
            row.remove();
            this.updateHeaderIndexes();
        }
    }

    updateHeaderIndexes() {
        const rows = document.querySelectorAll('#headersContainer .input-group');
        rows.forEach((index, row) => {
            row.querySelector('input[name^="Headers["]').name = `Headers[${index}].Key`;
            row.querySelector('input[name$="].Value"]').name = `Headers[${index}].Value`;
        });
    }

    // Modal Management
    showModal(isEdit = false) {
        const modal = document.getElementById('functionModal');
        if (!modal) return;

        // Reset form before showing
        const form = modal.querySelector('#functionForm');
        if (form) {
            form.reset();
            form.action = isEdit ? '?handler=Update' : '?handler=Create';
            document.getElementById('headersContainer').innerHTML = '';
        }

        // Update modal title and button text
        modal.querySelector('#functionModalTitle').textContent =
            isEdit ? 'Edit Function' : 'Register Function';
        modal.querySelector('#submitButtonText').textContent =
            isEdit ? 'Save Changes' : 'Register Function';

        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    // Event Handlers
    async handleEditClick(e) {
        e.preventDefault();
        const functionId = e.currentTarget.dataset.functionId;
        if (!functionId) return;

        try {
            const response = await fetch(`?handler=Function&functionId=${functionId}`);
            if (!response.ok) throw new Error('Failed to fetch function details');

            const data = await response.json();
            this.populateForm(data);
            this.showModal(true);
        } catch (error) {
            console.error('Error:', error);
            alert('Failed to load function details. Please try again.');
        }
    }

    populateForm(data) {
        const form = document.getElementById('functionForm');
        if (!form) {
            console.error('Form not found');
            return;
        }

        // Clear form fields first
        form.reset();
        document.getElementById('headersContainer').innerHTML = '';

        // Directly set each field based on the exact field names from the response
        const fields = [
            { responseKey: 'id', formName: 'Id' },
            { responseKey: 'name', formName: 'Name' },
            { responseKey: 'description', formName: 'Description' },
            { responseKey: 'endpoint', formName: 'Endpoint' },
            { responseKey: 'actionType', formName: 'ActionType' },
            { responseKey: 'isEnabled', formName: 'IsEnabled' },
            { responseKey: 'maxRetries', formName: 'MaxRetries' },
            { responseKey: 'timeoutSeconds', formName: 'TimeoutSeconds' }
        ];

        fields.forEach(({ responseKey, formName }) => {
            if (responseKey in data) {
                const input = form.querySelector(`[name="${formName}"]`);
                if (input) {
                    if (input.type === 'checkbox') {
                        input.checked = Boolean(data[responseKey]);
                    } else if (input.type === 'number') {
                        input.value = Number(data[responseKey]) || 0;
                    } else {
                        input.value = data[responseKey] || '';
                    }
                    console.log(`Set ${formName} to`, input.type === 'checkbox' ? input.checked : input.value);
                } else {
                    console.log(`Input not found for ${formName}`);
                }
            } else {
                console.log(`Field ${responseKey} not present in response data`);
            }
        });

        // Handle headers if present
        if (data.headers && typeof data.headers === 'object') {
            Object.entries(data.headers).forEach(([key, value]) => {
                this.addHeaderRow();
                const lastRow = document.querySelector('#headersContainer .input-group:last-child');
                if (lastRow) {
                    const keyInput = lastRow.querySelector('input[name$="].Key"]');
                    const valueInput = lastRow.querySelector('input[name$="].Value"]');
                    if (keyInput) keyInput.value = key;
                    if (valueInput) valueInput.value = value;
                    console.log(`Added header: ${key} = ${value}`);
                }
            });
        }

        // Verify form state after population
        console.log('Form values after population:');
        fields.forEach(({ formName }) => {
            const input = form.querySelector(`[name="${formName}"]`);
            if (input) {
                console.log(`${formName}:`, input.type === 'checkbox' ? input.checked : input.value);
            }
        });
    }

    async handleToggle(functionId, enabled) {
        if (!functionId) return;

        try {
            const form = new FormData();
            form.append('functionId', functionId);
            form.append('isEnabled', enabled);
            form.append('__RequestVerificationToken',
                document.querySelector('input[name="__RequestVerificationToken"]').value);

            const response = await fetch('?handler=Toggle', {
                method: 'POST',
                body: form
            });

            if (!response.ok) throw new Error('Failed to update function status');
            window.location.reload();
        } catch (error) {
            console.error('Error:', error);
            alert('Failed to update function status. Please try again.');
            // Revert toggle state
            const toggle = document.querySelector(`[data-function-id="${functionId}"]`);
            if (toggle) toggle.checked = !enabled;
        }
    }

    async handleDelete(functionId, functionName) {
        if (!confirm(`Are you sure you want to delete the function "${functionName}"?`)) return;

        try {
            const form = new FormData();
            form.append('functionId', functionId);
            form.append('__RequestVerificationToken',
                document.querySelector('input[name="__RequestVerificationToken"]').value);

            const response = await fetch('?handler=Delete', {
                method: 'POST',
                body: form
            });

            if (!response.ok) throw new Error('Failed to delete function');
            window.location.reload();
        } catch (error) {
            console.error('Error:', error);
            alert('Failed to delete function. Please try again.');
        }
    }

    // Table Management
    async refreshTable() {
        try {
            const response = await fetch(window.location.href);
            if (!response.ok) throw new Error('Failed to refresh data');

            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');

            // Update table
            const newTable = doc.querySelector('.table-responsive');
            const currentTable = document.querySelector('.table-responsive');
            if (newTable && currentTable) {
                currentTable.innerHTML = newTable.innerHTML;
            }

            // Update stats
            const newStats = doc.querySelectorAll('.card-animate');
            document.querySelectorAll('.card-animate').forEach((card, index) => {
                if (newStats[index]) card.innerHTML = newStats[index].innerHTML;
            });

            this.initializeEventListeners();
        } catch (error) {
            console.error('Error:', error);
        }
    }

    startAutoRefresh() {
        this.stopAutoRefresh();
        this.refreshTimer = setInterval(() => this.refreshTable(), this.refreshInterval);
    }

    stopAutoRefresh() {
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }
    }

    // Event Listeners
    initializeEventListeners() {
        // Form handling
        document.querySelectorAll('form').forEach(form => {
            form.removeEventListener('submit', this.handleFormSubmit);
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                try {
                    const response = await fetch(form.action, {
                        method: 'POST',
                        body: new FormData(form)
                    });

                    if (!response.ok) throw new Error('Form submission failed');
                    window.location.reload();
                } catch (error) {
                    console.error('Error:', error);
                    alert('Form submission failed. Please try again.');
                }
            });
        });

        // Action buttons
        document.querySelectorAll('[data-action]').forEach(element => {
            const action = element.dataset.action;
            element.removeEventListener('click', this[action]);

            switch (action) {
                case 'edit':
                    element.addEventListener('click', e => this.handleEditClick(e));
                    break;
                case 'delete':
                    element.addEventListener('click', e =>
                        this.handleDelete(
                            e.currentTarget.dataset.functionId,
                            e.currentTarget.dataset.functionName
                        ));
                    break;
                case 'toggle':
                    element.addEventListener('change', e =>
                        this.handleToggle(
                            e.currentTarget.dataset.functionId,
                            e.currentTarget.checked
                        ));
                    break;
            }
        });

        // Refresh button
        const refreshBtn = document.getElementById('refreshTable');
        if (refreshBtn) {
            refreshBtn.removeEventListener('click', this.refreshTable);
            refreshBtn.addEventListener('click', () => {
                this.refreshTable();
                this.startAutoRefresh();
            });
        }

        // Initialize tooltips
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el =>
            new bootstrap.Tooltip(el));
    }
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gameFunctionsManager = new GameFunctionsManager();
});

window.addHeaderRow = () => window.gameFunctionsManager?.addHeaderRow();
window.removeHeaderRow = (button) => window.gameFunctionsManager?.removeHeaderRow(button);
window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text)
        .then(() => {
            const tooltip = bootstrap.Tooltip.getInstance(event.currentTarget);
            const originalTitle = event.currentTarget.getAttribute('title');

            event.currentTarget.setAttribute('title', 'Copied!');
            tooltip.update();

            setTimeout(() => {
                event.currentTarget.setAttribute('title', originalTitle);
                tooltip.update();
            }, 1000);
        })
        .catch(err => console.error('Failed to copy:', err));
};