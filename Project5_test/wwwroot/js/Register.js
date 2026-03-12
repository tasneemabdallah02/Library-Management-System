static show(message, type = 'success', duration = 3000) {
    if (!this.shouldShowNotification()) return;

    const container = document.getElementById('notification-container');
    const notification = document.createElement('div');
    notification.className = `custom-notification ${type}`;

    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'times-circle' : 'exclamation-circle'}"></i>
        <div class="notification-content">
            <p>${message}</p>
        </div>
    `;

    container.appendChild(notification);
    notification.style.animation = 'slideIn 0.3s ease forwards';

    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease forwards';
        setTimeout(() => notification.remove(), 300);
    }, duration);
}

static shouldShowNotification() {
    const lastAction = localStorage.getItem('lastAction');
    const currentTime = Date.now();

    const allowedActions = ['settings', 'markAllRead', 'clearNotifications'];
    return allowedActions.includes(lastAction);
}

static init() {
    const notificationList = document.querySelector('.notification-list');
    const notificationTrigger = document.querySelector('.notification-trigger');
    const notificationsDropdown = document.querySelector('.notifications-dropdown');
    const markAllRead = document.querySelector('.action-button.secondary');
    const filterPills = document.querySelectorAll('.filter-pill');
    const filterButtons = document.querySelectorAll('.filter-button');

    this.renderNotifications();

    if (notificationTrigger) {
        notificationTrigger.addEventListener('click', (e) => {
            e.stopPropagation();
            notificationsDropdown.classList.toggle('active');
        });
    }

    if (markAllRead) {
        markAllRead.addEventListener('click', () => {
            localStorage.setItem('lastAction', 'markAllRead');
            this.notifications.forEach(notif => notif.unread = false);
            this.renderNotifications();
        });
    }

    filterPills.forEach(pill => {
        pill.addEventListener('click', () => {
            filterPills.forEach(p => p.classList.remove('active'));
            pill.classList.add('active');
            this.renderNotifications(pill.textContent.toLowerCase());
        });
    });

    filterButtons.forEach(button => {
        button.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            button.classList.add('active');
        });
    });

    document.addEventListener('click', (e) => {
        if (!notificationsDropdown?.contains(e.target)) {
            notificationsDropdown?.classList.remove('active');
        }
    });
}

static renderNotifications(filter = 'all') {
    const notificationList = document.querySelector('.notification-list');
    if (!notificationList) return;

    let filteredNotifications = this.notifications;
    if (filter === 'unread') {
        filteredNotifications = this.notifications.filter(n => n.unread);
    } else if (filter === 'important') {
        filteredNotifications = this.notifications.filter(n => n.type === 'warning');
    }

    notificationList.innerHTML = filteredNotifications.map(notification => `
        <div class="notification-item ${notification.unread ? 'unread' : ''}" data-id="${notification.id}">
            <div class="notification-icon ${notification.type}">
                <i class="fas fa-${notification.icon}"></i>
            </div>
            <div class="notification-content">
                <div class="notification-title">${notification.title}</div>
                <div class="notification-message">${notification.message}</div>
                <div class="notification-time">${notification.time}</div>
                ${notification.actions ? `
                    <div class="notification-actions">
                        ${notification.actions.map(action => `
                            <button class="notification-button ${action.toLowerCase() === 'view details' ? 'view' : 'dismiss'}">
                                ${action === 'View Details' ?
            '<i class="fas fa-eye"></i> View' :
            '<i class="fas fa-times"></i> Dismiss'}
                            </button>
                        `).join('')}
                    </div>
                ` : ''}
            </div>
        </div>
    `).join('');

    const badge = document.querySelector('.notification-badge');
    const unreadCount = this.notifications.filter(n => n.unread).length;
    if (badge) {
        badge.textContent = unreadCount;
        badge.style.display = unreadCount > 0 ? 'block' : 'none';
    }

    const notificationItems = notificationList.querySelectorAll('.notification-item');
    notificationItems.forEach(item => {
        item.addEventListener('click', () => {
            const id = parseInt(item.dataset.id);
            const notification = this.notifications.find(n => n.id === id);
            if (notification && notification.unread) {
                notification.unread = false;
                this.renderNotifications(filter);
            }
        });
    });
}