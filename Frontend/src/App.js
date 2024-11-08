import { BrowserRouter, Routes, Route } from "react-router-dom";
import React from "react";
import Register from "./Pages/Register";
import Login from "./Pages/Login";

function App() {
            return (
                <div className="App">
                <Routes>
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                </Routes>
                </div>
        );
  }
  
  export default App;