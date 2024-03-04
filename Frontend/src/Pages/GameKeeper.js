import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';

const GameKeeper = (streamInfo) => {
    const { gameId, editor, seasonMode, leagueIdValue } = useParams();

    const [hostPlayer, setHostPlayer] = useState('');
    const [hostScore, setHostScore] = useState(0);

    const [guestPlayer, setGuestPlayer] = useState('');
    const [guestScore, setGuestScore] = useState(0);

    const [finalScore, setFinalScore] = useState([]);

    const [inGameScores, setInGameScores] = useState([]);

    const [inGameInfo, setInGameInfo] = useState([]);

    const [streamLink, setStreamLink] = useState('');

    const [errorMessage, setErrorMessage] = useState('');

    const [forceUpdate, setForceUpdate] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            const gameInfo = await axios.get(`/SingleGame/${gameId}`);

            setInGameScores(gameInfo.data.inGameScores);

            setHostPlayer(gameInfo.data.hostPlayer);

            if (Date.now >= gameInfo.data.timePlayed) {
                if (inGameScores.length > 0) {
                    setHostScore(inGameScores[inGameScores.length-1][0]);
                    setGuestScore(inGameScores[inGameScores.length-1][1]);
                }
                else {
                    setHostScore(0);
                    setGuestScore(0);
                }
            }

            setGuestPlayer(gameInfo.data.guestPlayer);

            setInGameInfo(gameInfo.data.inGameInfo);

            setStreamLink(streamInfo["stream_url"]);
        };

        fetchData();
    }, []);

    useEffect(() => {
        setErrorMessage('');
    }, [hostScore, guestScore]);

    const handleSubmitInGameScores = (event) => {
        event.preventDefault();

        setHostScore(event.target.host_score.value);

        setGuestScore(event.target.guest_score.value);

        var inGameScore = [event.target.score_type.value, [event.target.host_score.value, event.target.guest_score.value]];

        var currScores = inGameScores;
        currScores.push(inGameScore);
        setInGameScores(currScores);

        const addInGameScore = async () => {
            try {
                const scoreInfo = {
                    gameId : gameId,
                    gameScoreType : event.target.score_type.value,
                    guestScore : event.target.guest_score.value,
                    hostScore : event.target.host_score.value 
                };
                const res = await axios.post("/SingleGame/AddInGameScore", scoreInfo);
    
            } catch (e) {
                setErrorMessage('Could not add in game score');
            }
        };

        addInGameScore();
    };

    const handleSubmitFinalScore = (event) => {
        event.preventDefault();

        setHostScore(event.target.final_host_score.value);

        setGuestScore(event.target.final_guest_score.value);

        var finalScoreValue = [event.target.final_host_score.value, event.target.final_guest_score.value];

        setFinalScore(finalScoreValue);

        const addFinalScore = async () => {
            try {
                const scoreInfo = {
                    gameId : gameId,
                    guestPoints : event.target.final_guest_score.value,
                    hostPoints : event.target.final_host_score.value 
                };
                const res = await axios.post("/SingleGame/finalScore", scoreInfo);
    
            } catch (e) {
                setErrorMessage('Could not add final score');
            }
        };

        addFinalScore();

    };

    const handleSubmitGameInfo = (event) => {
        event.preventDefault();

        var allGameInfo = inGameInfo;

        allGameInfo.push(event.target.currGameInfo.value);

        setInGameInfo(allGameInfo);

        const addGameInfo = async () => {
            try {
                const data = {
                    gameId : gameId,
                    gameInfo : event.target.currGameInfo.value
                };

                const res = await axios.get("/SingleGame/OtherGameInfo", data);

            } catch (e) {
                setErrorMessage('Could not add in game score');
            }
        };

        addGameInfo();
    };

    const handleSubmitMetricsInfo = (event) => {
        event.preventDefault();

        const updateMetrics = async () => {
            const hostInfo = await axios.get(`/Player/${hostPlayer}`);

            const guestInfo = await axios.get(`/Player/${guestPlayer}`);

            var record_status_host = 0;
            var record_status_guest = 0;

            if (hostScore > guestScore) {
                record_status_host = 1;
                record_status_guest = -1;
            } else if (guestScore > hostScore) {
                record_status_host = -1;
                record_status_guest = 1;
            }

            const leagueInfo = await axios.get(`/League/${leagueIdValue}`);

            const metrics = await axios.get(`/Config/${leagueInfo.data.LeagueConfig}`);

            metrics = metrics.data.otherMetrics;

            var metric_size = event.target.value.length;

            var hostRecord = {
                playerId : hostInfo.data.playerId,
                recordStatus : record_status_host
            };

            for (let i = 0; i < metric_size; ++i) {
                hostRecord[`${metrics[i]}`] = event.target['host${metrics[i]}'].value;
            }

            try {
                const res = await axios.post(`/League/Record/${leagueIdValue}`, hostRecord);
            } catch (e) {
                setErrorMessage('Could not update standings');
            }

            var guestRecord = {
                playerId : guestInfo.data.playerId,
                recordStatus : record_status_guest
            };

            for (let i = 0; i < metric_size; ++i) {
                guestRecord[`${metrics[i]}`] = event.target['guest${metrics[i]}'].value;
            }

            try {
                const res2 = await axios.post(`/League/Record/${leagueIdValue}`, guestRecord);
            } catch (e) {
                setErrorMessage('Could not update standings');
            }
        };

        updateMetrics();
    };

    useEffect(() => {
        const timeout = setTimeout(() => {
          setForceUpdate(prevState => !prevState);
        }, 70000); 
    
        return () => clearTimeout(timeout);
    }, []);
};

export default GameKeeper;