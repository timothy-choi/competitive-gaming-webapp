import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';
import { json, useParams } from 'react-router-dom';


const SingleGame = () => {
    const { gameId } = useParams();

    const [seasonMode, setSeasonMode] = useState(false);
    const [league, setLeague] = useState('');

    const [playoffMode, setPlayoffMode] = useState(false);
    const [playoffRound, setPlayoffRound] = useState('');
    const [playoffSeriesGame, setPlayoffSeriesGame] = useState(null);
    const [seriesWinner, setSeriesWinner] = useState(bool);

    const [hostPlayer, setHostPlayer] = useState('');
    const [hostScore, setHostScore] = useState(null);
    const [hostSeasonRecord, setHostSeasonRecord] = useState([]);

    const [guestPlayer, setGuestPlayer] = useState('');
    const [guestScore, setGuestScore] = useState(null);
    const [guestSeasonRecord, setGuestSeasonRecord] = useState([]);


    const [SeriesPlayoffRecord, setSeriesPlayoffRecord] = useState([]);

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
        const fetchGame = async () => {
            const gameInfo = await axios.get(`/SingleGame/${gameId}`);

            setHostPlayer(gameInfo.data.hostPlayer);
            setGuestPlayer(gameInfo.data.guestPlayer);

            var listStr = JSON.stringify(gameInfo.data.inGameScores);

            setInGameScores(JSON.parse(listStr));

            setGameTime(gameInfo.data.timePlayed);

            if (gameInfo.data.finalScore != null) {
                setFinalScore(gameInfo.data.finalScore);
                setHostScore(finalScore[0]);
                setGuestScore(finalScore[1]);
            }
            else if (Date.now >= gameTime) {
                if (inGameScores.length > 0) {
                    setHostScore(inGameScores[inGameScores.length-1][1][0]);
                    setGuestScore(inGameScores[inGameScores.length-1][1][1]);
                }
                else {
                    setHostScore(0);
                    setGuestScore(0);
                }
            }

            setGameEditor(gameInfo.data.gameEditor);

            setInGameInfo(gameInfo.data.otherGameInfo);

            setPredictionId(gameInfo.data.predictionId);

            setPredictionOn(true);

            const playerInfo = await axios.get(`/Player/${hostPlayer}`);

            const guestInfo = await axios.get(`/Player/${guestPlayer}`);

            var leagueId = "";

            if (playerInfo.data.leagueJoined) {
                var seasonAssignments = '';
                var playoffAssignments = '';
                const leagues = await axios.get(`/League/`);
                for (var league in leagues) {
                    if (league.Name == playerInfo.playerLeagueJoined) {
                        seasonAssignments = league.SeasonAssignments;
                        playoffAssignments = league.PlayoffAssignments;
                        leagueId = league.LeagueId;
                        break;
                    }
                }

                const seasonSchedule = await axios.get(`/LeagueSeasonAssignments/${seasonAssignments}/FinalFullSchedule`);

                const foundGame = seasonSchedule.find(game => game.gameId = gameId);

                if (foundGame != null) {
                    setSeasonMode(true);
                    setLeague(playerInfo.playerLeagueJoined);

                    const hostPlayerRecord = await axios.get(`/League/${leagueId}/${playerInfo.data.playerId}`);

                    setHostSeasonRecord([hostPlayerRecord.data["wins"], hostPlayerRecord.data["losses"], hostPlayerRecord.data["draws"]]);

                    const guestPlayerRecord = await axios.get(`/League/${leagueId}/${guestPlayer.playerId}`);

                    setGuestSeasonRecord([guestPlayerRecord.data["wins"], guestPlayerRecord.data["losses"], guestPlayerRecord.data["draws"]]);
                }
                else {
                    const playoffBracket = await axios.get(`/LeaguePlayoffs/${playoffAssignments}`);

                    var allBrackets = playoffBracket.SubPlayoffBrackets;

                    var found = false;

                    var rd = 2;

                    for (var bracket in allBrackets) {
                        for (var head in bracket.PlayoffHeadMatchups) {
                            if (head.currentPlayoffMatchup.GameId.find(game => game == gameId) != null) {
                                setPlayoffMode(true);
                                setPlayoffRound("1");
                                if (head.currentPlayoffMatchup.GameId.length > 1) {
                                    var index = head.currentPlayoffMatchup.GameId.findIndex(game => game == gameId);
                                    setPlayoffSeriesGame(index);
                                    var rec = [0,0];
                                    for (let i = 0; i < index + 1; ++i) {
                                        const g = await axios.get(`/SingleGame/${head.currentPlayoffMatchup.GameId[i]}`);
                                        if (g.finalScore != null) {
                                            if (finalScore[0] > finalScore[1]) {
                                                rec[0]++;
                                            }
                                            else {
                                                rec[1]++;
                                            }
                                        }
                                    }
                                    setSeriesPlayoffRecord(rec);
                                }
                                found = true;
                                break;
                            }
                        }
                        if (found) {
                            break;
                        }

                        for (var matchup in bracket.AllOtherMatchups) {
                            if (matchup[0] != rd) {
                                rd++;
                            }
                            if (matchup[1].currentPlayoffMatchup.GameId.find(game => game == gameId) != null) {
                                setPlayoffMode(true);
                                setPlayoffRound(rd.toString());
                                if (matchup[1].currentPlayoffMatchup.GameId.length > 1) {
                                    var index = matchup[1].currentPlayoffMatchup.GameId.findIndex(game => game == gameId);
                                    setPlayoffSeriesGame(index);
                                    var rec = [0,0];
                                    for (let i = 0; i < index + 1; ++i) {
                                        const g = await axios.get(`/SingleGame/${head.currentPlayoffMatchup.GameId[i]}`);
                                        if (g.finalScore != null) {
                                            if (finalScore[0] > finalScore[1]) {
                                                rec[0]++;
                                            }
                                            else {
                                                rec[1]++;
                                            }
                                        }
                                    }
                                    setSeriesPlayoffRecord(rec);
                                }
                                found = true;
                                break;
                            }
                        }

                        if (found) {
                            break;
                        }

                        rd = 2;
                    }

                    var initCt = allBrackets.length;

                    var ct = 0;

                    var finalRound = 1;

                    for (var finalMatch in playoffBracket.FinalRoundMatchups) {
                        if (ct == initCt) {
                            initCt /= 2;
                            ct = 0;
                            finalRound++;
                        }
                        if (finalMatch.currentPlayoffMatchup.GameId.find(game => game == gameId) != null) {
                            setPlayoffMode(true);
                            if (ct == 2) {
                                setPlayoffRound("Semifinals");
                            }
                            else if (ct == 1) {
                                setPlayoffRound("Championship");
                            }
                            else {
                                setPlayoffRound("Final Round " + finalRound);
                            }
                            if (finalMatch.currentPlayoffMatchup.GameId.length > 1) {
                                var index = finalMatch.currentPlayoffMatchup.GameId.findIndex(game => game == gameId);
                                setPlayoffSeriesGame(index);
                                var rec = [0,0];
                                for (let i = 0; i < index + 1; ++i) {
                                    const g = await axios.get(`/SingleGame/${head.currentPlayoffMatchup.GameId[i]}`);
                                    if (g.finalScore != null) {
                                        if (finalScore[0] > finalScore[1]) {
                                            rec[0]++;
                                        }
                                        else {
                                            rec[1]++;
                                        }
                                    }
                                }
                                setSeriesPlayoffRecord(rec);
                            }
                            break;
                        }
                        ct++;
                    }
                }
            }
        };

        fetchGame();

    }, []);


    useEffect(() => {
        const getNewGameInfo = async () => {
            const res = await axios.get(`/data/AddOtherGameInfo/${gameId}`);

            return res.data;
        };

        var gameInfo = getNewGameInfo();

        var newGameInfo = inGameInfo;

        newGameInfo.push(gameInfo);

        setInGameInfo(newGameInfo);

    }, [inGameInfo]);

    useEffect(() => {
        const endPrediction = async () => {
            if (((TimeSpan) (gameTime - Date.Now)).TotalSeconds <= 60) {
                try {
                    const endPred = await axios.patch("/twitchId/prediction/end");
                    setPredictionOn(false);
                    setPredictionId('');
                } catch (e) {
                    setErrorMessage(`Couldn't end prediction`);
                }
            }
        }

        endPrediction();
    }, [predictionId, predictionOn]);

    useEffect(() => {
        const getGameScore = async () => {
            const newScore = await axios.get(`/data/AddInGameScore/${gameId}`);

            var scoreInfo = newScore.data.substring(newScore.data.indexof("(") + 1, newScore.data.indexof(")")).split(",");

            var scoreEntry = [newScore.data.substring(0, newScore.data.indexof(",")), scoreInfo];

            setHostScore(scoreInfo[0]);
            setGuestScore(scoreInfo[1]);

            var newScores = inGameScores;

            newScores.push(scoreEntry);

            setInGameScores(newScores);
        }

        getGameScore();

    }, [inGameScores, hostScore, guestScore]);

    useEffect(() => {
        const addFinalScore = async () => {
            const res = await axios.get(`/data/UpdateSingleGameFinalScore/${gameId}`);

            var finalScoreInfo = [res.data.substring(0, res.data.indexof(",")), res.data.substring(res.data.indexof(",")+1)];
            setHostScore(finalScoreInfo[0]);
            setGuestScore(finalScoreInfo[1]);

            setFinalScore(finalScoreInfo);

            if (playoffMode) {
                var record = SeriesPlayoffRecord;
                if (guestScore > hostScore) {
                    record[1]++; 
                }
                else {
                    record[0]++;
                }
                setSeriesPlayoffRecord(record);

                var seasonConfig = "";

                var leagueName = league;

                const leagues = await axios.get(`/League/`);
                for (var league in leagues) {
                    if (league.Name == leagueName) {
                        seasonConfig = league.LeagueConfig;
                        break;
                    }
                }

                const config = await axios.get(`/LeagueConfig/${seasonConfig}`);

                if (config.data.PlayoffSeries) {
                    var num_games = config.data.GamesPerRound[playoffRound-1];

                    if (record[0] == num_games || record[1] == num_games) {
                        setSeriesWinner(true);
                    }
                }
            }
            else if (seasonMode) {
                var tempHost = hostSeasonRecord;
                var tempGuest = guestSeasonRecord;
                if (hostScore > guestScore) {
                    tempHost[0]++;
                    tempGuest[1]++;
                }
                else if (guestScore > hostScore) {
                    tempHost[1]++;
                    tempGuest[0]++;
                }
                else {
                    tempHost[2]++;
                    tempGuest[2]++;
                }
                setHostSeasonRecord(tempHost);
                setGuestSeasonRecord(tempGuest);
            }
        }

        addFinalScore();
    }, [HostScore, GuestScore, finalScore, hostSeasonRecord, guestSeasonRecord, SeriesPlayoffRecord]);


};

export default SingleGame;