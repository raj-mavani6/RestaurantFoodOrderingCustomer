// Cart functions
function updateCartCount() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            const cartCountElement = document.getElementById('cartCount');
            if (cartCountElement) {
                cartCountElement.textContent = data.count;
            }
        })
        .catch(error => console.error('Error:', error));
}

function addToCart(foodId, quantity = 1) {
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `foodId=${foodId}&quantity=${quantity}`
    })
    .then(response => {
        console.log('Response status:', response.status);
        return response.json();
    })
    .then(data => {
        console.log('Response data:', data);
        if (data.success) {
            showToast('Success', data.message, 'success');
            updateCartCount();
        } else {
            // Redirect to login page if user is not logged in
            if (data.requireLogin === true) {
                console.log('Redirecting to login page...');
                showToast('Info', data.message, 'warning');
                setTimeout(() => {
                    console.log('Executing redirect...');
                    window.location.href = '/Account/Login';
                }, 1500);
            } else {
                showToast('Error', data.message, 'danger');
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Error', 'Failed to add item to cart', 'danger');
    });
}

function updateQuantity(foodId, quantity) {
    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `foodId=${foodId}&quantity=${quantity}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            location.reload();
        }
    })
    .catch(error => console.error('Error:', error));
}

function removeFromCart(foodId) {
    if (confirm('Are you sure you want to remove this item?')) {
        fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `foodId=${foodId}`
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                location.reload();
            }
        })
        .catch(error => console.error('Error:', error));
    }
}

// Toast notification function
function showToast(title, message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    
    if (!toastContainer) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '11';
        document.body.appendChild(container);
    }
    
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <strong>${title}</strong><br>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    document.getElementById('toastContainer').appendChild(toastEl);
    
    const toast = new bootstrap.Toast(toastEl);
    toast.show();
    
    toastEl.addEventListener('hidden.bs.toast', function () {
        toastEl.remove();
    });
}

// Initialize cart count on page load
document.addEventListener('DOMContentLoaded', function() {
    // Only update cart count if cart count element exists (user is logged in)
    const cartCountElement = document.getElementById('cartCount');
    if (cartCountElement) {
        updateCartCount();
    }
});

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return false;
    }
    return true;
}

// Number input increment/decrement
function incrementQuantity(inputId) {
    const input = document.getElementById(inputId);
    input.value = parseInt(input.value) + 1;
}

function decrementQuantity(inputId) {
    const input = document.getElementById(inputId);
    if (parseInt(input.value) > 1) {
        input.value = parseInt(input.value) - 1;
    }
}

// ===================================
// Menu Filter Functions
// ===================================

document.addEventListener('DOMContentLoaded', function() {
    // Clear Search Button
    const clearSearchBtn = document.getElementById('clearSearch');
    const searchInput = document.getElementById('searchInput');
    
    if (clearSearchBtn && searchInput) {
        clearSearchBtn.addEventListener('click', function() {
            searchInput.value = '';
            searchInput.focus();
        });
    }
    
    // Filter Dropdowns
    const priceRangeDropdown = document.getElementById('priceRangeDropdown');
    const foodTypeDropdown = document.getElementById('foodTypeDropdown');
    const ratingDropdown = document.getElementById('ratingDropdown');
    const filterForm = document.getElementById('filterForm');
    
    // Optional: Auto-submit on dropdown change
    // Uncomment below to enable instant filtering without clicking Apply button
    /*
    if (priceRangeDropdown) {
        priceRangeDropdown.addEventListener('change', function() {
            filterForm.submit();
        });
    }
    
    if (foodTypeDropdown) {
        foodTypeDropdown.addEventListener('change', function() {
            filterForm.submit();
        });
    }
    
    if (ratingDropdown) {
        ratingDropdown.addEventListener('change', function() {
            filterForm.submit();
        });
    }
    */
    
    // Add visual feedback on dropdown change
    function addDropdownChangeEffect(dropdown) {
        if (dropdown) {
            dropdown.addEventListener('change', function() {
                // Add a subtle animation effect
                this.style.transform = 'scale(1.02)';
                setTimeout(() => {
                    this.style.transform = 'scale(1)';
                }, 200);
            });
        }
    }
    
    addDropdownChangeEffect(priceRangeDropdown);
    addDropdownChangeEffect(foodTypeDropdown);
    addDropdownChangeEffect(ratingDropdown);
});

// Combo functions
function addComboToCart(comboId, quantity = 1) {
    fetch('/Combo/AddComboToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `comboId=${comboId}&quantity=${quantity}`
    })
    .then(response => {
        console.log('Combo Response status:', response.status);
        return response.json();
    })
    .then(data => {
        console.log('Combo Response data:', data);
        if (data.success) {
            showToast('Success', data.message, 'success');
            updateCartCount();
        } else {
            // Redirect to login page if user is not logged in
            if (data.requireLogin === true) {
                console.log('Redirecting to login page...');
                showToast('Info', data.message, 'warning');
                setTimeout(() => {
                    console.log('Executing redirect...');
                    window.location.href = '/Account/Login';
                }, 1500);
            } else {
                showToast('Error', data.message, 'danger');
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Error', 'Failed to add combo to cart', 'danger');
    });
}
