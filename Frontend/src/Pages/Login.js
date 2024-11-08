import { useRef, useState, useEffect, useContext } from 'react';
import axios from '../Api';
import { useNavigate } from 'react-router-dom'; 
import AuthContext from "../AuthProvider";

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [success, setSuccess] = useState(false);

    const { setAuth, login } = useContext(AuthContext);
    const userRef = useRef();
    const errRef = useRef();
    const navigate = useNavigate(); 

    useEffect(() => {
        userRef.current.focus();
    }, []);

    useEffect(() => {
        setErrorMessage('');
    }, [username, password]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const loginInfo = { username, password };
            const response = await axios.post("/login", loginInfo);
            
            // Assuming you have setAuth and login functions to update the context state
            setAuth({ username, password });
            login(username, password);

            setUsername('');
            setPassword('');
            setSuccess(true);

            // Redirect to the desired path after successful login
            navigate('/');  // Redirect to home or another path
            
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

    return (
        <section>
            <p ref={errRef} className={errorMessage ? "errmsg" : "offscreen"} aria-live="assertive">
                {errorMessage}
            </p>
            {success ? (
                <p>Logged in successfully!</p>
            ) : (
                <form onSubmit={handleSubmit}>
                    <label htmlFor="username">Username:</label>
                    <input
                        type="text"
                        id="username"
                        ref={userRef}
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                    <button type="submit">Login</button>
                </form>
            )}
        </section>
    );
};

export default Login;
