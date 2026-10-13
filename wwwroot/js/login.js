document.addEventListener("DOMContentLoaded", function () {

    const emailField = document.querySelector('[name="Email"]');
    const passField = document.querySelector('[name="Password"]');

    if (!emailField || !passField) {
        return;
    }

    function showError(input, message) {
        input.style.border = "2px solid red";

        const error = input.nextElementSibling;

        if (error) {
            error.textContent = message;
        }
    }

    function showSuccess(input) {
        input.style.border = "2px solid green";

        const error = input.nextElementSibling;

        if (error) {
            error.textContent = "";
        }
    }

    emailField.addEventListener("blur", function () {
        const email = emailField.value.trim();

        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            showError(emailField, "Invalid email format");
        } else {
            showSuccess(emailField);
        }
    });

    window.validateLogin = function () {
        const email = emailField.value.trim();

        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            showError(emailField, "Valid email required");
            return false;
        }

        showSuccess(emailField);

        // Password is intentionally optional.
        // Blank password means OTP login for normal users.
        return true;
    };
});
