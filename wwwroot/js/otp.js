document.addEventListener("DOMContentLoaded", function () {
    const otpField = document.querySelector('[name="Otp"]');

    if (!otpField) {
        return;
    }

    otpField.addEventListener("input", function () {
        otpField.value = otpField.value
            .replace(/\D/g, "")
            .slice(0, 6);
    });
});
