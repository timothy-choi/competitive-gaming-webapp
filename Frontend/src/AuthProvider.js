import { createContext, useState } from "react";

const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {
    const [auth, setAuth] = useState({});

    const login = (username, password) => {
        setAuth({ username, password });
      };
    
      const logout = () => {
        setAuth({});
      };

    return (
        <AuthContext.Provider value={{ auth, setAuth, login, logout}}>
            {children}
        </AuthContext.Provider>
    )
}

export default AuthContext;