const webPush = require("web-push");
const express = require('express');
const bodyParser = require('body-parser');
const axios = require('axios');
require('dotenv').config();

const app = express();

app.use(bodyParser.json());

webPush.setVapidDetails(
    'email', //Change with real email
    process.env.VapidPublicKey,
    process.env.VapidPrivateKey
);

function sendPushNotifications(clientID, message, subject, link) {
    msg = {};
    msg["message"] = message;
    msg["subject"] = subject;
    msg["link"] = link;
    webPush.sendNotification(clientID, JSON.stringify(msg))
    .then((res) => {return 0;})
    .catch((err) => {return 1; });
}

app.post("push/sendMessage", async (req, res) => {
    const {clientId, msg, subject, link} = req.body;

    player = axios.get(`/Player/${clientId}`);

    if (!player.enablePushNotifications) {
        res.status(200).json({});
        return;
    }

    result = sendPushNotifications(clientId, msg, subject, link);
    if (result == 1) {
        res.status(400).json({"Message": "Error"});
    }

    res.status(200).json({});
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {});