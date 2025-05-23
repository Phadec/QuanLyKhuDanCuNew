// Initialization for common UI components
document.addEventListener('DOMContentLoaded', function() {
    // Initialize dropdowns
    const dropdownToggleList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    if (typeof bootstrap !== 'undefined') {
        dropdownToggleList.map(function (dropdownToggle) {
            return new bootstrap.Dropdown(dropdownToggle);
        });
    }

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    if (typeof bootstrap !== 'undefined') {
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Add any additional global initializers here
});

// Helper function to show/hide password
function togglePasswordVisibility(inputId, toggleButtonId) {
    const passwordInput = document.getElementById(inputId);
    const toggleButton = document.getElementById(toggleButtonId);
    
    if (passwordInput && toggleButton) {
        toggleButton.addEventListener('click', function() {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);
            
            // Toggle icon
            this.querySelector('i').classList.toggle('fa-eye');
            this.querySelector('i').classList.toggle('fa-eye-slash');
        });
    }
}

// Allow for custom file input styling
document.addEventListener('DOMContentLoaded', function() {
    const customFileInputs = document.querySelectorAll('.custom-file-input');
    
    customFileInputs.forEach(input => {
        input.addEventListener('change', function(e) {
            const fileName = this.files[0]?.name || 'Chọn tệp tin';
            const nextSibling = this.nextElementSibling;
            
            if (nextSibling && nextSibling.classList.contains('custom-file-label')) {
                nextSibling.textContent = fileName;
            }
        });
    });
});

// Handle logout via AJAX
function setupLogoutHandlers() {
    const logoutLinks = document.querySelectorAll('.ajax-logout');
    if (logoutLinks.length > 0) {
        logoutLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Get the anti-forgery token
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                
                fetch('/Account/LogoutAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    credentials: 'include'
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        window.location.href = '/Account/Login';
                    }
                })
                .catch(error => {
                    console.error('Logout error:', error);
                    // Fallback to regular form submission
                    document.getElementById('logoutForm').submit();
                });
            });
        });
    }
}

// Initialize when document loads
document.addEventListener('DOMContentLoaded', function() {
    // Toggle sidebar
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }
    
    // Setup logout handlers
    setupLogoutHandlers();
    
    // Other existing initialization code...
});
