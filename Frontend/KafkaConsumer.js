const { Kafka } = require('kafkajs');
const axios = require('axios');
const express = require('express');

const app = express();

const kafka = new Kafka({
    brokers: ['',],
});

const getConsumer = (clientId) => {
    return kafka.consumer({ groupId: 'my-group', clientId: clientId });
};

const getMessage = async (topic_name, clientId) => {
    return new Promise((resolve, reject) => {
        const consumer = getConsumer(clientId);

        consumer.connect().then(async () => {
            await consumer.subscribe({ topic: topic_name, fromBeginning: true });

            consumer.run({
                eachMessage: async ({ topic, partition, message }) => {
                    resolve(message.value.toString());
                },
            }).catch(error => {
                reject(error);
            });
        }).catch(error => {
            reject(error);
        });
    });
};

app.get(`/kafka/:topic/:clientId`, async (req, res) => {
    try {
        const { topic, clientId } = req.params;

        const sendResponse = (message) => {
            res.json({ message });
        };

        const pollMessages = async () => {
            try {
                const message = await getMessage(topic, clientId);

                if (message) {
                    sendResponse(message);
                } else {
                    setTimeout(pollMessages, 1000); 
                }
            } catch (error) {
                console.error('Error:', error);
                res.status(500).json({ error: 'Internal server error' });
            }
        };

        pollMessages();
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

app.listen(8000, () => {});
