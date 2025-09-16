(function($) {
    'use strict';

    var Cart = {
        selectedProducts: [],
        
        init: function() {
            this.bindEvents();
            this.updateCheckoutButtonState();
        },

        bindEvents: function() {
            // Select all checkbox
            $('#selectAll').on('change', this.handleSelectAll.bind(this));
            
            // Individual checkboxes
            $('input[name="productCheckbox"]').on('change', this.handleProductSelection.bind(this));
            
            // Checkout button
            $('#proceedToCheckout').on('click', this.proceedToCheckout.bind(this));
        },

        handleSelectAll: function(e) {
            var isChecked = $(e.target).is(':checked');
            $('input[name="productCheckbox"]').prop('checked', isChecked).trigger('change');
        },

        handleProductSelection: function(e) {
            var $checkbox = $(e.target);
            var productId = $checkbox.val();
            
            if ($checkbox.is(':checked')) {
                if (this.selectedProducts.indexOf(productId) === -1) {
                    this.selectedProducts.push(productId);
                }
            } else {
                var index = this.selectedProducts.indexOf(productId);
                if (index > -1) {
                    this.selectedProducts.splice(index, 1);
                }
                $('#selectAll').prop('checked', false);
            }
            
            this.updateSelectAllState();
            this.updateCheckoutButtonState();
            this.updateSelectedInfo();
        },

        updateSelectAllState: function() {
            var totalProducts = $('input[name="productCheckbox"]').length;
            var selectedCount = this.selectedProducts.length;
            $('#selectAll').prop('checked', totalProducts > 0 && selectedCount === totalProducts);
        },

        updateCheckoutButtonState: function() {
            var $checkoutBtn = $('#proceedToCheckout');
            if (this.selectedProducts.length > 0) {
                $checkoutBtn.removeClass('disabled').prop('disabled', false);
            } else {
                $checkoutBtn.addClass('disabled').prop('disabled', true);
            }
        },

        updateSelectedInfo: function() {
            var count = this.selectedProducts.length;
            var $info = $('#selectedInfo');
            if (count > 0) {
                $info.text(`Selected ${count} product${count > 1 ? 's' : ''} for checkout`).show();
            } else {
                $info.hide();
            }
        },

        proceedToCheckout: function(e) {
            e.preventDefault();
            
            if (this.selectedProducts.length === 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'No Products Selected',
                    text: 'Please select at least one product to checkout.',
                    confirmButtonText: 'OK'
                });
                return;
            }

            // Store selected products in sessionStorage for checkout page
            sessionStorage.setItem('selectedProducts', JSON.stringify(this.selectedProducts));
            
            // Redirect to checkout page
            window.location.href = '/Customer/Checkout';
        }
    };

    // Initialize when document is ready
    $(document).ready(function() {
        Cart.init();
    });

})(jQuery);