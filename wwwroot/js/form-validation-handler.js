// Enhanced form validation and loading state handler
(function() {
    'use strict';
    
    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        initializeFormHandlers();
        initializeValidationObserver();
    });
    
    function initializeFormHandlers() {
        // Handle all forms with enhanced validation
        const forms = document.querySelectorAll('form');
        
        forms.forEach(function(form) {
            // Store original button text
            const submitButtons = form.querySelectorAll('button[type="submit"]');
            submitButtons.forEach(function(button) {
                if (!button.hasAttribute('data-original-text')) {
                    button.setAttribute('data-original-text', button.innerHTML);
                }
            });
            
            // Enhanced form submission handler
            form.addEventListener('submit', function(e) {
                handleFormSubmission(form, e);
            });
            
            // Add real-time validation listeners
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(function(input) {
                input.addEventListener('blur', function() {
                    checkAndResetButtonStates(form);
                });
                
                input.addEventListener('input', function() {
                    // Debounced validation check
                    clearTimeout(input.validationTimeout);
                    input.validationTimeout = setTimeout(function() {
                        checkAndResetButtonStates(form);
                    }, 300);
                });
            });
        });
    }
    
    function handleFormSubmission(form, event) {
        const submitButtons = form.querySelectorAll('button[type="submit"]');
        
        // Check HTML5 validation first
        if (form.checkValidity && !form.checkValidity()) {
            resetButtonStates(submitButtons);
            return;
        }
        
        // Check jQuery validation if available
        if (window.jQuery && typeof jQuery.fn.valid === 'function') {
            const $form = jQuery(form);
            if ($form.valid && !$form.valid()) {
                resetButtonStates(submitButtons);
                return;
            }
        }
        
        // Check for required fields manually
        const requiredFields = form.querySelectorAll('[required]');
        let hasEmptyRequired = false;
        
        requiredFields.forEach(function(field) {
            if (!field.value.trim()) {
                hasEmptyRequired = true;
                // Highlight the field
                field.classList.add('is-invalid');
            } else {
                field.classList.remove('is-invalid');
            }
        });
        
        if (hasEmptyRequired) {
            resetButtonStates(submitButtons);
            showValidationMessage(form, 'Vui lòng điền đầy đủ các trường bắt buộc.');
            return;
        }
        
        // Form seems valid, show loading state
        submitButtons.forEach(function(button) {
            setButtonLoading(button, true);
        });
        
        // Set timeout to reset buttons in case of server-side validation errors
        setTimeout(function() {
            submitButtons.forEach(function(button) {
                if (button.disabled) {
                    setButtonLoading(button, false);
                }
            });
        }, 5000);
    }
    
    function checkAndResetButtonStates(form) {
        const submitButtons = form.querySelectorAll('button[type="submit"]');
        const hasValidationErrors = form.querySelectorAll('.field-validation-error:not(:empty), .validation-summary-errors').length > 0;
        
        if (hasValidationErrors) {
            resetButtonStates(submitButtons);
        }
    }
    
    function setButtonLoading(button, isLoading) {
        if (isLoading) {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang xử lý...';
            button.classList.add('btn-loading');
        } else {
            button.disabled = false;
            const originalText = button.getAttribute('data-original-text');
            button.innerHTML = originalText || 'Submit';
            button.classList.remove('btn-loading');
        }
    }
    
    function resetButtonStates(buttons) {
        buttons.forEach(function(button) {
            setButtonLoading(button, false);
        });
    }
    
    function showValidationMessage(form, message) {
        // Remove existing validation messages
        const existingMessages = form.querySelectorAll('.custom-validation-message');
        existingMessages.forEach(function(msg) {
            msg.remove();
        });
        
        // Create new validation message
        const validationDiv = document.createElement('div');
        validationDiv.className = 'alert alert-danger custom-validation-message mt-2';
        validationDiv.innerHTML = '<i class="fas fa-exclamation-triangle me-1"></i>' + message;
        
        // Insert at the top of the form
        form.insertBefore(validationDiv, form.firstChild);
        
        // Auto-hide after 5 seconds
        setTimeout(function() {
            if (validationDiv.parentNode) {
                validationDiv.remove();
            }
        }, 5000);
    }
    
    function initializeValidationObserver() {
        // Modern approach using MutationObserver
        if (window.MutationObserver) {
            const observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                    mutation.addedNodes.forEach(function(node) {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            // Check if validation errors were added
                            if (node.classList && (
                                node.classList.contains('field-validation-error') ||
                                node.classList.contains('validation-summary-errors') ||
                                node.querySelector('.field-validation-error') ||
                                node.querySelector('.validation-summary-errors')
                            )) {
                                // Reset all loading buttons
                                const loadingButtons = document.querySelectorAll('button[type="submit"].btn-loading');
                                resetButtonStates(loadingButtons);
                            }
                        }
                    });
                    
                    // Check for text content changes in validation spans
                    if (mutation.type === 'childList' || mutation.type === 'characterData') {
                        const target = mutation.target;
                        if (target.classList && target.classList.contains('field-validation-error') && target.textContent.trim()) {
                            const form = target.closest('form');
                            if (form) {
                                const submitButtons = form.querySelectorAll('button[type="submit"]');
                                resetButtonStates(submitButtons);
                            }
                        }
                    }
                });
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true,
                characterData: true
            });
        }
    }
    
    // Expose functions globally if needed
    window.FormValidationHandler = {
        setButtonLoading: setButtonLoading,
        resetButtonStates: resetButtonStates,
        checkAndResetButtonStates: checkAndResetButtonStates
    };
    
})();
