import { React, useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";


const Home = () => {
    const {loggedIn, username } = useAuth();

    const [friendsGames, setFriendsGames] = useState([]);

    const [currentGame, setCurrentGame] = useState(null);

    const [currentSeasonUpcomingGame, setCurrentSeasonUpcomingGame] = useState(null);

    const [currentSeasonOtherGames, setCurrentSeasonOtherGames] = useState([]);

    const [currentPlayoffGame, setPlayoffUpcomingGame] = useState(null);

    const [currentPlayoffOtherGames, setCurrentPlayoffOtherGames] = useState([]);

    const [currentFriendSeasonGames, setCurrentFriendSeasonGames] = useState([]);

    const [currentFriendPlayoffGames, setCurrentFriendPlayoffGames] = useState([]);

    const [query, setQuery] = useState('');

    const [errorMessage, setErrorMessage] = useState('');

    const history = useHistory();

    useEffect(() => {
        const getPlayerLeague = async (player) => {
            const leagues = await axios.get(`/League/`);
            const playerLeague = await axios.get(`/Player/${player}`);

            for (var league in leagues.data) {
                if (league.Name == playerLeague.leagueName) {
                    return league;
                }
            }

            return null;
        };


        const getSeasonGamesInLeague = async (player, assignmentsId) => {
            const res = await axio.get(`/LeagueSeasonAssignments/${assignmentsId}/${player}/PlayerSchedule`);

            return res.data["schedule"];
        };

        const isInBracket = async (player, playoffsId) => {
            const playoffs = await axios.get(`/LeaguePlayoffs/${playoffsId}`);

            for (var bracket in playoffs.FinalFullBracket.SubPlayoffBrackets) {
                for (var head in bracket.PlayoffHeadMatchups) {
                    if (head.Player1 == player || head.Player2 == player) {
                        return bracket.playoffName;
                    }
                }
            }

            return null;
        }

        const getPlayoffGamesInLeague = async (player, playoffsId) => {
            const playoffs = await axios.get(`/LeaguePlayoffs/${playoffsId}`);

            var single = true;
            var bracketName = playoffs.FinalFullBracket.SubPlayoffBrackets[0].playoffName;
            if (playoffs.FinalFullBracket.SubPlayoffBrackets.length > 1) {
                single = false;
                var res = isInBracket(player, playoffsId);
                if (res) {
                    bracketName = res;
                }
                else {
                    return [];
                }
            }

            var trail = await axios.get(`/LeaguePlayoffs/${playoffsId}/${player}/${single}/${bracketName}/PlayoffRunTrail`);

            var playoffGames = [];
            for (var round in trail) {
                playoffGames.push([round.round, round.GameId]);
            }

            return playoffGames;
        };

        const processUserGames = async (username) => {
            const res = await axios.get(`/SingleGame/${username}`);

            var regGames = [];
            var seasonGames = [];
            var playoffGames = [];

            var league = await getPlayerLeague(username);

            var seasonGameList = [];

            var playoffGameList = [];

            if (league) {
                seasonGameList = await getSeasonGamesInLeague(username, league.SeasonAssignments);

                playoffGameList = await getPlayoffGamesInLeague(username, league.PlayoffAssignments);
            }

            for (var game in res.data) {
                if (league) {
                    if (seasonGameList.find(gameEntry => gameEntry.gameId == game.gameId) != null) {
                        seasonGames.push(game);
                    }
                    if (playoffGameList.length > 0) {
                        for (var rd in playoffGameList) {
                            if (rd[1].find(gameEntry => gameEntry == game.gameId) != null) {
                                playoffGames.push([rd[0], game, rd[1].indexOf(game)]);
                            }
                        }
                    }
                } else {
                    regGames.push(game);
                }
            }

            regGames.sort((a, b) => {return a.timePlayed - b.timePlayed});
            if (league) {
                seasonGames.sort((a, b) => {return a.timePlayed - b.timePlayed});

                const nowTime = Date.now();
                const newSeasonList = seasonGames.filter(gameEntry => {
                    return nowTime > gameEntry.timePlayed; 
                });

                const gamesSixHoursAfterCurrentTime = seasonGames.filter(game => game.timePlayed > currentTime && game.timePlayed <= currentTime + (6 * 60 * 60 * 1000));

                seasonGames = newSeasonList.concat(gamesSixHoursAfterCurrentTime);

                playoffGames.sort((a, b) => {return a[1].timePlayed - b[1].timePlayed});

            }

            return {
                regularGames : regGames,
                seasonGames : seasonGames,
                playoffGames : playoffGames
            };
        }

        const fetchData = async () => {
            if (!loggedIn) {
                return;
            }
            var res = processUserGames(username);

            setCurrentGame(res.regGames[res.regGames.length-1]);

            if (league) {
                setCurrentSeasonUpcomingGame(res.seasonGames[res.seasonGames.length-1]);
                if (res.playoffGames.length > 0) {
                    setPlayoffUpcomingGame(res.playoffGames[res.playoffGames.length-1]);
                }
            }

            var playerInfo = await axios.get(`/Player/${username}`);

            var friends = playerInfo.playerFriends;

            for (var friend in friends) {
                var friendGameList = processUserGames(friend);

                var leagueName = await getPlayerLeague(friend);

                var friends_matches = friendsGames;

                friends_matches.push(friendGameList.regGames[friendGameList.regGames.length-1]);

                setFriendsGames(friends_matches);

                if (friendGameList.seasonGames.length > 0) {
                    var currentFriendSeasonGamesCopy = currentFriendSeasonGames;
                    currentFriendSeasonGamesCopy.push([leagueName, friendGameList.seasonGames[friendGameList.seasonGames.length-1]]);
                    setCurrentFriendSeasonGames(currentFriendSeasonGamesCopy);
                }
                if (friendGameList.playoffGames.length > 0) {
                    var currentFriendPlayoffGamesCopy = currentFriendPlayoffGames;
                    currentFriendPlayoffGamesCopy.push([leagueName, friendGameList.playoffGames[friendGameList.playoffGames.length-1]]);
                    setCurrentFriendPlayoffGames(currentFriendPlayoffGamesCopy);
                }
            }

            var leagueInfo = await getPlayerLeague(username);

            var leaguePlayers = leagueInfo.Players;

            for (var player in leaguePlayers) {
                const playerSchedule = await getSeasonGamesInLeague(player, leagueInfo.SeasonAssignments);

                playerSchedule.sort((a, b) => {return a.timePlayed - b.timePlayed});
                var pastGames = playerSchedule.filter(gameEntry => {
                    return nowTime > gameEntry.timePlayed; 
                });
                var upcomingGames = playerSchedule.filter(game => game.timePlayed > currentTime && game.timePlayed <= currentTime + (6 * 60 * 60 * 1000));

                var scheduleList = pastGames.concat(upcomingGames);

                var currentSeasonOtherGamesCopy = currentSeasonOtherGames;

                currentSeasonOtherGamesCopy.push(scheduleList[scheduleList.length-1]);

                setCurrentSeasonOtherGames(currentSeasonOtherGamesCopy);

                var userPlayoffRun = await getPlayoffGamesInLeague(player, leagueInfo.PlayoffAssignments);

                if (userPlayoffRun.length > 0) {
                    var lastPlayoffGame = await axios.get(`/SingleGame/${userPlayoffRun[userPlayoffRun.length-1][1][userPlayoffRun[userPlayoffRun.length-1][1].length-1].gameId}`);
                    var entry = [userPlayoffRun[userPlayoffRun.length-1][0], userPlayoffRun[userPlayoffRun.length-1][1].length, lastPlayoffGame.data];
                    var currentPlayoffOtherGamesCopy = currentPlayoffOtherGames;

                    currentPlayoffOtherGamesCopy.push(entry);

                    setCurrentPlayoffOtherGames(currentPlayoffOtherGamesCopy);
                }
            }

            for (var seasonGame in currentSeasonOtherGames) {
                if (seasonGame.gameId == currentSeasonUpcomingGame.gameId) {
                    var temp = currentSeasonOtherGames;
                    temp = temp.filter(game => game.gameId == seasonGame.gameId);
                    setCurrentSeasonOtherGames(temp);
                    break;
                }
            }

            for (var playoffGame in currentPlayoffOtherGames) {
                if (playoffGame[2].gameId == currentPlayoffGame.gameId) {
                    var temp = currentPlayoffOtherGames;
                    temp = temp.filter(game => game.gameId == playoffGame[2].gameId);
                    setCurrentPlayoffOtherGames(temp);
                    break;
                }
            }

            for (var seasonGame in currentFriendSeasonGames) {
                if (seasonGame[1].gameId == currentSeasonUpcomingGame.gameId) {
                    var temp = currentFriendSeasonGames;
                    temp = temp.filter(game => game.gameId == seasonGame[1].gameId);
                    setCurrentSeasonOtherGames(temp);
                    break;
                }
            }

            for (var playoffGame in currentFriendPlayoffGames) {
                if (playoffGame[1][2].gameId == currentPlayoffGame.gameId) {
                    var temp = currentFriendPlayoffGames;
                    temp = temp.filter(game => game.gameId == playoffGame[1][2].gameId);
                    setCurrentPlayoffOtherGames(temp);
                    break;
                }
            }
            
            for (var regularGame in friendsGames) {
                if (regularGame.gameId == currentGame.gameId) {
                    var temp = friendsGames;
                    temp = temp.filter(game => game.gameId == regularGame.gameId);
                    setFriendsGames(temp);
                    break;
                }
            }
        };

        fetchData();
    }, []);

    useEffect(() => {
        const updateInGameScore = async () => {
            const res = await axios.get(`/data/AddInGameScore/app`);

            const gameId = res.data.substring(0, res.data.indexof("_"));
            var score = res.data.substring(res.data.indexof("_") + 1).split(",");
            score = score.split(",");
            try {
                score[0] = parseInt(score[0].substring(0));
                score[1] = parseInt(score[1].substring(0, score[1].indexof(")")));
            } catch (e) {
                setErrorMessage('Could not get in game score');
            }

            if (currentGame.gameId == gameId) {
                var temp = currentGame;
                temp.hostScore = score[0];
                temp.guestScore = score[1];
                setCurrentGame(temp);
            }
            else if (currentSeasonUpcomingGame.gameId == gameId) {
                var temp = currentSeasonUpcomingGame;
                temp.hostScore = score[0];
                temp.guestScore = score[1];
                setCurrentSeasonUpcomingGame(temp);
            }
            else if (currentPlayoffGame.gameId == gameId) {
                var temp = currentPlayoffGame;
                temp.hostScore = score[0];
                temp.guestScore = score[1];
                setPlayoffUpcomingGame(temp);
            }
            else if (friendsGames.find(game => game.gameId == gameId) != null) {
                var temp = friendsGames;
                var foundGame = temp.find(game => game.gameId == gameId);
                foundGame.hostScore = score[0];
                foundGame.guestScore = score[1];
                setFriendsGames(temp);
            }
            else {
                if (currentFriendSeasonGames.find(game => game[1].gameId == gameId) != null) {
                    var temp = currentFriendSeasonGames;
                    var foundGame = temp.find(game => game[1].gameId == gameId);
                    foundGame.hostScore = score[0];
                    foundGame.guestScore = score[1];
                    setCurrentFriendSeasonGames(temp);
                }
                if (currentSeasonOtherGames.find(game => game.gameId == gameId) != null) {
                    var temp = currentSeasonOtherGames;
                    var foundGame = temp.find(game => game.gameId == gameId);
                    foundGame.hostScore = score[0];
                    foundGame.guestScore = score[1];
                    setCurrentSeasonOtherGames(temp);
                }
                if (currentFriendPlayoffGames.find(game => game[1][2].gameId == gameId) != null) {
                    var temp = currentFriendPlayoffGames;
                    var foundGame = temp.find(game => game[1][2].gameId == gameId);
                    foundGame.hostScore = score[0];
                    foundGame.guestScore = score[1];
                    setCurrentFriendSeasonGames(temp);
                }
                if (currentPlayoffOtherGames.find(game => game.gameId == gameId) != null) {
                    var temp = currentPlayoffOtherGames;
                    var foundGame = temp.find(game => game.gameId == gameId);
                    foundGame.hostScore = score[0];
                    foundGame.guestScore = score[1];
                    setCurrentSeasonOtherGames(temp);
                }
            }
        };

        updateInGameScore();
    }, []);

};

export default Home;