import { React, useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useLocation, useHistory} from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const SearchResults = () => {
    const {loggedIn, username } = useAuth();

    const location = useLocation();

    const history = useHistory();

    const [query, setQuery] = useState('');

    const [playerResults, setPlayerResults] = setState([]);

    const [gameResults, setGameResults] = setState([]);

    const [leagueResults, setLeagueResults] = setState([]);

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const q = params.get('query');

        const fetchAllResults = async (q) => {
            const playerRes = await axios.get(`/Search/Player/${q}`);

            const gameRes = await axios.get(`/Search/Game/${q}`);

            const leagueRes = await axios.get(`/Search/League/${q}`);

        };

        if (q) {
            setQuery(q);

            fetchAllResults(q);
        }
    }, [location.search]);

    const handleSearchSubmit = (event) => {
        event.preventDefault();

        history.push(`/SearchResults?query=${encodeURIComponent(query)}`);
    };
};

export default SearchResults;