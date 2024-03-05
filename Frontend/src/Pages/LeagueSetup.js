import { useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const LeagueSetup = (username) => {
    const [loggedIn, username] = useAuth();

    const handleSubmit = (event) => {
        event.preventDefault();

        
    };
};

export default LeagueSetup;