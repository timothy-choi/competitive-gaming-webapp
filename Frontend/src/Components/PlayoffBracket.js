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
        var updateSeries = async () => {
            var res = await axios.get(`/data/UpdateMatchupSeries/${leaguePlayoffId}`);

            setPlayoffBracket(JSON.parse(res.data));
        };

        updateSeries();
    }, []);

    useEffect(() => {
        var updatePlayoffRound = async () => {
            
        };

        updatePlayoffRound();
    }, []);

    useEffect(() => {
        var updateInGameScore = async () => {

        };

        updateInGameScore();
    }, []);

    useEffect(() => {
        var updateFinalScore = async () => {

        };

        updateFinalScore();
    }, []);

    useEffect(() => {
        var addInNextGame = async () => {

        };

        addInNextGame();
    }, []);

    useEffect(() => {
        const timeout = setTimeout(() => {
          setForceUpdate(prevState => !prevState);
        }, 70000); 
    
        return () => clearTimeout(timeout);
    }, []);
};

export default PlayoffBracket;