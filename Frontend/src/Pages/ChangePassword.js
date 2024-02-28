import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';

const password_regex = /^(?=.*\d)(?=.*[a-zA-Z])(?=.*[!@#$%^&*()_+])[a-zA-Z0-9!@#$%^&*()_+]{8,}$/;

const changePassword = () => {
    const [username, setUsername] = useState('');

    const [newPassword, setNewPassword] = useState('');
    const [validPassword, setValidPassword] = useState(false);
    const [passwordFocus, setPasswordFocus] = useState(false);

    const [matchPassword, setMatchPassword] = useState('');
    const [validMatch, setValidMatch] = useState(false);
    const [matchFocus, setMatchFocus] = useState(false);

    const [errorMessage, setErrorMessage] = useState('');
    const [success, setSuccess] = useState(false);

    const passwordRef = useRef();
    const errorRef = useRef();

    useEffect(() => {
        passwordRef.current.focus();
    }, [])

    useEffect(() => {
        setValidPassword(password_regex.test(password));
        setValidMatch(password === matchPassword);
    }, [newPassword, matchPassword])

    useEffect(() => {
        setErrMsg('');
    }, [username, newPassword, matchPassword])

    const handleSubmit = async (e) => {
        e.preventDefault();

        const check = password_regex.test(newPassword);
        if (!check) {
            setErrorMessage('Invalid Password');
            return;
        }

        try {
            const response = await axios.put(`/playerAuth/${username}/${newPassword}/${matchPassword}`);

            setCurrentPassword('');
            setNewPassword('');
            setMatchPassword('');
            setSuccess(true);

        } catch (e) {
            if (!e?.response) {
                setErrorMessage('No Server Response');
            } else if (e.response?.status === 409) {
                setErrorMessage('Username does not exist');
            } else {
                setErrorMessage('Change password failed')
            }
            errorRef.current.focus();
        }

    }

}

export default changePassword;