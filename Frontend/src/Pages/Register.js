import { useRef, useState, useEffect } from "react";
import axios from './api/axios';

const Register = () => {
    const userRef = useRef();
    const errRef = useRef();

    const [name, setName] = useState('');

    const [email, setEmail] = useState('');

    const [username, setUsername] = useState('');
    const [validUsername, setValidUsername] = useState(false);
    const [usernameFocus, setUsernameFocus] = useState(false);

    const [password, setPassword] = useState('');
    const [validPassword, setValidPassword] = useState(false);
    const [passwordFocus, setPasswordFocus] = useState(false);

    const [matchPassword, setMatchPassword] = useState('');
    const [validMatch, setValidMatch] = useState(false);
    const [matchFocus, setMatchFocus] = useState(false);

    const [errorMessage, setErrorMessage] = useState('');
    const [success, setSuccess] = useState(false);

    useEffect(() => {
        userRef.current.focus();
    }, [])


    useEffect(() => {
        setErrMsg('');
    }, [name, email, username, password, matchPassword])


    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const userAuthInfo = {
                username: username,
                password: password,
                retype_password: matchPassword
            };
            const response = await axios.post("/register", userInfo);

            //clear state and controlled input
            setPassword('');
            setMatchPassword('');
        } catch (e) {
            if (!err?.response) {
                setErrorMessage('No Server Response');
            } else if (err.response?.status === 409) {
                setErrorMessage('Username Taken');
            } else {
                setErrorMessage('Registration Failed')
            }
            errRef.current.focus();
        }

        try {
            const userInfo = {
                name: name,
                playerUsername: username,
                playerEmail: email
            };

            const response = await axios.post("Players/", userInfo);

            setSuccess(true);
            setUsername('');
            setName('');
            setEmail('');

        } catch (e) {
            if (!err?.response) {
                setErrorMessage('No Server Response');
            } else if (err.response?.status === 409) {
                setErrorMessage('Username Taken');
            } else {
                setErrorMessage('Registration Failed')
            }
            errRef.current.focus();
        }
    }

    return ()
}