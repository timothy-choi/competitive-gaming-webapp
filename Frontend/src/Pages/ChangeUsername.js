import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';

const regex = /^[a-zA-Z0-9_.]+$/;

const changeUsername = () => {
    const [currentUsername, setCurrentUsername] = useState('');
    const [newUsername, setNewUsername] = useState('');
    const [success, setSuccess] = useState(false);
    const [errorMessage, setErrorMessage] = useState(false);

    const usernameRef = useRef();
    const errRef = useRef();

    useEffect(() => {
        usernameRef.current.focus();
    }, [])

    useEffect(() => {
        setNewUsername(regex.text(newUsername))
    }, [newUsername])

    useEffect(() => {
        setErrorMessage('');
    }, [currentUsername, newUsername])


    const handleSubmit = (e) => {
        e.preventDefault();

        const check = regex.test(newUsername);
        if (!check) {
            setErrorMessage('Invalid Username');
            return;
        }

        try {
            const response = axios.put(`/playerAuth/${currentUsername}/${newUsername}`);

            setSuccess(true);
            setCurrentUsername('');
            setNewUsername('');


        } catch (e) {
            if (!e?.response) {
                setErrorMessage('No Server Response');
            } else if (e.message == "Username does not exist") {
                setErrorMessage('Username does not exist');
            } else if (e.message == "New username already exists") {
                setErrorMessage('New Username does already exist');
            }
            else {
                setErrorMessage(`Couldn't update username`);
            }

            errRef.current.focus();
        }

    };
}

export default changeUsername;