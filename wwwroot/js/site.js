// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Common JavaScript functions for the application

// Handle form validation visual feedback
(function () {
    'use strict';
    window.addEventListener('load', function () {
        // Fetch all forms we want to apply custom validation styles to
        const forms = document.querySelectorAll('.needs-validation');
        
        // Loop over them and prevent submission
        Array.prototype.filter.call(forms, function (form) {
            form.addEventListener('submit', function (event) {
                if (form.checkValidity() === false) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }, false);
})();

// Auto-hide alerts after 5 seconds
$(document).ready(function () {
    setTimeout(function () {
        $('.alert-dismissible').alert('close');
    }, 5000);
});

// Enable tooltips everywhere
$(function () {
    $('[data-toggle="tooltip"]').tooltip();
});

// Enable popovers
$(function () {
    $('[data-toggle="popover"]').popover();
});

// Handle dynamic form fields
function addFormField(containerId, templateId) {
    const container = document.getElementById(containerId);
    const template = document.getElementById(templateId);
    const clone = template.content.cloneNode(true);
    
    // Update the indices in the field names
    const index = container.getElementsByClassName('form-group').length;
    const inputs = clone.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        const name = input.getAttribute('name');
        if (name) {
            input.setAttribute('name', name.replace('__index__', index));
            input.setAttribute('id', name.replace('__index__', index));
        }
    });
    
    const labels = clone.querySelectorAll('label');
    labels.forEach(label => {
        const forAttr = label.getAttribute('for');
        if (forAttr) {
            label.setAttribute('for', forAttr.replace('__index__', index));
        }
    });
    
    container.appendChild(clone);
}

// Handle dynamic removal of form fields
function removeFormField(button) {
    const formGroup = button.closest('.form-group');
    formGroup.remove();
}

// Handle date pickers
$(document).ready(function() {
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        todayHighlight: true,
        language: 'vi'
    });
});

// Format currency inputs
$(document).ready(function() {
    $('.currency-input').on('input', function() {
        const value = $(this).val().replace(/,/g, '');
        if (value !== '') {
            const formattedValue = new Intl.NumberFormat('vi-VN').format(value);
            $(this).val(formattedValue);
        }
    });
});

// Handle confirmation dialogs
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
    return false;
}

// Function to handle loading states on buttons
function setLoading(button, isLoading) {
    if (isLoading) {
        button.setAttribute('disabled', 'disabled');
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...';
    } else {
        button.removeAttribute('disabled');
        button.innerHTML = button.getAttribute('data-original-text');
    }
}

// Save original button text when page loads
$(document).ready(function() {
    $('button[type="submit"]').each(function() {
        $(this).attr('data-original-text', $(this).html());
    });
    
    // Enhanced form submission handling
    $('form').on('submit', function(e) {
        const form = this;
        const submitButton = $(form).find('button[type="submit"]');
        
        // Check if form is valid before showing loading state
        if (form.checkValidity && !form.checkValidity()) {
            // Form is invalid, don't show loading state
            return;
        }
        
        // Check jQuery validation if available
        if (typeof $.fn.valid === 'function' && $(form).valid && !$(form).valid()) {
            // jQuery validation failed, don't show loading state
            return;
        }
        
        // Form appears valid, show loading state
        setLoading(submitButton[0], true);
        
        // Set a timeout to reset button if page doesn't redirect (validation errors)
        setTimeout(function() {
            if (submitButton.length > 0 && submitButton.is(':disabled')) {
                setLoading(submitButton[0], false);
            }
        }, 3000);
    });
    
    // Reset button state when validation errors are shown
    $(document).on('DOMNodeInserted', function(e) {
        const target = $(e.target);
        if (target.hasClass('validation-summary-errors') || 
            target.find('.validation-summary-errors').length > 0 ||
            target.hasClass('field-validation-error') ||
            target.find('.field-validation-error').length > 0) {
            
            // Reset all submit buttons on the page
            $('button[type="submit"]:disabled').each(function() {
                setLoading(this, false);
            });
        }
    });
});

// Export functionality with loading states and success feedback
document.addEventListener('DOMContentLoaded', function() {
    // Handle export button clicks
    const exportButtons = document.querySelectorAll('.export-btn');
    
    exportButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Add loading state
            this.classList.add('loading');
            this.style.pointerEvents = 'none';
            
            const exportType = this.dataset.type === 'excel' ? 'Excel' : 'PDF';
            const originalText = this.innerHTML;
            
            // Show loading message
            this.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i>Đang xuất ${exportType}...`;
            
            // Perform the actual download
            const downloadUrl = this.href;
            
            // Create a temporary link for download
            const tempLink = document.createElement('a');
            tempLink.href = downloadUrl;
            tempLink.style.display = 'none';
            document.body.appendChild(tempLink);
            tempLink.click();
            document.body.removeChild(tempLink);
            
            // Reset button after delay
            setTimeout(() => {
                this.classList.remove('loading');
                this.classList.add('export-success');
                this.innerHTML = `<i class="fas fa-check me-2"></i>Đã xuất ${exportType}!`;
                this.style.pointerEvents = 'auto';
                
                // Reset to original state
                setTimeout(() => {
                    this.classList.remove('export-success');
                    this.innerHTML = originalText;
                }, 2000);
            }, 1500);
        });
    });
});

// Enhanced chart animations
if (typeof Chart !== 'undefined') {
    Chart.defaults.animation.duration = 2000;
    Chart.defaults.animation.easing = 'easeInOutQuart';
}
