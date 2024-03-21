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
            var res = await axios.get(`/data/UpdatePlayoffBracket/${leagueId}`);

            setPlayoffBracket(JSON.parse(res.data));
        };

        updatePlayoffRound();
    }, []);

    useEffect(() => {
        var updateFinalPlayoffRound = async () => {
            var res = await axios.get(`/data/UpdateFinalRounds/${leaguePlayoffId}`);

            setPlayoffBracket(JSON.parse(res.data));
        };

        updateFinalPlayoffRound();
    }, []);

    useEffect(() => {
        var addInFinalRounds = async () => {
            var res = await axios.get(`/data/SetupFinalRounds/${leaguePlayoffId}`);

            setPlayoffBracket(JSON.parse(res.data));
        };

        addInFinalRounds();
    }, []);

    useEffect(() => {
        var updateInGameScore = async () => {
            var res = await axios.get(`/data/AddInGameScore/app`);

            var temp = await axios.get(`/LeaguePlayoffs/${leaguePlayoffId}/PlayoffBracket`);

            setPlayoffBracket(temp.data);
        };

        updateInGameScore();
    }, []);

    useEffect(() => {
        var updateFinalScore = async () => {
            var res = await axios.get(`/data/UpdateSingleGameFinalScore/app`);

            var temp = await axios.get(`/LeaguePlayoffs/${leaguePlayoffId}/PlayoffBracket`);

            setPlayoffBracket(temp.data);
        };

        updateFinalScore();
    }, []);

    useEffect(() => {
        var addInNextGame = async () => {
            var res = await axios.get(`/data/AddGameToMatchup/${leaguePlayoffId}`);

            var temp = await axios.get(`/LeaguePlayoffs/${leaguePlayoffId}/PlayoffBracket`);

            setPlayoffBracket(temp.data);
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