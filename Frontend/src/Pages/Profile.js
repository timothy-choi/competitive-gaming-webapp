import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';
import { useParams } from 'react-router-dom';
import AuthContext from '../AuthProvider';

const Profile = () => {
    let { username } = useParams();

    const { auth } = useContext(AuthContext);

    const isMyProfile = auth.username === username;

    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [dateJoined, setDateJoined] = useState(null);
    const [isAvailable, setIsAvailable] = useState(false);
    const [playingGame, setPlayingGame] = useState(false);

    const [leagueJoined, setLeagueJoined] = useState(false);
    const [leagueName, setLeagueName] = useState('');

    const [playerFriends, setPlayerFriends] = useState([]);
    const [playerFriendCount, setPlayerFriendCount] = useState(null);

    const [playerRecord, setPlayerRecord] = useState([]);

    const [regularGames, setRegularGames] = useState([]);
    const [seasonLeagueGames, setSeasonLeagueGames] = useState([]);
    const [playoffLeagueGames, setPlayoffLeagueGames] = useState([]);

    const [errorMessage, setErrorMessage] = useState('');

    const initUser = async () => {
        try {
            const response = await axios.get(`/Players/${username}`);

            setName(response.data.playerName);
            setEmail(response.data.playerEmail);
            setDateJoined(response.data.playerJoined);
            setIsAvailable(response.data.playerAvailable);
            setPlayingGame(response.data.playerInGame);
            setPlayerFriendCount(response.data.playerFriends.length);
            setLeagueJoined(response.data.leagueJoined);
            setLeagueName(response.data.playerLeagueJoined);
            setPlayerRecord(response.data.singlePlayerRecord);
            var allFriends = [];
            for (let i = 0; i < response.data.playerFriends.length; ++i) {
                var friend = await axios.get(`/Players/${response.data.playerFriends[i]}`);
                var friendInfo = {
                    name: friend.data.name,
                    username: friend.data.username,
                    isAvailable: friend.data.playerAvailable,
                    playingGame: friend.data.playerInGame
                };
                allFriends.push(friendInfo);
            }
            setPlayerFriends(allFriends);

            var allGames = await axios.get(`singleGame/${username}`);
            var regularGames = [];
            var seasonGames = [];
            var playoffGames = [];            
            for (let j = 0; j < allGames.length; ++j) {
                if (leagueJoined) {
                    const leagues = await axios.get("/Leagues");

                    var added = false;
                    var leagueId = "";
                    for (var league in leagues) {
                        if (league.Name == leagueName) {
                            leagueId = league.LeagueId;
                            break;
                        }
                    }

                    const leagueInfo = await axios.get(`/League/${leagueId}`);

                    const user_games = await axios.get(`/LeagueSeasonAssignments/${leagueInfo.data.SeasonAssignments}/${username}`);

                    const foundGame = user_games.find(game => game.SingleGameId === allGames[j].SingleGameId);

                    if (foundGame) {
                        added = true;
                        var gameInfo = {
                            gameId : allGames[j].SingleGameId,
                            hostPlayer : allGames[j].hostPlayer,
                            guestPlayer : allGames[j].guestPlayer,
                            finalScore : allGames[j].finalScore,
                            recentScore : allGames[j].recentScore.length > 0 ? allGames[j].recentScore[allGames[j].recentScore.length-1] : null,
                            timePlayed : new Date().toLocaleString() > allGames[j].timePlayed ? null : allGames[j].timePlayed
                        }
                        seasonGames.push(gameInfo);
                    }
                    else {
                        const playoffInfo = await axios.get(`/LeaguePlayoffs/${leagueinfo.PlayoffAssignments}`);

                        var singleBracket = false;
                        if (playoffInfo.WholeMode) {
                            singleBracket = true;
                        }

                        var bracketName = playoffInfo.FinalPlayoffBracket.SubPlayoffBrackets[0].playoffName;

                        const playoffTrail = await axios.get(`${playoffInfo.LeaguePlayoffId}/${username}/${singleBracket}/${bracketName}/PlayoffRunTrail`);

                        var rd = 1;
                        for (var round in playoffTrail) {
                            for (var game in round.GameId) {
                                if (game == allGames[j].SingleGameId) {
                                    const gameInfo = await axios.get(`/SingleGame/${allGames[j].SingleGameId}`);
                                    var playoffGameInfo = {
                                        round: rd,
                                        gameId: allGames[j].SingleGameId,
                                        hostPlayer : gameInfo.hostPlayer,
                                        guestPlayer : gameInfo.guestPlayer,
                                        finalScore : gameInfo.finalScore,
                                        winner: gameInfo.finalScore[0] > gameInfo.finalScore[1] ? hostPlayer : guestPlayer,
                                        recentScore : gameInfo.recentScore.length > 0 ? gameInfo.recentScore[allGames[j].recentScore.length-1] : null,
                                        timePlayed : new Date().toLocaleString() > gameInfo.timePlayed ? null : gameInfo.timePlayed
                                    }
                                    if (round.GameId.length > 1) {
                                        playoffGameInfo.gameNumber = round.GameId.indexOf(game) + 1;
                                    }
                                    
                                    playoffGames.push(playoffGameInfo);
                                }
                            }
                            rd++;
                        }

                        added = true;

                    }
                }  

                if (!added) {
                    var gameInfo = {
                        gameId : allGames[j].SingleGameId,
                        hostPlayer : allGames[j].hostPlayer,
                        guestPlayer : allGames[j].guestPlayer,
                        finalScore : allGames[j].finalScore,
                        recentScore : allGames[j].recentScore.length > 0 ? allGames[j].recentScore[allGames[j].recentScore.length-1] : null,
                        timePlayed : new Date().toLocaleString() > allGames[j].timePlayed ? null : allGames[j].timePlayed
                    }
                    regularGames.push(gameInfo);
                }
            }

            regularGames.sort(game => game.timePlayed);
            seasonGames.sort(game => game.timePlayed);
            playoffGames.sort(game => game.timePlayed);

            setRegularGames(regularGames);
            setSeasonLeagueGames(seasonGames);
            setPlayoffLeagueGames(playoffGames);

        } catch (e) {
            setErrorMessage(`${username} doesn't exist`);
        }
    }

    useEffect(() => {
        initUser();
    }, []);

}

export default Profile;