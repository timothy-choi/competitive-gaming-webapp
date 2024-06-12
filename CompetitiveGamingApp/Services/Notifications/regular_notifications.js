const http = require('http');
const socketIO = require('socket.io');
const Notification = require('./models/Notification');

const server = http.createServer(app);
const io = socketIO(server);

const clients = {};

io.on('connection', (socket) => {
    socket.on('identify', (userId) => {
        clients[userId] = socket.id;
    });

    socket.on('disconnect', () => {
        for (const userId in clients) {
            if (clients[userId] === socket.id) {
                delete clients[userId];
            }
        }
    });
});

async function createNotification(userId, message, subject, link, createdAt) {
    try {
        const notification = new Notification({
            userId,
            message,
            subject,
            link,
            createdAt
        });
        await notification.save();
        console.log('Notification saved:', notification);
    } catch (error) {
        console.error('Error creating notification:', error);
    }
}

async function sendRealTimeNotification(userId, message, subject, link) {
    const socketId = clients[userId];
    if (socketId) {
        msg = {
            "message" : message,
            "subject" : subject,
            "link" : link
        };

        io.to(socketId).emit('notification', msg);
    }
}

async function handleNewMessage(userId, message, subject, link, createdAt) {
    await createNotification(userId, message, subject, link, createdAt); // Save to DB
    await sendRealTimeNotification(userId, message, subject, link); // Send via WebSocket
}

module.exports = {handleNewMessage};

const PORT = process.env.PORT || 3000;
server.listen(PORT, () => {});