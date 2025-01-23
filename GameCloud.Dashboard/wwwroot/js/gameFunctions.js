class GameFunctionsManager {
    constructor() {
        this.refreshInterval = 30000;
        this.refreshTimer = null;
        this.statsTimer = null;
        this.initializeEventListeners();
        this.startAutoRefresh();
        this.loadAllFunctionStats();
        this.loadAllExecutionStatuses(); // Add this line
        this.startAutoStatsRefresh();
    }

    // Stats Management
    async loadFunctionStats(functionId) {
        const statsContainer = document.getElementById(`stats-${functionId}`);
        if (!statsContainer) return;

        statsContainer.classList.add('stats-loading');

        try {
            const response = await fetch(`/game/${gameId}/functions?handler=FunctionStats&functionId=${functionId}`, {
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load stats');
            }

            const stats = await response.json();
            statsContainer.querySelector('.stats-success-rate').textContent =
                `${stats.successRate?.toFixed(2) || 0}%`;

            statsContainer.querySelector('.stats-avg-time').textContent =
                `${Math.round(stats.averageExecutionTimeMs || 0)}ms`;

            statsContainer.querySelector('.stats-total-calls').textContent =
                stats.totalExecutions || 0;

        } catch (error) {
            console.error(`Error loading stats for function ${functionId}:`, error);
            statsContainer.querySelector('.stats-success-rate').textContent = 'Error';
            statsContainer.querySelector('.stats-avg-time').textContent = 'Error';
            statsContainer.querySelector('.stats-total-calls').textContent = 'Error';
        } finally {
            statsContainer.classList.remove('stats-loading');
        }
    }

    loadAllFunctionStats() {
        document.querySelectorAll('[id^="stats-"]').forEach(container => {
            const functionId = container.id.replace('stats-', '');
            this.loadFunctionStats(functionId);
        });
    }

    startAutoStatsRefresh() {
        this.stopAutoStatsRefresh();
        this.statsTimer = setInterval(() => this.loadAllFunctionStats(), this.refreshInterval);
    }

    stopAutoStatsRefresh() {
        if (this.statsTimer) {
            clearInterval(this.statsTimer);
            this.statsTimer = null;
        }
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
        rows.forEach((row, index) => {
            row.querySelector('input[name^="Headers["]').name = `Headers[${index}].Key`;
            row.querySelector('input[name$="].Value"]').name = `Headers[${index}].Value`;
        });
    }

    // Modal Management
    showModal(isEdit = false) {
        const modal = document.getElementById('functionModal');
        if (!modal) return;

        const form = modal.querySelector('#functionForm');
        if (form) {
            form.reset();
            form.action = isEdit ? '?handler=Update' : '?handler=Create';
            document.getElementById('headersContainer').innerHTML = '';
        }

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
            const response = await fetch(`?handler=Function&functionId=${functionId}`, {
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
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

        form.reset();
        document.getElementById('headersContainer').innerHTML = '';

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
                }
            }
        });

        if (data.headers && typeof data.headers === 'object') {
            Object.entries(data.headers).forEach(([key, value]) => {
                this.addHeaderRow();
                const lastRow = document.querySelector('#headersContainer .input-group:last-child');
                if (lastRow) {
                    const keyInput = lastRow.querySelector('input[name$="].Key"]');
                    const valueInput = lastRow.querySelector('input[name$="].Value"]');
                    if (keyInput) keyInput.value = key;
                    if (valueInput) valueInput.value = value;
                }
            });
        }
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
            await this.refreshTable();
        } catch (error) {
            console.error('Error:', error);
            alert('Failed to update function status. Please try again.');
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
            await this.refreshTable();
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

            const newTable = doc.querySelector('.table-responsive');
            const currentTable = document.querySelector('.table-responsive');
            if (newTable && currentTable) {
                currentTable.innerHTML = newTable.innerHTML;
            }

            const newStats = doc.querySelectorAll('.card-animate');
            document.querySelectorAll('.card-animate').forEach((card, index) => {
                if (newStats[index]) card.innerHTML = newStats[index].innerHTML;
            });

            this.loadAllFunctionStats();
            this.initializeEventListeners();
            this.loadAllExecutionStatuses();
            
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
                    await this.refreshTable();
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
            refreshBtn.removeEventListener('click', () => this.refreshTable);
            refreshBtn.addEventListener('click', () => {
                this.refreshTable();
                this.startAutoRefresh();
                this.loadAllFunctionStats();
                this.startAutoStatsRefresh();
            });
        }

        // Initialize tooltips
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el =>
            new bootstrap.Tooltip(el));
    }
    loadAllExecutionStatuses() {
        document.querySelectorAll('[id^="execution-status-"]').forEach(container => {
            const functionId = container.id.replace('execution-status-', '');
            this.loadFunctionExecutionStatus(functionId);
        });
    }
    async loadFunctionExecutionStatus(functionId) {
        const statusContainer = document.getElementById(`execution-status-${functionId}`);
        if (!statusContainer) return;

        statusContainer.classList.add('status-loading');

        try {
            const response = await fetch(`/game/${gameId}/functions?handler=ExecutionStatus&functionId=${functionId}`, {
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load execution status');
            }

            const status = await response.json();

            if (status.lastExecutedAt) {
                let html = `<small class="text-muted">${new Date(status.lastExecutedAt).toLocaleString('en-US', {
                    month: 'short',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                })}</small>`;

                if (status.lastErrorMessage) {
                    html += `
                    <div class="mt-1">
                        <span class="badge bg-danger-subtle text-danger" data-bs-toggle="tooltip" 
                              title="${status.lastErrorMessage}">
                            <i class="ri-error-warning-line me-1"></i> Error
                        </span>
                    </div>`;
                }
                statusContainer.innerHTML = html;

                // Reinitialize tooltip
                const tooltip = statusContainer.querySelector('[data-bs-toggle="tooltip"]');
                if (tooltip) {
                    new bootstrap.Tooltip(tooltip);
                }
            } else {
                statusContainer.innerHTML = '<span class="text-muted">Never executed</span>';
            }

        } catch (error) {
            console.error(`Error loading execution status for function ${functionId}:`, error);
            statusContainer.innerHTML = '<span class="text-muted">Error loading status</span>';
        } finally {
            statusContainer.classList.remove('status-loading');
        }
    }
}

// Add styles for loading state
const style = document.createElement('style');
style.textContent = `
    .stats-loading {
        opacity: 0.6;
        position: relative;
    }

    .stats-loading::after {
        content: "";
        position: absolute;
        inset: 0;
        background: rgba(255, 255, 255, 0.7);
    }
`;
document.head.appendChild(style);

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gameFunctionsManager = new GameFunctionsManager();
});

// Global utility functions
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