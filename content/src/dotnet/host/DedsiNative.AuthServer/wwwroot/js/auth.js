document.addEventListener('DOMContentLoaded', () => {
    // 元素引用
    const accountInput = document.getElementById('accountInput');
    const passwordInput = document.getElementById('passwordInput');
    const btnNext = document.getElementById('btnNext');
    const btnBackToStep1 = document.getElementById('btnBackToStep1');
    const step1Panel = document.getElementById('step1Panel');
    const step2Panel = document.getElementById('step2Panel');
    const userAccountDisplay = document.getElementById('userAccountDisplay');
    const accountClientError = document.getElementById('accountClientError');
    const passwordClientError = document.getElementById('passwordClientError');
    const ssoLoginForm = document.getElementById('ssoLoginForm');

    // 步骤 1 -> 步骤 2
    function goToStep2() {
        if (!accountInput) return;
        const accountVal = accountInput.value.trim();
        if (!accountVal) {
            if (accountClientError) {
                accountClientError.textContent = '请输入有效的电子邮件、用户名或账号。';
            }
            accountInput.focus();
            return;
        }

        if (accountClientError) {
            accountClientError.textContent = '';
        }

        // 更新回显账号
        if (userAccountDisplay) {
            userAccountDisplay.textContent = accountVal;
        }

        // 切换步骤面板
        if (step1Panel && step2Panel) {
            step1Panel.classList.remove('active');
            step1Panel.classList.add('slide-left-exit');

            step2Panel.classList.remove('slide-right-hidden');
            step2Panel.classList.add('active');

            if (passwordInput) {
                passwordInput.value = '';
                setTimeout(() => passwordInput.focus(), 150);
            }
        }
    }

    // 步骤 2 -> 步骤 1 (返回修改账号)
    function goToStep1() {
        if (!step1Panel || !step2Panel) return;

        if (passwordClientError) {
            passwordClientError.textContent = '';
        }

        step2Panel.classList.remove('active');
        step2Panel.classList.add('slide-right-hidden');

        step1Panel.classList.remove('slide-left-exit');
        step1Panel.classList.add('active');

        if (accountInput) {
            setTimeout(() => {
                accountInput.focus();
                accountInput.select();
            }, 150);
        }
    }

    // 绑定下一步与返回按钮
    if (btnNext) {
        btnNext.addEventListener('click', (e) => {
            e.preventDefault();
            goToStep2();
        });
    }

    if (btnBackToStep1) {
        btnBackToStep1.addEventListener('click', (e) => {
            e.preventDefault();
            goToStep1();
        });
    }

    // 账号输入框回车直接进入下一步
    if (accountInput) {
        accountInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                goToStep2();
            }
        });
        accountInput.addEventListener('input', () => {
            if (accountClientError) {
                accountClientError.textContent = '';
            }
        });
    }

    // 密码输入框回车提交校验
    if (ssoLoginForm) {
        ssoLoginForm.addEventListener('submit', (e) => {
            if (!passwordInput) return;
            const pwdVal = passwordInput.value;
            if (!pwdVal) {
                e.preventDefault();
                if (passwordClientError) {
                    passwordClientError.textContent = '请输入密码。';
                }
                passwordInput.focus();
            }
        });
    }

    if (passwordInput) {
        passwordInput.addEventListener('input', () => {
            if (passwordClientError) {
                passwordClientError.textContent = '';
            }
        });
    }

    // 密码显隐切换
    const toggleBtns = document.querySelectorAll('.toggle-password');
    toggleBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = btn.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (input) {
                const isPassword = input.type === 'password';
                input.type = isPassword ? 'text' : 'password';
                btn.style.opacity = isPassword ? '1' : '0.6';
                btn.setAttribute('title', isPassword ? '隐藏密码' : '显示密码');
            }
        });
    });

    // 登录选项模态框
    const btnToggleOptions = document.getElementById('btnToggleOptions');
    const optionsModal = document.getElementById('optionsModal');
    const btnCloseOptions = document.getElementById('btnCloseOptions');

    if (btnToggleOptions && optionsModal) {
        btnToggleOptions.addEventListener('click', () => {
            optionsModal.style.display = 'flex';
        });
    }

    if (btnCloseOptions && optionsModal) {
        btnCloseOptions.addEventListener('click', () => {
            optionsModal.style.display = 'none';
        });
    }

    if (optionsModal) {
        optionsModal.addEventListener('click', (e) => {
            if (e.target === optionsModal) {
                optionsModal.style.display = 'none';
            }
        });
    }
});

// 全局选项处理
function selectOption(type) {
    const optionsModal = document.getElementById('optionsModal');
    if (optionsModal) {
        optionsModal.style.display = 'none';
    }
    if (type === 'fido') {
        alert('当前环境已准备好支持 WebAuthn / FIDO2 安全密钥认证。');
    } else if (type === 'sso') {
        alert('企业组织 SSO 将通过您的企业域名匹配相应的 IdP 身份提供商。');
    }
}
