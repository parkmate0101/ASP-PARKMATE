document.addEventListener("DOMContentLoaded", function () {

    const nameField = document.querySelector('[name="Name"]');
    const phoneField = document.querySelector('[name="Contact"]');
    const emailField = document.querySelector('[name="Email"]');

    if (!nameField || !phoneField || !emailField) {
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

    // NAME

    nameField.addEventListener("blur", function () {

        if (!/^[A-Za-z ]{3,50}$/.test(nameField.value.trim())) {
            showError(
                nameField,
                "Only letters & spaces (min 3 chars)"
            );
        }
        else {
            showSuccess(nameField);
        }
    });

    // PHONE

    phoneField.addEventListener("blur", function () {

        if (!/^[0-9]{10}$/.test(phoneField.value.trim())) {
            showError(
                phoneField,
                "Phone must be exactly 10 digits"
            );
        }
        else {
            showSuccess(phoneField);
        }
    });

    // EMAIL

    emailField.addEventListener("blur", function () {

        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailField.value.trim())) {
            showError(
                emailField,
                "Invalid email format"
            );
        }
        else {
            showSuccess(emailField);
        }
    });

    // SUBMIT

    window.validateSignup = function () {

        let valid = true;

        if (!/^[A-Za-z ]{3,50}$/.test(nameField.value.trim())) {

            showError(
                nameField,
                "Only letters & spaces (min 3 chars)"
            );

            valid = false;
        }

        if (!/^[0-9]{10}$/.test(phoneField.value.trim())) {

            showError(
                phoneField,
                "Phone must be 10 digits"
            );

            valid = false;
        }

        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailField.value.trim())) {

            showError(
                emailField,
                "Invalid email format"
            );

            valid = false;
        }

        if (!valid) {

            const firstError =
                document.querySelector(".auth-error:not(:empty)");

            if (firstError &&
                firstError.previousElementSibling) {

                firstError.previousElementSibling.focus();
            }

            return false;
        }

        const button =
            document.querySelector(".signup-submit");

        if (button) {
            button.disabled = true;
            button.style.opacity = "0.7";
        }

        return true;
    };
});