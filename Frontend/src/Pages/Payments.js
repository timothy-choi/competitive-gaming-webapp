import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';

const pollForData = async (type, username) => {
    let intervalId;
    const pollData = async (type, username, intervalId) => {
        await axios.get(`/webhook`)
        .then(res => {
            if (res.data.status == type && res.data.username == username) {
                clearInterval(intervalId);
                return res.data;
            }
        })
        .catch(error => {
            throw error;
        });
    }
   
    intervalId = setInterval(pollData, 5);
};


const paymentPopup = (props) => {
    const [successful, setSuccessful] = useState(false);
    const [error, setError] = useState(false);
    const [errMsg, setErrMsg] = useState('');
    const [price, setPrice] = useState(0.0);
    const [userAcct, setUserAcct] = useState(null); 


    useState(() => {
        var fetchData = async () => {
            const paymentAcct = await axios.get(`/playerPayment/${props.senderUsername}`);
            setUserAcct(paymentAcct);
            const player = await axios.get(`/player/${props.recipientUsername}`);
            setPrice(player.data.singleGamePrice);
        };

        fetchData();
    }, []);

    const handlePaymentSubmit = (event) => {
        const payUser = async () => {
            try {
                await axios.put(`/playerPayment/${props.senderUsername}/IdempotencyKey`);

                const paymentAcct = await axios.get(`/playerPayment/${props.senderUsername}`);
                setUserAcct(paymentAcct);

                var reqBody = {
                    "idempotency_key" : userAcct.idempotency_key,
                    "request" : {
                        "action": [
                            {
                                "amount" : price,
                                "currency" : "US"
                            }
                        ]
                    }
                };

                await axios.post(`/playerPayment/${props.sendUsername}/Grant`, reqBody);

                var res = pollForData("grant.created", props.sendUsername);

                if (res.data.status != "Valid") {
                    setError(true);
                    setErrMsg("Error: Couldn't process grant id to pay");
                    return;
                }

                reqBody = {
                    "idempotency_key" : userAcct.idempotency_key,
                    "price" : price,
                    "currency" : "US",
                    "grant_id" : res.data.grant_id
                };

                await axios.post(`/playerPayment/${props.senderUsername}/Payment`, reqBody);

                res = pollForData("payment.status.updated", props.sendUsername);

                if (!res.data.status.contains("Authorized") && !res.data.status.contains("Captured")) {
                    setError(true);
                    setErrMsg("Error: Couldn't process payment");
                    return;
                }

                setSuccessful(true);
            } catch (err) {
                setError(true);
                setErrMsg(err);
                return;
            }
        };

        payUser();
    };
};

const refundPopup = (props) => {
    const [successfull, setSuccessfull] = useState(false);
    const [error, setError] = useState(false);
    const [errMsg, setErrorMsg] = useState('');
    const [price, setPrice] = useState(0.0);
    const [userAcct, setUserAcct] = useState(null); 

    useState(() => {
        var fetchData = async () => {
            const paymentAcct = await axios.get(`/playerPayment/${props.senderUsername}`);
            setUserAcct(paymentAcct);
            const player = await axios.get(`/player/${props.recipientUsername}`);
            setPrice(player.data.singleGamePrice);
        };

        fetchData();
    }, []);

    const handleRefundSubmit = (event) => {
        var processRefund = async () => {
            try {
                var allTransactions = await axios.get("/GameTransactions");

                var paid_id;

                for (let i = allTransactions.length; i >= 0; --i) {
                    if (allTransactions.data[i].initPlayer == props.paidUsername && allTransactions.data[i].hostPlayer == props.hostUsername || allTransactions.data[i].initPlayer == props.hostUsername && allTransactions.data[i].hostPlayer == props.paidUsername) {
                        paid_id = allTransactions.data[i].paymentId;
                        return;
                    }
                }
                var reqBody = {
                    "amount" : price,
                    "currency" : "US",
                    "payment_id": paid_id
                };

                await axios.post(`/playerPayment/${props.paidUsername}/Refund`, reqBody);

                var res = pollForData("refund.status.updated", props.paidUsername);

                if (res.data.status.contains("DECLINED")) {
                    setError(true);
                    setErrorMsg("Error: Couldn't process refund");
                    return;
                }

                setSuccessfull(true);
            } catch (err) {
                setError(true);
                setErrorMsg(err);
                return;
            }
        };

        processRefund();
    };
};



