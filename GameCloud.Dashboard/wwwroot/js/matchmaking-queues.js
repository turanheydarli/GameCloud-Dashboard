let queueWizard;
let rulesEditor;

document.addEventListener('DOMContentLoaded', function () {
    queueWizard = new Stepper(document.querySelector('#queueWizard'), {
        linear: true,
        animation: true
    });

    rulesEditor = ace.edit("rulesEditor");
    rulesEditor.setTheme("ace/theme/monokai");
    rulesEditor.session.setMode("ace/mode/json");
    rulesEditor.setValue(JSON.stringify({
        "matchCriteria": {
            "skillRange": 100,
            "maxWaitTimeSeconds": 60
        },
        "teamConfig": {
            "teamsCount": 2,
            "playersPerTeam": 1
        },
        "turnConfig": {
            "firstTurnAssignment": "random",
            "turnNotificationType": "push"
        }
    }, null, 2));

    const modalElement = document.getElementById('createQueueModal');
    const modal = new bootstrap.Modal(modalElement);

    modalElement.addEventListener('shown.bs.modal', function () {
        // Reset stepper to first step when modal opens
        if (queueWizard) {
            queueWizard.to(0);
        }

        // Force content visibility
        const contentElements = document.querySelectorAll('.bs-stepper-content .content');
        contentElements.forEach(element => {
            if (element.id === 'queue-details') {
                element.classList.add('active', 'dblock');
            } else {
                element.classList.remove('active', 'dblock');
            }
        });

        // Force stepper content repaint
        const stepperContent = document.querySelector('.bs-stepper-content');
        if (stepperContent) {
            stepperContent.style.display = 'none';
            requestAnimationFrame(() => {
                stepperContent.style.display = 'block';
            });
        }
    });

    // Add modal backdrop fix
    modalElement.addEventListener('hide.bs.modal', function () {
        document.body.classList.remove('modal-open');
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) {
            backdrop.remove();
        }
    });

    // Set up queue type change handler
    const queueTypeSelect = document.getElementById('queueTypeSelect');
    if (queueTypeSelect) {
        queueTypeSelect.addEventListener('change', updateQueueTypeInfo);
        updateQueueTypeInfo(); // Initialize with default value
    }

    // Set up min/max players validation
    const minPlayers = document.getElementById('minPlayers');
    const maxPlayers = document.getElementById('maxPlayers');
    if (minPlayers && maxPlayers) {
        minPlayers.addEventListener('change', validatePlayerCounts);
        maxPlayers.addEventListener('change', validatePlayerCounts);
    }

    // Initialize tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0) {
        Array.from(tooltipTriggerList).map(tooltipTriggerEl =>
            new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // Load queue stats if there are queues
    loadQueueStats();
});

// Form validation functions
function validateAndGoNext(step) {
    // Store form data in each step
    if (step === 0) {
        if (!validateBasicInfo()) return;
    } else if (step === 1) {
        if (!validateMatchConfig()) return;
    }

    queueWizard.next();
}

function validateBasicInfo() {
    const queueName = document.getElementById('queueName');
    const minPlayers = document.getElementById('minPlayers');
    const maxPlayers = document.getElementById('maxPlayers');

    // Queue name validation - alphanumeric with underscores
    const nameRegex = /^[a-zA-Z0-9_]+$/;
    if (!nameRegex.test(queueName.value)) {
        queueName.classList.add('is-invalid');
        return false;
    } else {
        queueName.classList.remove('is-invalid');
    }

    // Player count validation
    if (parseInt(minPlayers.value) > parseInt(maxPlayers.value)) {
        maxPlayers.classList.add('is-invalid');
        return false;
    } else {
        maxPlayers.classList.remove('is-invalid');
    }

    return true;
}

