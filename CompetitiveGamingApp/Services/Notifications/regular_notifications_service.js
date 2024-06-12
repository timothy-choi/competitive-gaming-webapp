const express = require('express');
const Notification = require('./models/Notification');
const { handleNewMessage } = require('./regular_notifications');

const app = express();

app.get('/notifications/:userId', async (req, res) => {
    try {
        const { userId } = req.params;
        const notifications = await Notification.find({ userId });
        res.json(notifications);
    } catch (error) {
        res.status(500).send('Error fetching notifications');
    }
});

app.post('/notifications/:userId', async (req, res) => {
    try {
        const {clientId, message, subject, link} = req.body;

        await handleNewMessage(clientId, message, subject, link, new Date.toString());
        
        res.status(500).send('');
    } catch (error) {
        res.status(500).send('Error fetching notifications');
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {});