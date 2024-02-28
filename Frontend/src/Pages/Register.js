import { useRef, useState, useEffect } from "react";
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 

const regex = /^[a-zA-Z0-9_.]+$/;

const Register = () => {
    const userRef = useRef();
    const errRef = useRef();
    const history = useHistory();

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
        setValidUsername(regex.test(username));
    }, [username])


    useEffect(() => {
        setErrMsg('');
    }, [name, email, username, password, matchPassword])


    const handleSubmit = async (e) => {
        e.preventDefault();

        const check = regex.test(username);
        if (!check) {
            setErrorMessage('Invalid Username');
            return;
        }
        try {
            const userAuthInfo = {
                username: username,
                password: password,
                retype_password: matchPassword
            };
            const response = await axios.post("/register", userAuthInfo);

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

        var playerId = '';

        try {
            const userInfo = {
                name: name,
                playerUsername: username,
                playerEmail: email
            };

            const response = await axios.post("Players/", userInfo);

            playerId = response?.data;

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

        try {
            const playerEntry = {
                name: name,
                username: username,
                playerId: playerId
            };

            const response = await axios.post("Search/Players/", playerEntry);

            setSuccess(true);
            setUsername('');
            setName('');

            history.push('/Login');
        } catch (e) {
            if (!err?.response) {
                setErrorMessage('No Server Response');
            } else {
                setErrorMessage('Registration Failed')
            }
            errRef.current.focus();
        }
    }

    return ()
}

export default Register;