function validatePlayerCounts() {
    const minPlayers = document.getElementById('minPlayers');
    const maxPlayers = document.getElementById('maxPlayers');
    const playerCountInfo = document.getElementById('playerCountInfo');

    if (parseInt(minPlayers.value) > parseInt(maxPlayers.value)) {
        maxPlayers.classList.add('is-invalid');
        playerCountInfo.innerHTML = '<span class="text-danger">Maximum players must be greater than or equal to minimum players</span>';
    } else {
        maxPlayers.classList.remove('is-invalid');
        playerCountInfo.innerHTML = 'Matches will start when min players join and support up to max players.';
    }
}

function validateMatchConfig() {
    const ticketTTL = document.getElementById('ticketTTL');
    const turnTimeout = document.getElementById('turnTimeout');
    const matchTimeout = document.getElementById('matchTimeout');

    // Validate JSON in rules editor
    try {
        const rules = JSON.parse(rulesEditor.getValue());
        // If we got here, JSON is valid
        document.getElementById('rulesInput').value = rulesEditor.getValue();
        return true;
    } catch (e) {
        alert('Invalid JSON in matchmaking rules. Please correct the format.');
        return false;
    }
}

function validateQueueForm() {
    // Prevent default form submission
    event.preventDefault();

    // Final validation before submission
    if (!validateBasicInfo()) {
        queueWizard.to(0);
        return false;
    }

    if (!validateMatchConfig()) {
        queueWizard.to(1);
        return false;
    }

    // Validate required functions
    const initializeFunction = document.getElementById('initializeFunction');
    const transitionFunction = document.getElementById('transitionFunction');

    if (!initializeFunction.value || !transitionFunction.value) {
        alert('Match initialization and transition functions are required.');
        return false;
    }

    // All validations passed
    const submitButton = document.getElementById('submitButton');
    submitButton.disabled = true;
    submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...';

    // Get form data
    const form = document.getElementById('queueForm');
    const formData = new FormData(form);

    const requestData = {
        Id: formData.get('Id'),
        GameId: formData.get('GameId'),
        Name: formData.get('Name'),
        Description: formData.get('Description'),
        QueueType: formData.get('QueueType'),
        MinPlayers: parseInt(formData.get('MinPlayers')),
        MaxPlayers: parseInt(formData.get('MaxPlayers')),
        IsEnabled: formData.get('IsEnabled') === 'on',
        TicketTTLSeconds: parseInt(formData.get('TicketTTLSeconds')),
        TurnTimeoutSeconds: parseInt(formData.get('TurnTimeoutSeconds')),
        MatchTimeoutSeconds: parseInt(formData.get('MatchTimeoutSeconds')),
        InitializeFunctionId: formData.get('InitializeFunctionId'),
        TransitionFunctionId: formData.get('TransitionFunctionId'),
        LeaveFunctionId: formData.get('LeaveFunctionId'),
        EndFunctionId: formData.get('EndFunctionId'),
        Rules: JSON.parse(rulesEditor.getValue() || '{}')
    };

    fetch(form.action || window.location.href, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': formData.get('__RequestVerificationToken'),
            'X-Game-Key': formData.get('GameKey')
        },
        body: JSON.stringify(requestData)
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to create queue');
            return response.json();
        })
        .then(data => {
            window.location.reload();
        })
        .catch(error => {
            console.error('Error creating queue:', error);
            submitButton.disabled = false;
            submitButton.innerHTML = '<i class="ri-check-line me-1"></i> Create Queue';
            showToast('Failed to create queue', 'danger');
        });

    return false;
}

