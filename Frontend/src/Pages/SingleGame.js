import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';
import { useParams } from 'react-router-dom';


const SingleGame = () => {
    const { gameId } = useParams();

    const [seasonMode, setSeasonMode] = useState(false);
    const [league, setLeague] = useState('');

    const [playoffMode, setPlayoffMode] = useState(false);
    const [playoffRound, setPlayoffRound] = useState(null);
    const [playoffSeriesGame, setPlayoffSeriesGame] = useState(null);

    const [hostPlayer, setHostPlayer] = useState('');
    const [hostScore, setHostScore] = useState(null);
    const [hostSeasonRecord, setHostSeasonRecord] = useState([]);
    const [hostSeriesPlayoffRecord, setHostSeriesPlayoffRecord] = useState([]);

    const [guestPlayer, setGuestPlayer] = useState('');
    const [guestScore, setGuestScore] = useState(null);
    const [guestSeasonRecord, setGuestSeasonRecord] = useState([]);
    const [guestSeriesPlayoffRecord, setGuestSeriesPlayoffRecord] = useState([]);

    const [inGameScores, setInGameScores] = useState([]);

    const [finalScore, setFinalScore] = useState([]);

    const [inGameInfo, setInGameInfo] = useState([]);

    const [gameTime, setGameTime] = useState(null);

    const [predictionId, setPredictionId] = useState('');
    const [predictionOn, setPredictionOn] = useState(false);

    const [gameEditor, setGameEditor] = useState('');

    const [twitchBroadcasterId, setTwitchBroadcasterId] = useState('');

    const [streamLink, setStreamLink] = useState('');

    const [videoFilePath, setVideoFilePath] = useState('');


    useEffect(() => {

    }, []);









};

export default SingleGame;