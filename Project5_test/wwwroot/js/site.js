// تحديد عناصر الصفحة
const form = document.querySelector('form');
const emailInput = form.querySelector('input[type="email"]');
const passwordInput = form.querySelector('input[placeholder="Password"]');
const confirmInput = form.querySelector('input[placeholder="Confirm-Password"]');

// إنشاء div لعرض الأخطاء
let errorDiv = document.createElement('div');
errorDiv.style.color = '#b40306';
errorDiv.style.marginTop = '20px';
errorDiv.style.fontSize = '25px';
form.prepend(errorDiv);

form.addEventListener('submit', function (e) {
    e.preventDefault();
    const email = emailInput.value.trim();
    const password = passwordInput.value.trim();
    const confirmPassword = confirmInput.value.trim();

    // تحقق من البريد الجامعي
    const emailRegex = /^[a-zA-Z0-9._%+-]+@ses\.yu\.edu\.jo$/;
    if (!emailRegex.test(email)) {
        errorDiv.innerText = "Email must be a university email (@ses.yu.edu.jo)";
        return;
    }

    // تحقق من كلمة المرور: 8 أحرف على الأقل مع رقم واحد على الأقل
    const passwordRegex = /^(?=.*\d).{8,}$/;
    if (!passwordRegex.test(password)) {
        errorDiv.innerText = "Password must be at least 8 characters long and contain at least one number";
        return;
    }

    // تحقق من المطابقة
    if (password !== confirmPassword) {
        errorDiv.innerText = "Passwords do not match";
        return;
    }

    // إذا كل شيء صحيح
    errorDiv.innerText = "";

    // حفظ البيانات مؤقتًا للخطوة التالية
    localStorage.setItem('regEmail', email);
    localStorage.setItem('regPassword', password);

    // توجيه المستخدم لصفحة Step2
    alert("Pass");
    window.location.href = "signup-step2.html";
});