// Queue type related functions
function updateQueueTypeInfo() {
    const queueType = document.getElementById('queueTypeSelect').value;
    const queueTypeIcon = document.getElementById('queueTypeIcon');
    const queueTypeDescription = document.getElementById('queueTypeDescription');
    const queueTypeSummary = document.getElementById('queueTypeSummary');

    // Update icon
    queueTypeIcon.className = GetQueueTypeIcon(queueType);

    // Update description
    switch (queueType) {
        case 'TurnBased':
            queueTypeDescription.innerHTML = 'Players take turns making moves. Ideal for strategy games and board games.';
            queueTypeSummary.innerHTML = `
                <h6>Turn-Based Queue</h6>
                <p class="mb-0">Players take actions in sequence. Each player receives a notification when it's their turn to act. 
                Matches can last for extended periods as players may not be online simultaneously.</p>
            `;
            break;
        case 'Simultaneous':
            queueTypeDescription.innerHTML = 'All players act at the same time. Best for real-time competitive games.';
            queueTypeSummary.innerHTML = `
                <h6>Simultaneous Queue</h6>
                <p class="mb-0">All players must be online at the same time. Actions are processed in real-time.
                Ideal for fast-paced gameplay where all participants play concurrently.</p>
            `;
            break;
        case 'Asynchronous':
            queueTypeDescription.innerHTML = 'Players can join and leave at different times. Good for casual games.';
            queueTypeSummary.innerHTML = `
                <h6>Asynchronous Queue</h6>
                <p class="mb-0">Players can participate on their own schedule. The game state persists between sessions,
                allowing for long-running matches with minimal time pressure.</p>
            `;
            break;
    }

    // Update match functions info
    updateMatchFunctionsInfo(queueType);

    // Update rules template
    updateRulesTemplate();
}

function updateMatchFunctionsInfo(queueType) {
    const matchFunctionsInfo = document.getElementById('matchFunctionsInfo');

    switch (queueType) {
        case 'TurnBased':
            matchFunctionsInfo.innerHTML = 'Connect functions to manage turn-based gameplay. Your initialize function should set up the game board and turn order.';
            break;
        case 'Simultaneous':
            matchFunctionsInfo.innerHTML = 'Connect functions for real-time gameplay. Your transition function should handle concurrent player actions and resolve conflicts.';
            break;
        case 'Asynchronous':
            matchFunctionsInfo.innerHTML = 'Connect functions that support disconnection and reconnection. Your functions should maintain game state between sessions.';
            break;
    }
}

function updateRulesTemplate() {
    const queueType = document.getElementById('queueTypeSelect').value;
    let defaultRules = {};

    switch (queueType) {
        case 'TurnBased':
            defaultRules = {
                "matchCriteria": {
                    "skillRange": 100,
                    "maxWaitTimeSeconds": 60
                },
                "teamConfig": {
                    "teamsCount": 2,
                    "playersPerTeam": 1
                },
                "turnConfig": {
                    "firstTurnAssignment": "random",
                    "turnNotificationType": "push"
                }
            };
            break;
        case 'Simultaneous':
            defaultRules = {
                "matchCriteria": {
                    "skillRange": 150,
                    "regionPriority": ["na-east", "na-west", "eu"]
                },
                "teamConfig": {
                    "teamsCount": 2,
                    "playersPerTeam": 2,
                    "balanceSkill": true
                }
            };
            break;
        case 'Asynchronous':
            defaultRules = {
                "matchCriteria": {
                    "maxWaitTimeSeconds": 1800,
                    "skillRange": 200
                },
                "teamConfig": {
                    "teamsCount": 2,
                    "playersPerTeam": 1
                },
                "asyncConfig": {
                    "inactivityTimeoutHours": 48,
                    "remindersEnabled": true
                }
            };
            break;
    }

    rulesEditor.setValue(JSON.stringify(defaultRules, null, 2));
}

function formatJson() {
    try {
        const rules = JSON.parse(rulesEditor.getValue());
        rulesEditor.setValue(JSON.stringify(rules, null, 2));
    } catch (e) {
        alert('Invalid JSON. Cannot format.');
    }
}

