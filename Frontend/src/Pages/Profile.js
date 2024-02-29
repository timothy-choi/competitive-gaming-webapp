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
    const [playerFriendRemoved, setPlayerFriendRemoved] = useState(false);

    const [playerRecord, setPlayerRecord] = useState([]);
    const [playerSeasonRecord, setPlayerSeasonRecord] = useState([]);

    const [regularGames, setRegularGames] = useState([]);
    const [seasonLeagueGames, setSeasonLeagueGames] = useState([]);
    const [archieveSeasonGames, setArchieveSeasonGames] = useState([]);
    const [playoffLeagueGames, setPlayoffLeagueGames] = useState([]);
    const [archievePlayoffGames, setArchievePlayoffGames] = useState([]);

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

            if (leagueJoined) {
                var leagueId = '';
                const leagues = await axios.get("/League/");
                for (var league in leagues) {
                    if (league.Name == leagueName) {
                        leagueId = league.LeagueId;
                        break;
                    }
                }
                const seasonRecord = await axios.get(`/League/${leagueId}/${response.data.playerId}`);
                var record = [seasonRecord.data["wins"], seasonRecord.data["losses"], seasonRecord.data["draws"]];
                setPlayerSeasonRecord(record);
            }

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

    useEffect(() => {
        if (!playerFriendRemoved) {
            const getNewFriend = async () => {
                const player = await axios.get(`/Player/${username}`);
    
                const response = await axios.get(`/data/addPlayerFriend/${player.data.playerId}`);
    
                player = await axios.get(`/Player/${response.data}`);
    
                return player.data;
            }
    
            var playerInfo = getNewFriend();
    
            var playerEntry = {
                name: playerInfo.name,
                username: playerInfo.username,
                isAvailable: playerInfo.playerAvailable,
                playingGame: playerInfo.playerInGame
            }
    
            var friends = playerFriends;
            friends.push(playerEntry);
    
            setPlayerFriends(friends);
            setPlayerFriendCount(friends.length);

            //impl notification that user was added as friend
        }
        else {
            const getFriendIndex = async () => {
                const player = await axios.get(`/Player/${auth.username}`);
    
                const response = await axios.get(`/data/RemoveFromFriendsList/${player.data.playerId}`);
    
                player = await axios.get(`/Player/${response.data}`);
    
                return parseInt(player.data);
            };
    
            var index = getFriendIndex();
            var friends = playerFriends;
            friends.splice(index, 1);
    
            setPlayerFriends(friends);
            setPlayerFriendCount(friends.length);
        }
    }, [playerFriendChanged]);

    useEffect(() => {
        const getNewAvailability = async () => {
            const player = await axios.get(`/Player/${auth.username}`);

            const availability = await axios.get(`/data/ChangedAvailableStatus/${player.playerId}`);

            return availability.data;
        };

        var newAvailability = getNewAvailability();
        if (newAvailability) {
            setIsAvailable(true);
        }
        else {
            setIsAvailable(false);
        }
    }, [isAvailable]);

    useEffect(() => {
        const getNewGameStatus = async () => {
            const player = await axios.get(`/Player/${auth.username}`);

            const playingGameStatus = await axios.get(`/data/ChangedGameStatus/${player.playerId}`);

            return playingGameStatus.data;
        };

        var newGameStatus = getNewGameStatus();
        if (newGameStatus) {
            setPlayingGame(true);
            setIsAvailable(false);
        }
        else {
            setPlayingGame(false);
        }
    }, [playingGame]);

    useEffect(() => {
        const getAnyFriendsAvailabilityStatus = async () => {
            const playingAvailability = await axios.get(`/data/ChangedAvailabilityStatus/app`);

            const user = playingAvailability.substring(0, playingAvailability.indexOf('_'));

            var newFriendsList = playerFriends;

            const found = newFriendsList.find(player => player.username == user);

            if (!found) {
                return [];
            }

            var availability = playingAvailability.substring(playingAvailability.indexOf('_') + 1);

            found.isAvailable = availability == "available" ? true : false;

            return newFriendsList;
        }

        var res = getAnyFriendsAvailabilityStatus();

        if (res.length > 0) {
            setPlayerFriends(res);
        }

    }, [playerFriends]);

    useEffect(() => {
        const getAnyFriendsGameStatus = async () => {
            const playingGameFriends = await axios.get(`/data/ChangedAvailabilityStatus/app`);

            const user = playingGameFriends.substring(0, playingGameFriends.indexOf('_'));

            var newFriendsList = playerFriends;

            const found = newFriendsList.find(player => player.username == user);

            if (!found) {
                return [];
            }

            var gameStatus = playingGameFriends.substring(playingGameFriends.indexOf('_') + 1);

            found.playerInGame = gameStatus == "Playing in game" ? true : false;
            found.isAvailable = gameStatus != "Playing in game" ? false : true;

            return newFriendsList;
        };

        var res = getAnyFriendsGameStatus();

        if (res.length > 0) {
            setPlayerFriends(res);
        }

    }, [playerFriends]);

    useEffect(() => {
        const getPlayerRecord = async () => {
            const player = await axios.get(`/Player/${username}`);
    
            const response = await axios.get(`/data/UpdatePlayerRecord/${player.data.playerId}`);

            const record = response.data.split(",");

            return record;
        };

        var playerRecord = getPlayerRecord();

        setPlayerRecord(playerRecord);
    }, [playerRecord]);

    useEffect(() => {
        var changedRegular = false;
        var changedSeason = false;
        var changedPlayoffs = false;
        const updatePlayingScores = async () => {
            const response = await axios.get(`/data/AddInGameScore/app`);

            const game_id = response.data.substring(0, response.data.indexof('_'));

            const score = response.data.substring(response.data.indexof('_')+1).split(',');

            var regularGameCopy = regularGames;

            var foundGame = regularGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.recentScore.push(score);
                changedRegular = true;
                return regularGameCopy;
            }

            var seasonGameCopy = seasonLeagueGames;

            foundGame = seasonGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.recentScore.push(score);
                changedSeason = true;
                return seasonGameCopy;
            }

            var playoffGameCopy = playoffLeagueGames;

            foundGame = playoffGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.recentScore.push(score);
                changedPlayoffs = true;
                return playoffGameCopy;
            }

            return [];
        };

        var updatedGames = updatePlayingScores();

        if (changedRegular) {
            setRegularGames(updatedGames);
        }
        else if (changedSeason) {
            setSeasonLeagueGames(updatedGames);
        }
        else if (changedPlayoffs) {
            setPlayoffLeagueGames(updatedGames);
        }

    }, [regularGames, seasonLeagueGames, playoffLeagueGames]);

    useEffect(() => {
        var changedRegular = false;
        var changedSeason = false;
        var changedPlayoffs = false;

        const addFinalScore = async () => {
            const response = await axios.get(`/data/UpdateSingleGameFinalScore/app`);

            const game_id = response.data.substring(0, response.data.indexof('_'));

            const final_score = response.data.substring(response.data.indexof('_')+1).split(',');

            const final_score_processed = [parseInt(final_score[0]), parseInt(final_score[1])];

            var regularGameCopy = regularGames;

            var foundGame = regularGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.finalScore = final_score_processed;
                changedRegular = true;
                return regularGameCopy;
            }

            var seasonGameCopy = seasonLeagueGames;

            foundGame = seasonGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.finalScore = final_score_processed;
                changedSeason = true;
                return seasonGameCopy;
            }

            var playoffGameCopy = playoffLeagueGames;

            foundGame = playoffGameCopy.find(game => game.gameId == game_id);

            if (foundGame) {
                foundGame.finalScore = final_score_processed;
                changedPlayoffs = true;
                return playoffGameCopy;
            }
        };

        var updatedGames = addFinalScore();

        if (changedRegular) {
            setRegularGames(updatedGames);
        }
        else if (changedSeason) {
            setSeasonLeagueGames(updatedGames);
        }
        else if (changedPlayoffs) {
            setPlayoffLeagueGames(updatedGames);
        }

    }, [regularGames, seasonLeagueGames, playoffLeagueGames]);

    useEffect(() => {
        const archieveGame = async () => {
            try {
                const response = await axios.get("/data/ArchievePlayerSchedules/app");

                archieveSeasonGames.push(seasonLeagueGames);
                setArchieveSeasonGames(archieveSeasonGames);
            } catch {
                setErrorMessage(`Couldn't archieve games.`);
            }

            archieveGame();
        }
    }, [archieveSeasonGames]);

    useEffect(() => {
        const archieveGame = async () => {
            try {
                const response = await axios.get("/data/ArchievePlayoffs/app");

                archievePlayoffGames.push(playoffLeagueGames);
                setArchievePlayoffGames(archievePlayoffGames);
            } catch {
                setErrorMessage(`Couldn't archieve games.`);
            }

            archieveGame();
        }
    }, [archievePlayoffGames]);

    useEffect(() => {
        const updateSeasonRecord = async () => {
            var LeagueId = "";
            const leagues = await axios.get("/Leagues");
            for (var league in leagues) {
                if (league.Name == leagueName) {
                    LeagueId = league.LeagueId;
                    break;
                }
            }
            const response = await axios.get(`/data/UpdateStandings/${LeagueId}`);

            var leagueStandings = JSON.parse(response.data[0]);

            var record = leagueStandings.find(player => player["name"] == username);

            return [record["record"][0], record["record"][1], record["record"][2]];
        };

        var updatedRecord = updateSeasonRecord();

        setPlayerSeasonRecord(updatedRecord);
        
    }, [playerSeasonRecord]);
}

export default Profile;