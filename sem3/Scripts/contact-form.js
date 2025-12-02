/**
 * Contact Form JavaScript
 * Handles form validation, CAPTCHA, and layout stabilization
 */

(function ($) {
    'use strict';

    var ContactForm = {
        // Initialize all contact form functionality
        init: function () {
            this.stabilizeLayout();
            this.bindEvents();
            this.handlePageLoad();
        },

        // Bind all event handlers
        bindEvents: function () {
            // Form submission
            $('#contactForm').on('submit', this.handleSubmit.bind(this));

            // CAPTCHA refresh
            $('#refreshCaptcha').on('click', this.refreshCaptcha.bind(this));

            // Window resize
            $(window).on('resize', this.handleResize.bind(this));

            // CAPTCHA input validation
            $('#captchaInput').on('input', this.validateCaptchaInput.bind(this));

            // Form input validation
            $('input, textarea').on('blur', this.validateInput.bind(this));
        },

        // Handle form submission
        handleSubmit: function (e) {
            var form = $(e.target);
            var submitBtn = form.find('.submit-btn');
            var captchaInput = $('#captchaInput').val().trim();

            // Basic validation
            if (!this.validateForm()) {
                e.preventDefault();
                return false;
            }

            // CAPTCHA validation
            if (captchaInput.length === 0) {
                e.preventDefault();
                this.showCaptchaError('Please enter CAPTCHA code');
                return false;
            }

            // Show loading state
            this.showLoading(submitBtn);

            // Stabilize layout during submission
            setTimeout(function () {
                ContactForm.stabilizeLayout();
            }, 300);

            // Continue with form submission
            return true;
        },

        // Validate form inputs
        validateForm: function () {
            var isValid = true;
            var form = $('#contactForm');

            // Check required fields
            form.find('[required]').each(function () {
                var input = $(this);
                var value = input.val().trim();

                if (value === '') {
                    isValid = false;
                    ContactForm.showFieldError(input, 'This field is required');
                } else {
                    ContactForm.clearFieldError(input);
                }
            });

            // Validate email format
            var emailInput = $('#Email');
            if (emailInput.length && emailInput.val().trim()) {
                var email = emailInput.val().trim();
                var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

                if (!emailRegex.test(email)) {
                    isValid = false;
                    ContactForm.showFieldError(emailInput, 'Please enter a valid email address');
                }
            }

            return isValid;
        },

        // Validate individual input on blur
        validateInput: function (e) {
            var input = $(e.target);
            var value = input.val().trim();

            if (input.attr('required') && value === '') {
                this.showFieldError(input, 'This field is required');
            } else {
                this.clearFieldError(input);
            }

            // Specific validation for email
            if (input.attr('type') === 'email' && value) {
                var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(value)) {
                    this.showFieldError(input, 'Please enter a valid email address');
                }
            }
        },

        // Validate CAPTCHA input in real-time
        validateCaptchaInput: function (e) {
            var input = $(e.target);
            var value = input.val().trim();

            // Auto-uppercase
            if (value !== value.toUpperCase()) {
                input.val(value.toUpperCase());
            }

            // Clear error if user starts typing
            if (value.length > 0) {
                $('#captchaError').hide();
            }
        },

        // Refresh CAPTCHA image
        refreshCaptcha: function () {
            var captchaContainer = $('.captcha-container');
            var captchaImage = $('#captchaImage');
            var captchaInput = $('#captchaInput');

            // Show loading state
            captchaContainer.css('opacity', '0.7');
            $('#captchaError').hide();

            // Generate new timestamp to prevent caching
            var timestamp = new Date().getTime();
            var newSrc = captchaImage.attr('src').split('?')[0] + '?t=' + timestamp;

            // Fade out and in effect
            captchaImage.fadeOut(200, function () {
                captchaImage.attr('src', newSrc).fadeIn(200);
                captchaInput.val('');
                captchaContainer.css('opacity', '1');

                // Play refresh sound (optional)
                ContactForm.playSound('refresh');
            });

            // Re-stabilize layout
            setTimeout(function () {
                ContactForm.stabilizeLayout();
            }, 300);
        },

        // Show field error
        showFieldError: function (input, message) {
            var fieldName = input.attr('name') || 'field';
            var errorId = fieldName + 'Error';
            var errorSpan = $('#' + errorId);

            if (errorSpan.length === 0) {
                errorSpan = $('<span/>', {
                    id: errorId,
                    class: 'field-error text-danger',
                    text: message
                });
                input.after(errorSpan);
            } else {
                errorSpan.text(message).show();
            }

            input.addClass('error');
            input.focus();
        },

        // Clear field error
        clearFieldError: function (input) {
            var fieldName = input.attr('name') || 'field';
            var errorId = fieldName + 'Error';

            $('#' + errorId).hide();
            input.removeClass('error');
        },

        // Show CAPTCHA error
        showCaptchaError: function (message) {
            var errorSpan = $('#captchaError');
            errorSpan.text(message || 'Invalid CAPTCHA code').show();
            $('#captchaInput').focus().addClass('error');

            // Auto-refresh CAPTCHA after error
            setTimeout(this.refreshCaptcha.bind(this), 1500);
        },

        // Show loading state
        showLoading: function (button) {
            var originalText = button.html();
            button.data('original-text', originalText);
            button.addClass('loading').html(
                '<span class="loading-spinner"></span> Sending Message...'
            ).prop('disabled', true);
        },

        // Hide loading state
        hideLoading: function (button) {
            var originalText = button.data('original-text');
            button.removeClass('loading').html(originalText).prop('disabled', false);
        },

        // Stabilize layout to prevent shifting
        stabilizeLayout: function () {
            var form = $('.contact-form');
            var contactInfo = $('.contact-info');
            var alertElement = $('.alert');

            // Calculate heights
            var formHeight = form.outerHeight();
            var infoHeight = contactInfo.outerHeight();
            var alertHeight = alertElement.outerHeight() || 0;

            // Use the larger height for both columns
            var maxHeight = Math.max(formHeight, infoHeight);

            // Apply consistent heights
            form.css('min-height', maxHeight + 'px');
            contactInfo.css('min-height', maxHeight + 'px');

            // Adjust content position based on alert
            $('.contact-content').css('margin-top', alertHeight + 'px');

            // Force reflow for smooth transitions
            form[0].offsetHeight;
        },

        // Handle window resize
        handleResize: function () {
            clearTimeout(this.resizeTimer);
            this.resizeTimer = setTimeout(function () {
                ContactForm.stabilizeLayout();
            }, 250);
        },

        // Handle page load
        handlePageLoad: function () {
            // Scroll to form if there's an alert
            if ($('.alert').length > 0) {
                this.scrollToForm();
            }

            // Auto-refresh CAPTCHA every 2 minutes
            this.startCaptchaAutoRefresh();

            // Initialize tooltips (if any)
            this.initTooltips();
        },

        // Scroll to form smoothly
        scrollToForm: function () {
            $('html, body').animate({
                scrollTop: $('.contact-container').offset().top - 100
            }, 800);
        },

        // Start auto-refresh for CAPTCHA
        startCaptchaAutoRefresh: function () {
            setInterval(this.refreshCaptcha.bind(this), 120000); // 2 minutes
        },

        // Initialize tooltips
        initTooltips: function () {
            $('[data-toggle="tooltip"]').tooltip({
                trigger: 'hover',
                placement: 'top'
            });
        },

        // Play sound effect (optional)
        playSound: function (type) {
            try {
                var audioContext = new (window.AudioContext || window.webkitAudioContext)();
                var oscillator = audioContext.createOscillator();
                var gainNode = audioContext.createGain();

                oscillator.connect(gainNode);
                gainNode.connect(audioContext.destination);

                if (type === 'refresh') {
                    oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
                    oscillator.frequency.setValueAtTime(1200, audioContext.currentTime + 0.1);
                    gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
                }

                oscillator.start();
                oscillator.stop(audioContext.currentTime + 0.2);
            } catch (e) {
                // Audio not supported, silent fail
            }
        },

        // Public method to manually refresh layout
        refreshLayout: function () {
            this.stabilizeLayout();
        }
    };

    // Initialize when document is ready
    $(document).ready(function () {
        ContactForm.init();
    });

    // Make ContactForm available globally
    window.ContactForm = ContactForm;

})(jQuery);