// API interaction functions
function loadQueueStats() {
    const queueElements = document.querySelectorAll('[id^="active-matches-"]');
    const queueIds = Array.from(queueElements).map(el =>
        el.id.replace('active-matches-', '')
    );

    if (queueIds.length === 0) return;

    const gameId = document.querySelector('input[name="GameId"]').value;

    fetch(`/api/games/${gameId}/matchmaking/stats?queueIds=${queueIds.join(',')}`)
        .then(response => {
            if (!response.ok) throw new Error('Failed to fetch queue stats');
            return response.json();
        })
        .then(data => {
            data.forEach(stat => {
                const queueId = stat.queueId;
                // Update queue stats
                const activeMatches = document.getElementById(`active-matches-${queueId}`);
                const waitingPlayers = document.getElementById(`waiting-players-${queueId}`);
                const avgWait = document.getElementById(`avg-wait-${queueId}`);

                if (activeMatches) activeMatches.textContent = stat.activeMatches || 0;
                if (waitingPlayers) waitingPlayers.textContent = stat.waitingPlayers || 0;
                if (avgWait) avgWait.textContent = stat.avgWaitTimeSeconds ?
                    `${stat.avgWaitTimeSeconds.toFixed(1)}s` : '0.0s';
            });
        })
        .catch(error => {
            console.error('Error loading queue stats:', error);
            // Set fallback values for all stats elements
            queueElements.forEach(el => {
                const queueId = el.id.replace('active-matches-', '');
                document.getElementById(`active-matches-${queueId}`).textContent = '0';
                document.getElementById(`waiting-players-${queueId}`).textContent = '0';
                document.getElementById(`avg-wait-${queueId}`).textContent = '0.0s';
            });
        });

    // Update stats every 30 seconds
    setTimeout(loadQueueStats, 30000);
}

// Toggle queue active state
document.addEventListener('change', function (e) {
    if (e.target.dataset.action === 'toggle-queue') {
        const queueId = e.target.dataset.queueId;
        const isEnabled = e.target.checked;
        const originalState = !isEnabled;
        const gameId = document.querySelector('input[name="GameId"]').value;

        fetch(`/api/games/${gameId}/matchmaking/queues/${queueId}/toggle`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({isEnabled})
        })
            .then(response => {
                if (!response.ok) throw new Error('Failed to toggle queue state');
                return response.json();
            })
            .then(data => {
                // Show toast notification
                showToast(`Queue ${isEnabled ? 'enabled' : 'disabled'} successfully`, 'success');
            })
            .catch(error => {
                console.error('Error toggling queue:', error);
                // Revert checkbox state on error
                e.target.checked = originalState;
                showToast('Failed to update queue status', 'danger');
            });
    }
});

// Edit queue handler
document.addEventListener('click', function (e) {
    if (e.target.dataset.action === 'edit' || e.target.parentElement && e.target.parentElement.dataset.action === 'edit') {
        const element = e.target.dataset.action ? e.target : e.target.parentElement;
        const queueId = element.dataset.queueId;
        const gameId = document.querySelector('input[name="GameId"]').value;

        // Update modal title
        document.getElementById('queueModalTitle').textContent = 'Edit Matchmaking Queue';
        document.getElementById('submitButton').innerHTML = '<i class="ri-save-line me-1"></i> Update Queue';

        // Load queue data
        fetch(`/api/games/${gameId}/matchmaking/queues/${queueId}`)
            .then(response => {
                if (!response.ok) throw new Error('Failed to load queue data');
                return response.json();
            })
            .then(queue => {
                // Update form fields
                document.getElementById('queueId').value = queue.id;
                document.getElementById('queueName').value = queue.name;
                document.getElementById('queueDescription').value = queue.description || '';
                document.getElementById('queueTypeSelect').value = queue.queueType;
                document.getElementById('minPlayers').value = queue.minPlayers;
                document.getElementById('maxPlayers').value = queue.maxPlayers;
                document.getElementById('queueEnabled').checked = queue.isEnabled;

                // Update timeouts
                document.getElementById('ticketTTL').value = queue.ticketTTL || 300;
                document.getElementById('turnTimeout').value = queue.turnTimeoutSeconds || 86400;
                document.getElementById('matchTimeout').value = queue.matchTimeoutSeconds || 604800;

                // Update rules
                rulesEditor.setValue(JSON.stringify(queue.rules || {}, null, 2));

                // Update functions
                if (queue.initializeFunctionId) {
                    document.getElementById('initializeFunction').value = queue.initializeFunctionId;
                }
                if (queue.transitionFunctionId) {
                    document.getElementById('transitionFunction').value = queue.transitionFunctionId;
                }
                if (queue.leaveFunctionId) {
                    document.getElementById('leaveFunction').value = queue.leaveFunctionId;
                }
                if (queue.endFunctionId) {
                    document.getElementById('endFunction').value = queue.endFunctionId;
                }

                // Update queue type display
                updateQueueTypeInfo();

                // Change form submission URL
                document.getElementById('queueForm').action = '?handler=Update';

                // Show modal
                queueWizard.to(0);
                new bootstrap.Modal(document.getElementById('createQueueModal')).show();
            })
            .catch(error => {
                console.error('Error loading queue details:', error);
                showToast('Failed to load queue details', 'danger');
            });
    }
});

