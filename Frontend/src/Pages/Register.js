import { useRef, useState, useEffect } from "react";
import axios from '../Api';
import { useNavigate } from 'react-router-dom'; 

// Regular expressions for validation
const USER_REGEX = /^[a-zA-Z0-9_.]{3,20}$/; // Adjusted to enforce length
const PASSWORD_REGEX = /^(?=.*\d)(?=.*[a-zA-Z])(?=.*[!@#$%^&*()_+])[a-zA-Z0-9!@#$%^&*()_+]{8,}$/;

const Register = () => {
    const userRef = useRef();
    const errRef = useRef();
    const history = useNavigate();

    // Form state
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

    // Focus on username input on component mount
    useEffect(() => {
        userRef.current.focus();
    }, [])

    // Validate username
    useEffect(() => {
        setValidUsername(USER_REGEX.test(username));
    }, [username])

    // Validate password and match
    useEffect(() => {
        setValidPassword(PASSWORD_REGEX.test(password));
        setValidMatch(password === matchPassword);
    }, [password, matchPassword])

    // Clear error message when user modifies input
    useEffect(() => {
        setErrorMessage('');
    }, [name, email, username, password, matchPassword])

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validate inputs before sending
        const isUsernameValid = USER_REGEX.test(username);
        const isPasswordValid = PASSWORD_REGEX.test(password);

        if (!isUsernameValid) {
            setErrorMessage('Invalid Username. It should be 3-20 characters and can include letters, numbers, underscores, and periods.');
            userRef.current.focus();
            return;
        }

        if (!isPasswordValid) {
            setErrorMessage('Invalid Password. It must be at least 8 characters long and include a number, a letter, and a special character.');
            return;
        }

        try {
            // Registration API call
            const userAuthInfo = {
                username: username,
                password: password,
                retype_password: matchPassword
            };
            const response = await axios.post("/register", userAuthInfo);

            // Assuming the registration was successful
            // Clear password fields
            setPassword('');
            setMatchPassword('');
        } catch (e) {
            if (!e.response) {
                setErrorMessage('No Server Response');
            } else if (e.response.status === 409) {
                setErrorMessage('Username Taken');
            } else {
                setErrorMessage('Registration Failed');
            }
            errRef.current.focus();
            return; // Exit the function if registration fails
        }

        let playerId = '';

        try {
            // Add player information
            const userInfo = {
                name: name,
                playerUsername: username,
                playerEmail: email
            };
            const response = await axios.post("/Players/", userInfo);

            playerId = response.data.playerId; // Adjust based on actual response structure

            setEmail('');
        } catch (e) {
            if (!e.response) {
                setErrorMessage('No Server Response');
            } else if (e.response.status === 409) {
                setErrorMessage('Username Taken');
            } else {
                setErrorMessage('Registration Failed');
            }
            errRef.current.focus();
            return; // Exit the function if adding player info fails
        }

        try {
            // Add player entry to search
            const playerEntry = {
                name: name,
                username: username,
                playerId: playerId
            };
            await axios.post("/Search/Players/", playerEntry);

            setSuccess(true);
            setUsername('');
            setName('');
            setEmail('');

            // Redirect to login page
            history('/Login');
        } catch (e) {
            if (!e.response) {
                setErrorMessage('No Server Response');
            } else {
                setErrorMessage('Registration Failed');
            }
            errRef.current.focus();
        }
    }

    return (
        <section className="register">
            <p ref={errRef} className={errorMessage ? "errmsg" : "offscreen"} aria-live="assertive">
                {errorMessage}
            </p>
            {success ? (
                <div className="success">
                    <h1>Success!</h1>
                    <p>
                        <a href="/Login">Sign In</a>
                    </p>
                </div>
            ) : (
                <form onSubmit={handleSubmit}>
                    <h1>Register</h1>
                    <label htmlFor="name">
                        Name:
                    </label>
                    <input
                        type="text"
                        id="name"
                        ref={userRef}
                        autoComplete="off"
                        onChange={(e) => setName(e.target.value)}
                        value={name}
                        required
                    />

                    <label htmlFor="email">
                        Email:
                    </label>
                    <input
                        type="email"
                        id="email"
                        autoComplete="off"
                        onChange={(e) => setEmail(e.target.value)}
                        value={email}
                        required
                    />

                    <label htmlFor="username">
                        Username:
                        <span className={validUsername ? "valid" : "invalid"}>
                            {validUsername ? "✔" : "✖"}
                        </span>
                    </label>
                    <input
                        type="text"
                        id="username"
                        ref={userRef}
                        autoComplete="off"
                        onChange={(e) => setUsername(e.target.value)}
                        value={username}
                        required
                        aria-invalid={validUsername ? "false" : "true"}
                        aria-describedby="uidnote"
                        onFocus={() => setUsernameFocus(true)}
                        onBlur={() => setUsernameFocus(false)}
                    />
                    <p id="uidnote" className={usernameFocus && username && !validUsername ? "instructions" : "offscreen"}>
                        3 to 20 characters.<br />
                        Letters, numbers, underscores, and periods allowed.
                    </p>

                    <label htmlFor="password">
                        Password:
                        <span className={validPassword ? "valid" : "invalid"}>
                            {validPassword ? "✔" : "✖"}
                        </span>
                    </label>
                    <input
                        type="password"
                        id="password"
                        onChange={(e) => setPassword(e.target.value)}
                        value={password}
                        required
                        aria-invalid={validPassword ? "false" : "true"}
                        aria-describedby="pwdnote"
                        onFocus={() => setPasswordFocus(true)}
                        onBlur={() => setPasswordFocus(false)}
                    />
                    <p id="pwdnote" className={passwordFocus && !validPassword ? "instructions" : "offscreen"}>
                        8 characters minimum.<br />
                        Must include a letter, a number, and a special character.
                    </p>

                    <label htmlFor="confirm_password">
                        Confirm Password:
                        <span className={validMatch && matchPassword ? "valid" : "invalid"}>
                            {validMatch && matchPassword ? "✔" : "✖"}
                        </span>
                    </label>
                    <input
                        type="password"
                        id="confirm_password"
                        onChange={(e) => setMatchPassword(e.target.value)}
                        value={matchPassword}
                        required
                        aria-invalid={validMatch ? "false" : "true"}
                        aria-describedby="confirmnote"
                        onFocus={() => setMatchFocus(true)}
                        onBlur={() => setMatchFocus(false)}
                    />
                    <p id="confirmnote" className={matchFocus && !validMatch ? "instructions" : "offscreen"}>
                        Must match the first password input field.
                    </p>

                    <button
                        type="submit"
                        disabled={!validUsername || !validPassword || !validMatch ? true : false}
                    >
                        Sign Up
                    </button>
                </form>
            )}
        </section>
    )
}

export default Register;