import { React, useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const SearchResults = (query) => {
    const {loggedIn, username } = useAuth();

    const [playerResults, setPlayerResults] = setState([]);

    const [gameResults, setGameResults] = setState([]);

    const [leagueResults, setLeagueResults] = setState([]);

    useEffects(() => {

    }, []);

    const handleSearchSubmit = (event) => {
        event.preventDefault();
    };


};

export default SearchResults;