// Delete queue handler
document.addEventListener('click', function (e) {
    if (e.target.dataset.action === 'delete' || e.target.parentElement && e.target.parentElement.dataset.action === 'delete') {
        const element = e.target.dataset.action ? e.target : e.target.parentElement;
        const queueId = element.dataset.queueId;
        const queueName = element.dataset.queueName;
        const gameId = document.querySelector('input[name="GameId"]').value;

        if (confirm(`Are you sure you want to delete the queue "${queueName}"? This action cannot be undone.`)) {
            fetch(`/api/games/${gameId}/matchmaking/queues/${queueId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            })
                .then(response => {
                    if (!response.ok) throw new Error('Failed to delete queue');
                    return response.json();
                })
                .then(data => {
                    showToast('Queue deleted successfully', 'success');
                    // Reload page after short delay
                    setTimeout(() => window.location.reload(), 1000);
                })
                .catch(error => {
                    console.error('Error deleting queue:', error);
                    showToast('Failed to delete queue', 'danger');
                });
        }
    }
});

// Helper functions
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();
    const toastId = 'toast-' + Date.now();

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center border-0 bg-${type}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="ri-${type === 'success' ? 'check-double-line' : type === 'danger' ? 'error-warning-line' : 'information-line'} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {autohide: true, delay: 3000});
    toast.show();

    // Remove toast after it's hidden
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = "1090";
    document.body.appendChild(container);
    return container;
}

// Utility functions referenced in HTML
function GetQueueTypeName(queueType) {
    switch (queueType) {
        case 'TurnBased':
            return 'Turn-Based';
        case 'Simultaneous':
            return 'Simultaneous';
        case 'Asynchronous':
            return 'Asynchronous';
        default:
            return queueType;
    }
}

function GetQueueTypeColor(queueType) {
    switch (queueType) {
        case 'TurnBased':
            return 'primary';
        case 'Simultaneous':
            return 'success';
        case 'Asynchronous':
            return 'info';
        default:
            return 'secondary';
    }
}

function GetQueueTypeIcon(queueType) {
    switch (queueType) {
        case 'TurnBased':
            return 'ri-clockwise-line';
        case 'Simultaneous':
            return 'ri-group-line';
        case 'Asynchronous':
            return 'ri-time-line';
        default:
            return 'ri-gamepad-line';
    }
}

function FormatTimeSpan(timeSpan) {
    if (!timeSpan) return 'None';

    const totalSeconds = timeSpan.totalSeconds || 0;
    if (totalSeconds === 0) return 'None';

    if (totalSeconds < 60) return `${totalSeconds}s`;
    if (totalSeconds < 3600) return `${Math.floor(totalSeconds / 60)}m`;
    if (totalSeconds < 86400) return `${Math.floor(totalSeconds / 3600)}h`;
    return `${Math.floor(totalSeconds / 86400)}d`;
}