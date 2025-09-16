(function($) {
    'use strict';

    var Checkout = {
        selectedProductIds: [],
        cartData: [],
        
        init: function() {
            this.selectedProductIds = JSON.parse(sessionStorage.getItem('selectedProducts') || '[]');
            this.cartData = window.cartData || [];
            
            if (this.selectedProductIds.length === 0) {
                this.redirectToCart();
                return;
            }
            
            this.displaySelectedProducts();
            this.bindEvents();
        },

        redirectToCart: function() {
            Swal.fire({
                icon: 'warning',
                title: 'No Products Selected',
                text: 'Please select products from your cart first.',
                confirmButtonText: 'Go to Cart'
            }).then(function() {
                window.location.href = '/Customer/GetCartItemsByUser';
            });
        },

        displaySelectedProducts: function() {
            var self = this;
            var $container = $('#selectedProducts');
            var subtotal = 0;
            
            $container.empty();
            
            this.selectedProductIds.forEach(function(productId) {
                var product = self.cartData.find(function(item) {
                    return item.ProductId === productId;
                });
                
                if (product) {
                    var itemTotal = product.Quantity * product.ProductPrice;
                    subtotal += itemTotal;
                    
                    var row = $('<tr>').html(
                        '<td>' + product.ProductName + ' <span class="product-qty">× ' + product.Quantity + '</span></td>' +
                        '<td>' + self.formatCurrency(itemTotal) + '</td>'
                    );
                    $container.append(row);
                }
            });
            
            $('#subtotal').text(this.formatCurrency(subtotal));
            $('#total').text(this.formatCurrency(subtotal)); // Same as subtotal since shipping is free
        },

        bindEvents: function() {
            $('#placeOrderBtn').on('click', this.placeOrder.bind(this));
        },

        placeOrder: function(e) {
            e.preventDefault();
            
            var shippingAddress = $('#shippingAddress').val().trim();
            var paymentMethod = $('input[name="payment_method"]:checked').val();
            
            // Validation
            if (!shippingAddress) {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: 'Please enter your shipping address.',
                    confirmButtonText: 'OK'
                });
                $('#shippingAddress').focus();
                return;
            }
            
            if (!paymentMethod) {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: 'Please select a payment method.',
                    confirmButtonText: 'OK'
                });
                return;
            }
            
            // Prepare checkout data
            var checkoutData = {
                userId: window.userId,
                shippingAddress: shippingAddress,
                paymentMethod: paymentMethod,
                productIds: this.selectedProductIds
            };
            
            this.submitOrder(checkoutData);
        },

        submitOrder: function(checkoutData) {
            var self = this;
            
            // Show loading state
            var $btn = $('#placeOrderBtn');
            var originalText = $btn.html();
            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Processing...');
            
            $.ajax({
                url: '/Customer/PlaceOrder',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(checkoutData),
                success: function(response) {
                    if (response.success) {
                        // Clear selected products from session storage
                        sessionStorage.removeItem('selectedProducts');
                        
                        Swal.fire({
                            icon: 'success',
                            title: 'Order Placed Successfully!',
                            text: 'Your order has been placed and is being processed.',
                            confirmButtonText: 'Continue Shopping'
                        }).then(function() {
                            window.location.href = '/Customer/Index';
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Order Failed',
                            text: response.message || 'There was an error processing your order.',
                            confirmButtonText: 'Try Again'
                        });
                    }
                },
                error: function(xhr, status, error) {
                    console.error('Order submission error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Order Failed',
                        text: 'There was an error processing your order. Please try again.',
                        confirmButtonText: 'Try Again'
                    });
                },
                complete: function() {
                    // Restore button state
                    $btn.prop('disabled', false).html(originalText);
                }
            });
        },

        formatCurrency: function(amount) {
            return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
        }
    };

    // Initialize when document is ready
    $(document).ready(function() {
        Checkout.init();
    });

})(jQuery);