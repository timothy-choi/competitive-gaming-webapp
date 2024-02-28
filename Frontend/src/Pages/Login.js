import { useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import AuthContext from "./context/AuthProvider";

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [success, setSuccess] = useState(false);

    const { setAuth, login } = useContext(AuthContext);
    const userRef = useRef();
    const errRef = useRef();
    const history = useHistory(); 

    useEffect(() => {
        userRef.current.focus();
    }, [])

    useEffect(() => {
        setErrMsg('');
    }, [user, pwd])

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const loginInfo = {
                username: username,
                password: password
            }
            const response = await axios.post("/login", loginInfo);
            setAuth({ username, password});

            login(username, password);

            setUsername('');
            setPassword('');
            setSuccess(true);

            history.push('');

        } catch (err) {
            if (!err?.response) {
                setErrorMessage('No Server Response');
            } else if (err.response?.status === 400) {
                setErrorMessage('Bad Request');
            } else if (err.response?.status === 401) {
                setErrorMessage('Unauthorized');
            } else {
                setErrorMessage('Login Failed');
            }
            errRef.current.focus();
        }
    };

    return ()

}

export default Login;