class ConfirmationView extends HTMLElement {
    constructor() {
        super();
        this.timeoutDuration = parseInt(this.getAttribute('timeout')) || 5000; // Default 5s timeout
        this.attachShadow({ mode: 'open' });
        this.currentView = 'initial-view';
        this.timeoutId = null;

        // Set up the shadow DOM with slots
        this.shadowRoot.innerHTML = `
      <style>
        .hidden {
          display: none;
        }
      </style>
      <div class="container">
        <div id="initial-view">
          <slot name="initial-content"></slot>
        </div>
        <div id="confirmation-view" class="hidden">
          <div class="confirmation-content">
            <slot name="confirmation-content"></slot>
          </div>
        </div>
      </div>
    `;
    }

    connectedCallback() {
        // Get DOM elements
        this.initialView = this.shadowRoot.querySelector('#initial-view');
        this.confirmationView = this.shadowRoot.querySelector('#confirmation-view');

        // Listen for events on initial-content slot
        this.shadowRoot.querySelector('slot[name="initial-content"]').addEventListener('slotchange', () => {
            this.shadowRoot.querySelector('slot[name="initial-content"]').assignedNodes().forEach(node => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    node.addEventListener('click', () => this.switchToConfirmationView());
                }
            });
        });

        // Listen for confirm or reject actions in confirmation-content slot
        this.shadowRoot.querySelector('slot[name="confirmation-content"]').addEventListener('slotchange', () => {
            this.shadowRoot.querySelector('slot[name="confirmation-content"]').assignedNodes().forEach(node => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    node.querySelectorAll('[data-action]').forEach(element => {
                        element.addEventListener('click', () => {
                            const action = element.dataset.action;
                            if (action === 'confirm') {
                                this.handleConfirmation();
                            } else if (action === 'reject') {
                                this.handleRejection();
                            }
                        });
                    });
                }
            });
        });
    }

    switchToConfirmationView() {
        this.currentView = 'confirmation-view';
        this.initialView.classList.add('hidden');
        this.confirmationView.classList.remove('hidden');
        this.startTimeout();
    }

    switchToInitialView() {
        this.currentView = 'initial-view';
        this.confirmationView.classList.add('hidden');
        this.initialView.classList.remove('hidden');
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
            this.timeoutId = null;
        }
    }

    startTimeout() {
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
        }
        this.timeoutId = setTimeout(() => {
            this.switchToInitialView();
        }, this.timeoutDuration);
    }

    handleConfirmation() {
        // Dispatch custom event for confirmation
        this.dispatchEvent(new CustomEvent('confirmed', { bubbles: true }));
        this.switchToInitialView();
    }

    handleRejection() {
        // Dispatch custom event for rejection
        this.dispatchEvent(new CustomEvent('rejected', { bubbles: true }));
        this.switchToInitialView();
    }

    disconnectedCallback() {
        // Clean up timeout
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
        }
    }
}

// Define the custom element
customElements.define('confirmation-view', ConfirmationView);