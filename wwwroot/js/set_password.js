document.addEventListener("DOMContentLoaded", function () {
    window.validatePassword = function () {
        const pass = document.querySelector('[name="Password"]');
        const confirm = document.querySelector('[name="ConfirmPassword"]');

        if (!pass || !confirm) {
            return false;
        }

        if (pass.value.length < 6) {
            return showError(pass, "Password must be at least 6 characters.");
        }

        if (!/[A-Z]/.test(pass.value)) {
            return showError(pass, "Password must contain an uppercase letter.");
        }

        if (!/[a-z]/.test(pass.value)) {
            return showError(pass, "Password must contain a lowercase letter.");
        }

        if (!/[0-9]/.test(pass.value)) {
            return showError(pass, "Password must contain a number.");
        }

        if (pass.value !== confirm.value) {
            return showError(confirm, "Passwords do not match.");
        }

        return true;
    };

    function showError(input, message) {
        input.style.border = "2px solid red";
        const error = input.nextElementSibling;
        if (error) {
            error.textContent = message;
        }
        return false;
    }
});
