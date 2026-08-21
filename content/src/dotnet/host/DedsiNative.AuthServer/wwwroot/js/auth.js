document.addEventListener('DOMContentLoaded', () => {
    // 密码显隐切换
    const toggleBtns = document.querySelectorAll('.toggle-password');
    toggleBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = btn.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (input) {
                if (input.type === 'password') {
                    input.type = 'text';
                    btn.textContent = '隐藏';
                } else {
                    input.type = 'password';
                    btn.textContent = '显示';
                }
            }
        });
    });
});
