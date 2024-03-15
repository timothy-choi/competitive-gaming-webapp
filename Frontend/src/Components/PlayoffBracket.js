import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';

const PlayoffBracket = (leagueId, leaguePlayoffId) => {
    const [playoffBracket, setPlayoffBracket] = useState(null);

    const [forceUpdate, setForceUpdate] = useState(false);

    useEffect(() => {
        var fetchBracket = async () => {
            const league = await axios.get(`/League/${leagueId}`);

            const playoffs = await axios.get(`/LeaguePlayoffs/${leaguePlayoffId}`);

            const bracket = await axios.get(`/LeaguePlayoffs/${leaguePlayoffId}/PlayoffBracket`);

            setPlayoffBracket(bracket);
        };

        fetchBracket();
    }, []);

    useEffect(() => {
    
    }, []);

    useEffect(() => {
        const timeout = setTimeout(() => {
          setForceUpdate(prevState => !prevState);
        }, 70000); 
    
        return () => clearTimeout(timeout);
    }, []);
};

export default PlayoffBracket;