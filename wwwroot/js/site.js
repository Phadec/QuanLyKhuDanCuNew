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
    
    // Set loading state when form is submitted
    $('form').on('submit', function() {
        const submitButton = $(this).find('button[type="submit"]');
        setLoading(submitButton[0], true);
    });
});
