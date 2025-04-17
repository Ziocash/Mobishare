// LOGIN
document.querySelector('.toggle-password').addEventListener('click', function() {
    const inputPW = this.parentElement.querySelector('input');
    if (inputPW.type === 'password') {
        inputPW.type = 'text';
    } else {
        inputPW.type = 'password';
    }
});