import { createContext, useState } from "react";

const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {
    const [auth, setAuth] = useState({});
    const [loggedIn, setLoggedIn] = useState(false);
    const [username, setUsername] = useState('');

    const login = (username, password) => {
        setAuth({ username, password });
        setLoggedIn(true);
        setUsername(username);
      };
    
      const logout = () => {
        setAuth({});
        setLoggedIn(false);
      };

    return (
        <AuthContext.Provider value={{ auth, setAuth, login, logout, loggedIn, username}}>
            {children}
        </AuthContext.Provider>
    )
}

export default AuthContext;

export const useAuth = () => useContext(AuthContext);