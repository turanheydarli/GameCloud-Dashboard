// Initialize form validation
document.addEventListener('DOMContentLoaded', function() {
    initializeFormValidation();
    initializeEventListeners();
    refreshQueueStats();
});

function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');

    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}

function initializeEventListeners() {
    // Queue toggle switches
    document.querySelectorAll('input[type="checkbox"][role="switch"]').forEach(toggle => {
        toggle.addEventListener('change', async (e) => {
            const queueId = e.target.id.split('-')[1];
            await toggleQueue(queueId, e.target.checked);
        });
    });

    // Edit queue buttons
    document.querySelectorAll('.edit-queue').forEach(button => {
        button.addEventListener('click', async (e) => {
            const queueId = e.target.dataset.id;
            await loadQueueDetails(queueId);
        });
    });

    // Delete queue buttons
    document.querySelectorAll('.delete-queue').forEach(button => {
        button.addEventListener('click', (e) => {
            const queueId = e.target.dataset.id;
            showDeleteConfirmation(queueId);
        });
    });

    // Save queue button
    document.getElementById('saveQueue').addEventListener('click', async () => {
        if (validateQueueForm()) {
            await saveQueue();
        }
    });

    // Delete confirmation button
    document.getElementById('deleteQueueBtn').addEventListener('click', async () => {
        const queueId = document.getElementById('deleteQueueBtn').dataset.queueId;
        await deleteQueue(queueId);
    });
}

async function toggleQueue(queueId, enabled) {
    try {
        const response = await fetch(`/api/v1/matchmaking/queues/${queueId}/toggle`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ enabled })
        });

        if (!response.ok) {
            throw new Error('Failed to toggle queue');
        }

        showToast(enabled ? 'Queue enabled successfully' : 'Queue disabled successfully', 'success');
    } catch (error) {
        console.error('Error toggling queue:', error);
        showToast('Failed to toggle queue status', 'error');
        // Revert toggle state
        const toggle = document.querySelector(`#queue-${queueId}`);
        if (toggle) toggle.checked = !enabled;
    }
}

async function loadQueueDetails(queueId) {
    try {
        const response = await fetch(`/api/v1/matchmaking/queues/${queueId}`);
        if (!response.ok) throw new Error('Failed to load queue details');

        const queue = await response.json();
        populateQueueForm(queue);

        const modal = new bootstrap.Modal(document.getElementById('createQueueModal'));
        modal.show();
    } catch (error) {
        console.error('Error loading queue details:', error);
        showToast('Failed to load queue details', 'error');
    }
}

function populateQueueForm(queue) {
    const form = document.getElementById('queueForm');
    form.querySelector('#queueId').value = queue.id;

    // Populate basic info
    form.querySelector('[name="name"]').value = queue.name;
    form.querySelector('[name="type"]').value = queue.type;
    form.querySelector('[name="description"]').value = queue.description;

    // Populate match configuration
    form.querySelector('[name="playersPerTeam"]').value = queue.playersPerTeam;
    form.querySelector('[name="teamsPerMatch"]').value = queue.teamsPerMatch;

    // Populate matchmaking rules
    form.querySelector('[name="maxWaitTime"]').value = queue.maxWaitTime;
    form.querySelector('[name="skillDifference"]').value = queue.skillDifference;
    form.querySelector('[name="regionPreference"]').value = queue.regionPreference;
    form.querySelector('[name="matchStartBehavior"]').value = queue.matchStartBehavior;

    // Populate team balancing
    form.querySelector('[name="teamFormation"]').value = queue.teamFormation;
    form.querySelector('[name="partyHandling"]').value = queue.partyHandling;

    // Populate switches
    form.querySelector('[name="allowParties"]').checked = queue.allowParties;
    form.querySelector('[name="backfillEnabled"]').checked = queue.backfillEnabled;
    form.querySelector('[name="autoBalance"]').checked = queue.autoBalance;
    form.querySelector('[name="enabled"]').checked = queue.enabled;
}

function validateQueueForm() {
    const form = document.getElementById('queueForm');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return false;
    }
    return true;
}

async function saveQueue() {
    const form = document.getElementById('queueForm');
    const formData = new FormData(form);
    const queueData = Object.fromEntries(formData.entries());

    const url = queueData.id ?
        `/api/v1/matchmaking/queues/${queueData.id}` :
        '/api/v1/matchmaking/queues';

    const method = queueData.id ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                ...queueData,
                // Convert string values to proper types
                playersPerTeam: parseInt(queueData.playersPerTeam),
                teamsPerMatch: parseInt(queueData.teamsPerMatch),
                maxWaitTime: parseInt(queueData.maxWaitTime),
                skillDifference: parseInt(queueData.skillDifference),
                // Convert checkbox values to booleans
                allowParties: queueData.allowParties === 'on',
                backfillEnabled: queueData.backfillEnabled === 'on',
                autoBalance: queueData.autoBalance === 'on',
                enabled: queueData.enabled === 'on'
            })
        });

        if (!response.ok) {
            throw new Error('Failed to save queue');
        }

        const modal = bootstrap.Modal.getInstance(document.getElementById('createQueueModal'));
        modal.hide();

        // Refresh the queue list
        window.location.reload();

        showToast(queueData.id ? 'Queue updated successfully' : 'Queue created successfully', 'success');
    } catch (error) {
        console.error('Error saving queue:', error);
        showToast('Failed to save queue', 'error');
    }
}

async function deleteQueue(queueId) {
    try {
        const response = await fetch(`/api/v1/matchmaking/queues/${queueId}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error('Failed to delete queue');
        }

        const modal = bootstrap.Modal.getInstance(document.getElementById('deleteQueueModal'));
        modal.hide();

        // Refresh the queue list
        window.location.reload();

        showToast('Queue deleted successfully', 'success');
    } catch (error) {
        console.error('Error deleting queue:', error);
        showToast('Failed to delete queue', 'error');
    }
}

function showDeleteConfirmation(queueId) {
    const deleteBtn = document.getElementById('deleteQueueBtn');
    deleteBtn.dataset.queueId = queueId;

    const modal = new bootstrap.Modal(document.getElementById('deleteQueueModal'));
    modal.show();
}

// Stats updating functions
async function refreshQueueStats() {
    try {
        // Refresh active tickets count for each queue
        const queueRows = document.querySelectorAll('[id^="queue-stats-"]');
        for (const row of queueRows) {
            const queueId = row.id.split('-')[2];
            const stats = await fetchQueueStats(queueId);
            updateQueueStatsRow(queueId, stats);
        }

        // Schedule next update
        setTimeout(refreshQueueStats, 30000); // Update every 30 seconds
    } catch (error) {
        console.error('Error refreshing queue stats:', error);
    }
}

async function fetchQueueStats(queueId) {
    const response = await fetch(`/api/v1/matchmaking/queues/${queueId}/stats`);
    if (!response.ok) throw new Error('Failed to fetch queue stats');
    return await response.json();
}

function updateQueueStatsRow(queueId, stats) {
    const row = document.getElementById(`queue-stats-${queueId}`);
    if (!row) return;

    // Update active tickets
    const ticketsElement = row.querySelector('.active-tickets');
    if (ticketsElement) {
        ticketsElement.textContent = stats.activeTickets;
    }

    // Update wait time
    const waitTimeElement = row.querySelector('.avg-wait-time');
    if (waitTimeElement) {
        waitTimeElement.textContent = `${stats.averageWaitTime.toFixed(1)}s`;
    }
}

// Utility function for showing toasts
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        // Create toast container if it doesn't exist
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type}`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    // Remove toast after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}