const axios = require('axios');

async function sendNotifications(userId, message, subject, link) {
    try {
        const pushNotificationPayload = {
            userId: userId,
            message: message,
            subject: subject,
            link: link
        };

        const regularNotificationPayload = {
            message: message,
            subject: subject,
            link: link
        };

        await Promise.all([
            axios.post("/push/sendMessage", pushNotificationPayload),
            axios.post(`/push/sendMessage/${userId}`, regularNotificationPayload)
        ]);

        return 0;
    } catch (error) {
        return 1;
    }
}

modules.export = {sendNotifications};