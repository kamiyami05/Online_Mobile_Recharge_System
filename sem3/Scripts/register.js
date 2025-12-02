if (typeof jQuery === 'undefined') {
    console.error('jQuery is not loaded!');
    var script = document.createElement('script');
    script.src = 'https://code.jquery.com/jquery-3.6.0.min.js';
    script.onload = function () {
        initializeRegister();
    };
    document.head.appendChild(script);
} else {
    $(document).ready(initializeRegister);
}

function initializeRegister() {

    $('#registerForm').on('submit', function (e) {
        e.preventDefault();
        return false;
    });

    $('#registerBtn').on('click', function (e) {
        e.preventDefault();

        const phoneNumber = $('#phoneNumber').val().trim();
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();

        if (!phoneNumber || !password || !confirmPassword) {
            Swal.fire('Missing information', 'Please fill in all required fields', 'error');
            return false;
        }

        if (password !== confirmPassword) {
            Swal.fire('Error', 'Passwords do not match', 'error');
            return false;
        }

        showOTPDialog(phoneNumber, password, confirmPassword);
        return false;
    });

    function showOTPDialog(phoneNumber, password, confirmPassword) {
        Swal.fire({
            title: 'OTP Verification',
            html: `
            <div style="text-align: center; max-width: 400px; margin: 0 auto;">
                <div style="font-size: 48px; color: #4CAF50; margin-bottom: 15px;">
                    <i class="fas fa-sms"></i>
                </div>
                <p style="margin-bottom: 10px; font-size: 16px; word-break: break-word;">
                    OTP has been sent to <b style="color: #2196F3;">${phoneNumber}</b>
                </p>
                <p style="margin: 15px 0; padding: 12px; background: #f8f9fa; border-radius: 8px; font-size: 15px;">
                    <i class="fas fa-info-circle" style="color: #2196F3; margin-right: 5px;"></i>
                    Default OTP code: <b style="color: #e91e63; font-size: 18px;">1234</b>
                </p>
            </div>
        `,
            input: 'text',
            inputPlaceholder: 'Enter OTP',
            inputAttributes: {
                autocapitalize: 'off',
                autocorrect: 'off',
                maxlength: '4',
                style: 'text-align: center; font-size: 24px; letter-spacing: 5px; width: 100%; box-sizing: border-box;'
            },
            showCancelButton: true,
            confirmButtonText: '<i class="fas fa-check"></i> Verify',
            cancelButtonText: '<i class="fas fa-times"></i> Cancel',
            confirmButtonColor: '#4CAF50',
            cancelButtonColor: '#f44336',
            allowOutsideClick: false,
            backdrop: 'rgba(0,0,0,0.4)',
            width: '450px',
            padding: '20px',
            customClass: {
                popup: 'otp-popup',
                title: 'otp-title',
                htmlContainer: 'otp-html',
                input: 'otp-input',
                confirmButton: 'otp-confirm-btn',
                cancelButton: 'otp-cancel-btn',
                container: 'otp-container'
            },
            preConfirm: (otp) => {
                if (!otp) {
                    Swal.showValidationMessage('Please enter the OTP code');
                    return false;
                }
                if (otp !== "1234") {
                    Swal.showValidationMessage('Invalid OTP. Please enter 1234');
                    return false;
                }
                return otp;
            },
            didOpen: () => {
                const input = Swal.getInput();
                if (input) {
                    input.focus();
                    input.select();
                    input.classList.add('otp-input-active');
                    input.style.width = '100%';
                    input.style.maxWidth = '100%';
                }

                const popup = Swal.getPopup();
                if (popup) {
                    popup.style.maxWidth = '450px';
                    popup.style.width = '450px';
                }
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const otp = result.value;
                sendRegistration(phoneNumber, password, confirmPassword, otp);
            } else {
                $('#registerBtn').prop('disabled', false).html('<i class="fas fa-user-plus"></i> Register');
            }
        });
    }

    function sendRegistration(phoneNumber, password, confirmPassword, otp) {
        Swal.fire({
            title: 'Processing...',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '/Login/Register',
            type: 'POST',
            data: {
                Phone: phoneNumber,
                Password: password,
                ConfirmPassword: confirmPassword,
                OTP: otp,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                Swal.close();

                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: response.message,
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#registerForm')[0].reset();
                        window.location.href = '/Login/Login';
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message,
                        confirmButtonText: 'Retry'
                    });
                }
            },
            error: function () {
                Swal.close();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred while connecting to the server',
                    confirmButtonText: 'OK'
                });
            }
        });
    }
}
